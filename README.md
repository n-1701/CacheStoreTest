# NAS Neuron — Claims Engine POC

A claims-validation app for **NAS NEURON Health Services**. The backend turns an editable rule
set into a **GoRules ZEN** decision model, publishes it to object storage, and evaluates claims
against the live ZEN Agent. The frontend is the operator console.

> **Security model:** the ZEN access token and the iDrive e2 (S3) credentials live **only** on the
> backend. The browser talks exclusively to the .NET API; it never sees the token or storage keys.

---

## Run in Visual Studio 2022 (no Node required)

The Angular UI is **pre-compiled** and bundled into the backend's `wwwroot`. The .NET app serves
the UI and the API together on one port, so **nothing JavaScript runs on your machine** — no `npm`,
no Node, nothing for group policy / AppLocker to block. You only need the **.NET 8 SDK** and
Visual Studio 2022.

1. **File → Open → Project/Solution** and choose `NasNeuron.ClaimsEngine.sln`.
2. (Optional, needed only to *save rules*) Open
   `backend/NasNeuron.ClaimsApi/Properties/launchSettings.json` and paste your iDrive e2 keys into
   the empty `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY` values.
3. Press **F5**.

The browser opens to `http://localhost:5080` and the whole app — UI plus API — runs from there.
Members and Claims work immediately on the seeded sample data. Swagger is at `/swagger`.

> **Changing the UI later:** the frontend source lives in `frontend/` for reference, but editing it
> requires rebuilding the Angular bundle (`ng build`), which needs Node — so do that on a machine
> where Node is allowed, then copy `frontend/dist/frontend/browser/*` into
> `backend/NasNeuron.ClaimsApi/wwwroot/`. The backend logic (rules, ZEN, storage) is pure C# and you
> can iterate on it freely in Visual Studio.

---

## Architecture

```
Angular SPA  ──HTTP──>  .NET 8 Web API  ──S3 PUT──>  iDrive e2 bucket (gorules-poc)
(operator UI)           (rules + claims)                      │
                              │                                │ polls every ~5s
                              └────────── evaluate ───────────> GoRules ZEN Agent (Render)
                                       (X-Access-Token)         hot-reloads on change
```

- **Medical Cases** — the rule set. Editing a rule rebuilds the JDM, repackages the zip, and
  uploads it to the bucket. The agent hot-reloads within ~5s.
- **Members** — directory with a detail panel.
- **Claims** — history plus a *New claim* modal that evaluates against the live ruleset.

---

## Prerequisites

- **.NET 8 SDK** (this is all you need to run the app)
- iDrive e2 (S3-compatible) credentials with write access to the `gorules-poc` bucket
  (only needed to *save* rules)
- The ZEN Agent reachable at the configured base URL (only needed to *evaluate* claims)
- **Node.js 20+** — **not** required to run; only needed if you want to rebuild the UI from source

---

## Running the backend

```bash
cd backend/NasNeuron.ClaimsApi

# storage credentials (required for saving rules)
export AWS_ACCESS_KEY_ID=your_idrive_access_key
export AWS_SECRET_ACCESS_KEY=your_idrive_secret_key

dotnet run
```

API: `http://localhost:5080` · Swagger (dev): `http://localhost:5080/swagger` · Health: `/health`

Non-secret config lives in `appsettings.json` and can be overridden by environment variables
(double-underscore syntax):

| Setting | Default | Env override |
|---|---|---|
| `Zen:AgentBaseUrl` | `https://agent-latest-jl93.onrender.com` | `Zen__AgentBaseUrl` |
| `Zen:AccessToken` | `nnhs-poc-token` | `Zen__AccessToken` |
| `S3:ServiceUrl` | `https://s3.eu-west-3.idrivee2.com` | `S3__ServiceUrl` |
| `S3:Bucket` | `gorules-poc` | `S3__Bucket` |
| `S3:ObjectKey` | `claim_validation.zip` | `S3__ObjectKey` |
| `Cors:Origins` | `http://localhost:4200` | `Cors__Origins__0` |
| (S3 credentials) | — | `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` |

## Rebuilding the frontend (optional)

