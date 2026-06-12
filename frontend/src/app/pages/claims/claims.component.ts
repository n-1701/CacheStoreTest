import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Claim, EvaluationResult, Member } from '../../models/models';

@Component({
  selector: 'app-claims',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
  <div class="topbar">
    <div>
      <div class="eyebrow">History</div>
      <h1>Claims</h1>
      <p>Every claim submitted and the ruleset decision it received. Submit a new claim to evaluate it live.</p>
    </div>
    <div>
      <button class="btn primary" (click)="openModal()">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg>
        New claim
      </button>
    </div>
  </div>

  <div class="content">
    <div class="card">
      <div class="toolbar">
        <div class="search">
          <svg viewBox="0 0 24 24" fill="none" stroke-width="2"><circle cx="11" cy="11" r="7"/><path d="M21 21l-4-4"/></svg>
          <input type="text" placeholder="Search claims…" [(ngModel)]="query" (ngModelChange)="tick()">
        </div>
        <select class="filter" [(ngModel)]="decision" (ngModelChange)="tick()">
          <option value="">All outcomes</option><option value="approved">Approved</option><option value="warning">Warning</option><option value="rejected">Rejected</option>
        </select>
        <span class="count">{{ filtered().length }} of {{ claims().length }} claims</span>
      </div>

      <div class="table-scroll">
        <table>
          <thead><tr>
            <th>Claim ID</th><th>Member</th><th>Claim type</th><th>Country</th><th>Submitted</th><th>Decision</th><th>Rule</th><th>Reason</th>
          </tr></thead>
          <tbody>
            <tr *ngFor="let c of filtered()">
              <td><span class="mono">{{ c.id }}</span></td>
              <td><div class="nm">{{ c.name }}</div><div class="sub mono">{{ c.memberId }}</div></td>
              <td style="text-transform:capitalize">{{ c.type }}</td>
              <td>{{ c.country }}</td>
              <td><span class="mono">{{ c.date }}</span></td>
              <td><span class="pill" [class]="'pill ' + dec(c.decision)"><span class="pd"></span>{{ c.decision }}</span></td>
              <td><span class="code-chip">{{ c.rule }}</span></td>
              <td style="max-width:280px;color:var(--muted)">{{ c.reason }}</td>
            </tr>
            <tr *ngIf="!filtered().length"><td colspan="8" class="empty-row">No claims match these filters.</td></tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>

  <!-- New claim modal -->
  <div class="modal-wrap" [class.show]="modalOpen()">
    <div class="modal">
      <div class="modal-head">
        <div><h2 style="font-size:19px">New claim</h2><div class="sub" style="margin-top:5px">Submit a claim context and evaluate it against the live ruleset.</div></div>
        <button class="close-x" (click)="closeModal()"><svg viewBox="0 0 24 24" fill="none" stroke-width="2"><path d="M6 6l12 12M18 6L6 18"/></svg></button>
      </div>
      <div class="modal-body">
        <div class="field">
          <label>Member</label>
          <select [(ngModel)]="form.memberId" (ngModelChange)="syncMember()">
            <option *ngFor="let m of members()" [value]="m.id">{{ m.name }} · {{ m.id }}</option>
          </select>
        </div>
        <div class="row2">
          <div class="field"><label>Gender</label><select [(ngModel)]="form.gender"><option>male</option><option>female</option></select></div>
          <div class="field"><label>Age</label><input type="number" [(ngModel)]="form.age" min="0" max="120"></div>
        </div>
        <div class="row2">
          <div class="field"><label>Claim type</label>
            <select [(ngModel)]="form.claimType"><option>maternity</option><option>pediatric</option><option>dental</option><option>general</option><option>optical</option></select>
          </div>
          <div class="field"><label>Treatment date</label><input type="date" [(ngModel)]="form.treatmentDate"></div>
        </div>
        <div class="row2">
          <div class="field"><label>Treatment country</label>
            <select [(ngModel)]="form.country">
              <option>UAE</option><option>Saudi Arabia</option><option>Qatar</option><option>Bahrain</option><option>Kuwait</option><option>Oman</option><option>India</option><option>United Kingdom</option><option>North Korea</option>
            </select>
          </div>
          <div class="field"><label>Amount (AED)</label><input type="number" [(ngModel)]="form.amount"></div>
        </div>

        <div *ngIf="evaluating()" style="font-size:12px;color:var(--faint);font-family:'IBM Plex Mono';margin-top:6px">
          POST /claims/evaluate → ZEN agent (X-Access-Token attached server-side)…
        </div>

        <div *ngIf="result() as res" class="result" [class]="'result ' + res.decision">
          <div class="verdict">
            <div class="vc">
              <svg *ngIf="res.decision==='approved'" viewBox="0 0 24 24" fill="none"><path d="M5 12l5 5L20 7"/></svg>
              <svg *ngIf="res.decision==='rejected'" viewBox="0 0 24 24" fill="none"><path d="M6 6l12 12M18 6L6 18"/></svg>
              <svg *ngIf="res.decision==='warning'" viewBox="0 0 24 24" fill="none"><path d="M12 3l9 16H3z"/><path d="M12 10v3.5M12 16.5h.01"/></svg>
            </div>
            <div>
              <div class="vtitle">{{ title(res.decision) }}</div>
              <div class="vmeta">ruleCode: {{ res.ruleCode }}</div>
            </div>
          </div>
          <div class="reason">{{ res.reason }}</div>
        </div>

        <div *ngIf="error()" class="banner error" style="margin:14px 0 0">{{ error() }}</div>
      </div>
      <div class="modal-foot">
        <button class="btn ghost" (click)="closeModal()">{{ result() ? 'Done' : 'Cancel' }}</button>
        <button class="btn primary" *ngIf="!result()" [disabled]="evaluating()" (click)="evaluate()">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M13 2L3 14h7l-1 8 10-12h-7l1-8z"/></svg>
          {{ evaluating() ? 'Evaluating…' : 'Evaluate claim' }}
        </button>
      </div>
    </div>
  </div>

  <div class="toast" [class.show]="toast()">{{ toast() }}</div>
  `
})
export class ClaimsComponent implements OnInit {
  claims = signal<Claim[]>([]);
  members = signal<Member[]>([]);
  modalOpen = signal(false);
  evaluating = signal(false);
  result = signal<EvaluationResult | null>(null);
  error = signal<string | null>(null);
  toast = signal<string | null>(null);

  query = '';
  decision = '';
  private filterTick = signal(0);

  form = {
    memberId: '', gender: 'male', age: 30, claimType: 'maternity',
    country: 'UAE', treatmentDate: '2026-06-10', amount: 1200, record: true
  };

  filtered = computed(() => {
    this.filterTick();
    const q = this.query.toLowerCase();
    return this.claims().filter(c =>
      (c.id.toLowerCase().includes(q) || c.name.toLowerCase().includes(q) || c.type.includes(q)) &&
      (!this.decision || c.decision === this.decision));
  });

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.api.getClaims().subscribe(c => this.claims.set(c));
    this.api.getMembers().subscribe(m => {
      this.members.set(m);
      if (m.length) { this.form.memberId = m[0].id; this.syncMember(); }
    });
  }

  tick() { this.filterTick.update(v => v + 1); }
  dec(d: string) { return d === 'approved' ? 'green' : d === 'warning' ? 'amber' : 'coral'; }
  title(d: string) { return d === 'approved' ? 'Approved' : d === 'warning' ? 'Needs review' : 'Rejected'; }

  openModal() {
    this.result.set(null);
    this.error.set(null);
    this.modalOpen.set(true);
  }
  closeModal() { this.modalOpen.set(false); }

  syncMember() {
    const m = this.members().find(x => x.id === this.form.memberId);
    if (m) { this.form.gender = m.gender.toLowerCase(); this.form.age = m.age; }
  }

  evaluate() {
    this.evaluating.set(true);
    this.error.set(null);
    this.api.evaluateClaim(this.form).subscribe({
      next: (res) => {
        this.evaluating.set(false);
        this.result.set(res.result);
        if (res.claim) {
          this.claims.update(list => [res.claim!, ...list]);
          this.flash(`Claim ${res.claim.id} recorded · ${res.result.decision}`);
        }
      },
      error: (e) => {
        this.evaluating.set(false);
        this.error.set(e?.error?.error ?? 'Evaluation failed. Check the backend and ZEN agent.');
      }
    });
  }

  private flash(msg: string) {
    this.toast.set(msg);
    setTimeout(() => this.toast.set(null), 3200);
  }
}
