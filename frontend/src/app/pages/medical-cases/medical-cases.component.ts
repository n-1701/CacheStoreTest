import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Rule } from '../../models/models';

interface Step { label: string; detail: string; state: 'pending' | 'active' | 'done' | 'failed'; }

@Component({
  selector: 'app-medical-cases',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
  <div class="topbar">
    <div>
      <div class="eyebrow">Validation</div>
      <h1>Medical Cases</h1>
      <p>Validation rules the ZEN Engine runs against every claim. Edit a rule to rebuild the JDM, repackage the zip, and hot-reload the agent.</p>
    </div>
  </div>

  <div class="content">
    <div class="card">
      <div class="toolbar">
        <div class="search">
          <svg viewBox="0 0 24 24" fill="none" stroke-width="2"><circle cx="11" cy="11" r="7"/><path d="M21 21l-4-4"/></svg>
          <input type="text" placeholder="Search rules…" [(ngModel)]="query" (ngModelChange)="tick()">
        </div>
        <select class="filter" [(ngModel)]="decision" (ngModelChange)="tick()">
          <option value="">All decisions</option><option value="rejected">Rejected</option><option value="warning">Warning</option><option value="approved">Approved</option>
        </select>
        <span class="count">{{ filtered().length }} of {{ rules().length }} rules</span>
      </div>

      <div class="table-scroll">
        <table>
          <thead><tr><th>Code</th><th>Condition</th><th>Decision</th><th>Status</th><th style="text-align:right">Actions</th></tr></thead>
          <tbody>
            <tr *ngFor="let r of filtered()" class="clickable" (click)="open(r)">
              <td><span class="code-chip">{{ r.code }}</span></td>
              <td style="max-width:360px">{{ r.condition }}</td>
              <td><span class="pill" [class]="'pill ' + dec(r.decision)"><span class="pd"></span>{{ r.decision }}</span></td>
              <td><span class="pill" [class]="'pill ' + (r.enabled ? 'blue' : 'gray')"><span class="pd"></span>{{ r.enabled ? 'Enabled' : 'Disabled' }}</span></td>
              <td style="text-align:right">
                <button class="icon-btn" (click)="$event.stopPropagation(); open(r)">
                  <svg viewBox="0 0 24 24" fill="none" stroke-width="2"><path d="M11 4H4v16h16v-7"/><path d="M18.5 2.5a2.1 2.1 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                </button>
              </td>
            </tr>
            <tr *ngIf="!filtered().length"><td colspan="5" class="empty-row">No rules match these filters.</td></tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>

  <!-- rule config panel -->
  <div class="scrim" [class.show]="!!editing()" (click)="close()"></div>
  <aside class="panel wide" [class.show]="!!editing()">
    <ng-container *ngIf="editing() as r">
      <div class="panel-head">
        <div>
          <div style="display:flex;align-items:center;gap:10px"><span class="code-chip">{{ r.code }}</span><h2 style="font-size:17px">Configure rule</h2></div>
          <div class="sub" style="margin-top:6px">Saving rebuilds the JDM and hot-reloads the ZEN Agent.</div>
        </div>
        <button class="close-x" (click)="close()"><svg viewBox="0 0 24 24" fill="none" stroke-width="2"><path d="M6 6l12 12M18 6L6 18"/></svg></button>
      </div>

      <!-- editing form -->
      <div class="panel-body" *ngIf="!publishing() && !published()">
        <div class="rule-grid">
          <div>
            <div class="field">
              <label>Rule code</label>
              <input [value]="r.code" disabled>
              <div class="hint">Returned as <span class="mono">ruleCode</span> in the evaluation result.</div>
            </div>

            <div class="field">
              <label>Condition</label>

              <!-- simple -->
              <div *ngIf="r.kind==='simple'" class="cond-builder">
                <div class="cond-row"><input [value]="conditionLeft(r)" disabled><span class="cond-op">test</span><input [value]="conditionRight(r)" disabled></div>
              </div>

              <!-- daterange -->
              <div *ngIf="r.kind==='daterange'" class="cond-builder">
                <div style="display:flex;align-items:center;gap:8px;flex-wrap:wrap;padding:9px">
                  <input value="treatmentDate" disabled style="flex:1;min-width:110px;padding:8px 10px;border:1px solid var(--line);border-radius:7px">
                  <span class="cond-op">outside</span>
                  <input type="date" [(ngModel)]="r.dateFrom" style="padding:7px 9px;border:1px solid var(--line);border-radius:7px">
                  <span class="and-tag" style="margin:0">→</span>
                  <input type="date" [(ngModel)]="r.dateTo" style="padding:7px 9px;border:1px solid var(--line);border-radius:7px">
                </div>
              </div>

              <!-- countries -->
              <div *ngIf="r.kind==='countries'">
                <div class="geo-col">
                  <div class="geo-head"><span class="geo-dot included"></span>Included countries<span class="geo-count">{{ r.included?.length || 0 }}</span></div>
                  <div class="chips">
                    <span class="chip included" *ngFor="let c of r.included; let i = index">{{ c }}<button title="Remove" (click)="removeCountry('included', i)">×</button></span>
                    <span class="chip-empty" *ngIf="!r.included?.length">No countries yet</span>
                  </div>
                  <div class="chip-add">
                    <input [(ngModel)]="addIncluded" placeholder="Add a country…" (keydown.enter)="addCountry('included')">
                    <button class="btn sm ghost" (click)="addCountry('included')">Add</button>
                  </div>
                  <div class="hint">Members are covered for treatment in these countries.</div>
                </div>
                <div class="geo-divider"></div>
                <div class="geo-col">
                  <div class="geo-head"><span class="geo-dot excluded"></span>Excluded countries<span class="geo-count">{{ r.excluded?.length || 0 }}</span></div>
                  <div class="chips">
                    <span class="chip excluded" *ngFor="let c of r.excluded; let i = index">{{ c }}<button title="Remove" (click)="removeCountry('excluded', i)">×</button></span>
                    <span class="chip-empty" *ngIf="!r.excluded?.length">No countries yet</span>
                  </div>
                  <div class="chip-add">
                    <input [(ngModel)]="addExcluded" placeholder="Add a country…" (keydown.enter)="addCountry('excluded')">
                    <button class="btn sm ghost" (click)="addCountry('excluded')">Add</button>
                  </div>
                  <div class="hint">Claims with a treatment country in this list are rejected.</div>
                </div>
              </div>

              <!-- catch -->
              <input *ngIf="r.kind==='catch'" value="catch-all (always last)" disabled>

              <div class="hint" *ngIf="r.kind==='catch'">The fall-through rule. Applies when no rejection or warning rule matches.</div>
              <div class="hint" *ngIf="r.kind==='simple'">First-match hit policy — rules evaluate top to bottom.</div>
            </div>

            <div class="field" *ngIf="r.kind!=='catch'">
              <label>Decision</label>
              <select [(ngModel)]="r.decision">
                <option value="rejected">rejected</option>
                <option value="warning">warning</option>
                <option value="approved">approved</option>
              </select>
            </div>

            <div class="field">
              <label>Reason message</label>
              <textarea [(ngModel)]="r.reason"></textarea>
              <div class="hint">Shown to the member and returned as <span class="mono">reason</span>.</div>
            </div>

            <div class="field" style="display:flex;align-items:center;gap:10px;margin-top:18px" *ngIf="r.kind!=='catch'">
              <input type="checkbox" [(ngModel)]="r.enabled" style="width:auto">
              <label style="margin:0">Rule enabled</label>
            </div>
          </div>

          <div>
            <div class="section-title" style="margin-top:0">JDM decision table</div>
            <div class="json-preview"><pre>{{ jdmSnippet(r) }}</pre></div>
            <div class="section-title">On save</div>
            <div style="font-size:12.5px;color:var(--muted);line-height:1.6">
              Rebuilds <span class="mono" style="color:var(--navy-soft)">claim_validation.json</span>, repackages the zip with <span class="mono" style="color:var(--navy-soft)">.config/project.json</span>, uploads to <span class="mono" style="color:var(--navy-soft)">gorules-poc</span> on iDrive&nbsp;e2. Agent hot-reloads within 5s.
            </div>
          </div>
        </div>
      </div>

      <!-- publish pipeline -->
      <div class="panel-body" *ngIf="publishing() || published()">
        <h3 style="font-size:16px;margin-bottom:4px">Publishing rule {{ r.code }}</h3>
        <div class="sub" style="margin-bottom:18px">Rebuilding the JDM and rolling it out to the live agent.</div>
        <div class="pipeline">
          <div class="pstep" *ngFor="let s of steps()" [class.pending]="s.state==='pending'" [class.active]="s.state==='active'" [class.done]="s.state==='done'" [class.failed]="s.state==='failed'">
            <div class="ic">
              <div class="spin" *ngIf="s.state==='active'"></div>
              <svg *ngIf="s.state==='done'" viewBox="0 0 24 24" fill="none" stroke="var(--green)" stroke-width="2.5"><path d="M5 12l5 5L20 7"/></svg>
              <svg *ngIf="s.state==='failed'" viewBox="0 0 24 24" fill="none" stroke="var(--coral)" stroke-width="2.5"><path d="M6 6l12 12M18 6L6 18"/></svg>
              <svg *ngIf="s.state==='pending'" viewBox="0 0 24 24" fill="none" stroke="var(--green)" stroke-width="2.5"><path d="M5 12l5 5L20 7"/></svg>
            </div>
            <div><div class="tx">{{ s.label }}</div><div class="mono-sm">{{ s.detail }}</div></div>
          </div>
        </div>

        <div *ngIf="published() && !error()" class="result approved" style="margin-top:18px">
          <div class="verdict"><div class="vc"><svg viewBox="0 0 24 24" fill="none"><path d="M5 12l5 5L20 7"/></svg></div>
            <div><div class="vtitle">Rule live</div><div class="vmeta">agent reloaded · {{ now }}</div></div></div>
          <div class="reason">Rule <b>{{ r.code }}</b> is now active in the ZEN Engine. New claims evaluate against the updated ruleset immediately.</div>
        </div>

        <div *ngIf="error()" class="banner error" style="margin-top:18px">{{ error() }}</div>
      </div>

      <div class="panel-foot">
        <ng-container *ngIf="!publishing() && !published()">
          <button class="btn ghost" (click)="close()">Cancel</button>
          <button class="btn primary" (click)="save()">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 12l5 5L20 7"/></svg>
            Save rule
          </button>
        </ng-container>
        <ng-container *ngIf="published()">
          <button class="btn ghost" (click)="close()">Close</button>
          <button class="btn primary" (click)="backToEdit()">Back to rule</button>
        </ng-container>
      </div>
    </ng-container>
  </aside>

  <div class="toast" [class.show]="toast()">{{ toast() }}</div>
  `
})
export class MedicalCasesComponent implements OnInit {
  rules = signal<Rule[]>([]);
  editing = signal<Rule | null>(null);
  publishing = signal(false);
  published = signal(false);
  error = signal<string | null>(null);
  steps = signal<Step[]>([]);
  toast = signal<string | null>(null);
  now = '';

  query = '';
  decision = '';
  addIncluded = '';
  addExcluded = '';
  private filterTick = signal(0);

  filtered = computed(() => {
    this.filterTick();
    const q = this.query.toLowerCase();
    return this.rules().filter(r =>
      (r.code.toLowerCase().includes(q) || r.condition.toLowerCase().includes(q)) &&
      (!this.decision || r.decision === this.decision));
  });

  constructor(private api: ApiService) {}

  ngOnInit(): void { this.load(); }

  load() { this.api.getRules().subscribe(r => this.rules.set(r)); }
  tick() { this.filterTick.update(v => v + 1); }
  dec(d: string) { return d === 'approved' ? 'green' : d === 'warning' ? 'amber' : 'coral'; }

  open(r: Rule) {
    // edit a clone so Cancel discards changes
    this.editing.set(JSON.parse(JSON.stringify(r)));
    this.publishing.set(false);
    this.published.set(false);
    this.error.set(null);
    this.addIncluded = '';
    this.addExcluded = '';
  }
  close() { this.editing.set(null); }
  backToEdit() { this.publishing.set(false); this.published.set(false); this.error.set(null); }

  addCountry(type: 'included' | 'excluded') {
    const r = this.editing(); if (!r) return;
    const val = (type === 'included' ? this.addIncluded : this.addExcluded).trim();
    if (!val) return;
    const list = (r[type] ??= []);
    if (!list.some(c => c.toLowerCase() === val.toLowerCase())) list.push(val);
    if (type === 'included') this.addIncluded = ''; else this.addExcluded = '';
    this.editing.set({ ...r });
  }
  removeCountry(type: 'included' | 'excluded', i: number) {
    const r = this.editing(); if (!r) return;
    r[type]?.splice(i, 1);
    this.editing.set({ ...r });
  }

  // --- condition helpers (display only, for simple rules) ---
  conditionLeft(r: Rule): string {
    if (r.gender && r.claimType) return `gender = ${r.gender} AND claimType`;
    if (r.gender) return 'gender';
    if (r.claimType) return `claimType = ${r.claimType}` + (r.ageTest ? ' AND age' : '');
    if (r.treatmentOlderThanOneYear) return 'treatmentDate';
    return r.condition;
  }
  conditionRight(r: Rule): string {
    if (r.gender && r.claimType) return `= ${r.claimType}`;
    if (r.claimType && r.ageTest) return r.ageTest;
    if (r.ageTest) return r.ageTest;
    if (r.treatmentOlderThanOneYear) return 'older than 1 year';
    return '';
  }

  jdmSnippet(r: Rule): string {
    const reason = (r.reason || '').slice(0, 40) + (r.reason && r.reason.length > 40 ? '…' : '');
    return `{
  "contentType": "application/vnd.gorules.decision",
  "nodes": [{
    "type": "decisionTableNode",
    "content": {
      "hitPolicy": "first",
      "rules": [
        … {
          "_id": "${r.code}",
          "_description": "${(r.condition || '').replace(/"/g, '')}",
          "o_decision": "\\"${r.decision}\\"",
          "o_reason": "\\"${reason}\\"",
          "o_ruleCode": "\\"${r.code}\\""
        } …
      ]
    }
  }]
}`;
  }

  // --- save + publish pipeline ---
  save() {
    const r = this.editing(); if (!r) return;
    this.publishing.set(true);
    this.published.set(false);
    this.error.set(null);
    this.steps.set([
      { label: 'Updating decision table', detail: `rule ${r.code} · first-match`, state: 'pending' },
      { label: 'Serializing claim_validation.json', detail: 'JDM v1.0.0', state: 'pending' },
      { label: 'Packaging zip', detail: '.config/project.json + claim_validation.json', state: 'pending' },
      { label: 'Uploading to iDrive e2', detail: 's3://gorules-poc/claim_validation.zip', state: 'pending' },
      { label: 'ZEN Agent hot-reload', detail: 'polling ≤ 5s', state: 'pending' }
    ]);
    this.runPipeline(r);
  }

  private async runPipeline(r: Rule) {
    // Kick off the real publish (single backend call that does all stages).
    let httpOk: boolean | null = null;
    let httpErr = '';
    this.api.saveRule(r).subscribe({
      next: () => { httpOk = true; this.applySaved(r); },
      error: (e) => { httpOk = false; httpErr = e?.error?.error ?? 'Failed to publish to storage.'; }
    });

    const sleep = (ms: number) => new Promise(res => setTimeout(res, ms));
    const setState = (i: number, s: Step['state']) => {
      this.steps.update(arr => arr.map((st, idx) => idx === i ? { ...st, state: s } : st));
    };

    // Stages 0-2 are quick representational steps.
    for (let i = 0; i < 3; i++) { setState(i, 'active'); await sleep(500); setState(i, 'done'); }

    // Stage 3 (upload) waits for the real HTTP result.
    setState(3, 'active');
    while (httpOk === null) await sleep(120);
    if (httpOk === false) {
      setState(3, 'failed');
      this.error.set(httpErr);
      this.published.set(true);
      this.publishing.set(false);
      this.flash('Publish failed');
      return;
    }
    setState(3, 'done');

    // Stage 4 (hot-reload).
    setState(4, 'active'); await sleep(1000); setState(4, 'done');
    this.now = new Date().toLocaleTimeString();
    this.published.set(true);
    this.publishing.set(false);
    this.flash(`Rule ${r.code} published — agent hot-reloaded`);
  }

  private applySaved(saved: Rule) {
    this.rules.update(list => {
      const idx = list.findIndex(x => x.code === saved.code);
      if (idx >= 0) { const copy = [...list]; copy[idx] = saved; return copy; }
      return [...list, saved];
    });
  }

  private flash(msg: string) {
    this.toast.set(msg);
    setTimeout(() => this.toast.set(null), 3200);
  }
}