You don't need this to run the app — the compiled UI already ships in
`backend/NasNeuron.ClaimsApi/wwwroot`. Only if you change the Angular source, on a machine where
Node is allowed:

```bash
cd frontend
npm install
ng build
# then copy dist/frontend/browser/* into ../backend/NasNeuron.ClaimsApi/wwwroot/
```

The production build calls the API at the relative path `/api` (`src/environments/environment.ts`),
so it works served same-origin by the backend with no CORS or proxy needed.

---

## API

| Method | Path | Purpose |
|---|---|---|
| GET | `/api/rules` | List rules |
| GET | `/api/rules/{code}` | Get one rule |
| GET | `/api/rules/jdm` | Current JDM document (preview) |
| PUT | `/api/rules/{code}` | Save a rule → rebuild JDM → zip → upload (hot-reload) |
| GET | `/api/members` | List members |
| GET | `/api/members/{id}` | Member detail |
| GET | `/api/claims` | Claims history |
| POST | `/api/claims/evaluate` | Evaluate a claim via the ZEN Agent (optionally records it) |

---

## The save-rule flow

`PUT /api/rules/{code}` runs entirely server-side:

1. Update/insert the rule in the in-memory set (the `PASS` catch-all stays last).
2. `JdmBuilder` serializes the full set into a ZEN decision-table graph
   (`inputNode → decisionTableNode → outputNode`, hit policy **first**).
3. `ZipPackager` builds the bundle in memory:
   ```
   claim_validation.zip
   ├── .config/project.json
   └── claim_validation.json
   ```
4. `S3Uploader` PUTs the zip to `gorules-poc`, overwriting `claim_validation.zip`
   (path-style addressing, payload signing disabled for S3-compatible storage).
5. The ZEN Agent polls the bucket and hot-reloads within ~5s.

If credentials are missing the endpoint returns **503** with a clear message; an upload failure
returns **502**. The UI surfaces both in the publish pipeline.

---

## The rule set

Evaluated top-to-bottom, first match wins:

| Code | Condition | Decision |
|---|---|---|
| `T01` | Treatment date older than 1 year | rejected |
| `T02` | Treatment date outside the coverage window (2025-07-01 → 2026-06-30) | rejected |
| `G01` | Treatment country excluded, **or** not in the included list | rejected |
| `M01` | `gender = male` AND `claimType = maternity` | rejected |
| `M02` | `claimType = pediatric` AND `age > 17` | rejected |
| `W01` | `claimType = dental` AND `age > 65` | **warning** |
| `PASS` | catch-all | approved |

`G01` exposes editable **included** and **excluded** country collections in the UI; editing them
and saving changes evaluation immediately. `T02` exposes editable from/to dates.

### ZEN expression dialect

`JdmBuilder` emits these ZEN Expression Language cells. The backend injects `today` into the
evaluation context so date rules are deterministic:

- **T01:** `date(today) - date(treatmentDate) > duration("365d")`
- **T02:** `date(treatmentDate) < date("2025-07-01") or date(treatmentDate) > date("2026-06-30")`
- **G01:** `country in ["North Korea","Syria"] or not (country in ["UAE","Saudi Arabia", …])`
- **M01/M02/W01:** unary tests in the bound columns (`gender`, `age`, `claimType`),
  e.g. gender cell `"male"`, age cell `> 17`.
- Output cells are quoted ZEN string literals, e.g. `"rejected"`.

> **Tune against your agent build.** Exact date/duration semantics vary slightly between ZEN
> engine versions. `GET /api/rules/jdm` returns the exact document being produced — diff it against
> what your agent expects and adjust the expressions in `Services/JdmBuilder.cs` if a rule misfires.

---

## Notes / caveats

- Rules, members, and claims are held **in memory** and reset on backend restart (POC scope).
  Swap `RuleStore` / `ClaimStore` / `SeedData` for a database when productionizing.
- The logo is a **placeholder slot** in the sidebar (`AppComponent`); drop the official mark in and
  replace the placeholder block.
- This repository was assembled in an environment without the .NET SDK, so the backend was **not
  compiled here**; build it with `dotnet build` on your machine. The frontend TypeScript was
  type-checked.
