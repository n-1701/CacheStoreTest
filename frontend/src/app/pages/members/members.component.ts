import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Claim, Member } from '../../models/models';

@Component({
  selector: 'app-members',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
  <div class="topbar">
    <div>
      <div class="eyebrow">Directory</div>
      <h1>Members</h1>
      <p>All enrolled members and policies. Click a member to open their full record.</p>
    </div>
  </div>

  <div class="content">
    <div class="card">
      <div class="toolbar">
        <div class="search">
          <svg viewBox="0 0 24 24" fill="none" stroke-width="2"><circle cx="11" cy="11" r="7"/><path d="M21 21l-4-4"/></svg>
          <input type="text" placeholder="Search by name or member ID…" [(ngModel)]="query" (ngModelChange)="apply()">
        </div>
        <select class="filter" [(ngModel)]="gender" (ngModelChange)="apply()">
          <option value="">All genders</option><option>Male</option><option>Female</option>
        </select>
        <select class="filter" [(ngModel)]="status" (ngModelChange)="apply()">
          <option value="">All statuses</option><option>Active</option><option>Inactive</option><option>Pending</option>
        </select>
        <span class="count">{{ filtered().length }} of {{ members().length }} members</span>
      </div>

      <div class="table-scroll">
        <table>
          <thead><tr>
            <th>Member ID</th><th>Name</th><th>Gender</th><th>Age</th><th>Date of birth</th><th>Policy number</th><th>Status</th><th style="text-align:right">Actions</th>
          </tr></thead>
          <tbody>
            <tr *ngFor="let m of filtered(); let i = index" class="clickable" (click)="open(m)">
              <td><span class="mono">{{ m.id }}</span></td>
              <td>
                <div class="name-cell">
                  <div class="avatar" [style.background]="color(i)">{{ initials(m.name) }}</div>
                  <div><div class="nm">{{ m.name }}</div><div class="sub">{{ m.plan }}</div></div>
                </div>
              </td>
              <td>{{ m.gender }}</td>
              <td>{{ m.age }}</td>
              <td><span class="mono">{{ m.dob }}</span></td>
              <td><span class="mono">{{ m.policy }}</span></td>
              <td><span class="pill" [class]="'pill ' + statusClass(m.status)"><span class="pd"></span>{{ m.status }}</span></td>
              <td style="text-align:right">
                <button class="icon-btn" (click)="$event.stopPropagation(); open(m)">
                  <svg viewBox="0 0 24 24" fill="none" stroke-width="2"><path d="M1.5 12S5 5 12 5s10.5 7 10.5 7-3.5 7-10.5 7S1.5 12 1.5 12z"/><circle cx="12" cy="12" r="3"/></svg>
                </button>
              </td>
            </tr>
            <tr *ngIf="!filtered().length"><td colspan="8" class="empty-row">No members match these filters.</td></tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>

  <!-- detail panel -->
  <div class="scrim" [class.show]="!!selected()" (click)="close()"></div>
  <aside class="panel" [class.show]="!!selected()">
    <ng-container *ngIf="selected() as m">
      <div class="panel-head">
        <div style="display:flex;align-items:center;gap:14px">
          <div class="avatar" [style.background]="color(index(m))" style="width:46px;height:46px;border-radius:12px;font-size:16px">{{ initials(m.name) }}</div>
          <div><h2 style="font-size:18px">{{ m.name }}</h2><div class="mono" style="margin-top:3px">{{ m.id }}</div></div>
        </div>
        <button class="close-x" (click)="close()"><svg viewBox="0 0 24 24" fill="none" stroke-width="2"><path d="M6 6l12 12M18 6L6 18"/></svg></button>
      </div>
      <div class="panel-body">
        <span class="pill" [class]="'pill ' + statusClass(m.status)"><span class="pd"></span>{{ m.status }}</span>
        <div class="section-title">Personal</div>
        <div class="kv">
          <div><div class="k">Gender</div><div class="v">{{ m.gender }}</div></div>
          <div><div class="k">Age</div><div class="v">{{ m.age }}</div></div>
          <div><div class="k">Date of birth</div><div class="v mono">{{ m.dob }}</div></div>
          <div><div class="k">Dependents</div><div class="v">{{ m.dependents }}</div></div>
          <div class="full"><div class="k">Email</div><div class="v">{{ m.email }}</div></div>
          <div class="full"><div class="k">Phone</div><div class="v mono">{{ m.phone }}</div></div>
        </div>
        <div class="section-title">Policy</div>
        <div class="kv">
          <div><div class="k">Policy number</div><div class="v mono">{{ m.policy }}</div></div>
          <div><div class="k">Plan</div><div class="v">{{ m.plan }}</div></div>
          <div><div class="k">Enrolled</div><div class="v mono">{{ m.joined }}</div></div>
          <div><div class="k">Member ID</div><div class="v mono">{{ m.id }}</div></div>
        </div>
        <div class="section-title">Recent claims</div>
        <div *ngIf="claimsFor(m.id).length; else noClaims">
          <div *ngFor="let c of claimsFor(m.id)" style="display:flex;align-items:center;justify-content:space-between;padding:11px 0;border-bottom:1px solid var(--line)">
            <div><div style="font-weight:600;text-transform:capitalize">{{ c.type }}</div><div class="sub mono">{{ c.id }} · {{ c.date }}</div></div>
            <span class="pill" [class]="'pill ' + dec(c.decision)"><span class="pd"></span>{{ c.decision }}</span>
          </div>
        </div>
        <ng-template #noClaims><div style="font-size:13px;color:var(--faint);padding:8px 0">No claims on record.</div></ng-template>
      </div>
      <div class="panel-foot">
        <button class="btn ghost" (click)="close()">Close</button>
        <button class="btn primary">Edit member</button>
      </div>
    </ng-container>
  </aside>
  `
})
export class MembersComponent implements OnInit {
  members = signal<Member[]>([]);
  claims = signal<Claim[]>([]);
  selected = signal<Member | null>(null);

  query = '';
  gender = '';
  status = '';
  private filterTick = signal(0);

  filtered = computed(() => {
    this.filterTick();
    const q = this.query.toLowerCase();
    return this.members().filter(m =>
      (m.name.toLowerCase().includes(q) || m.id.toLowerCase().includes(q)) &&
      (!this.gender || m.gender === this.gender) &&
      (!this.status || m.status === this.status));
  });

  private palette = ['#E8505B', '#3E8FD8', '#5B3A8C', '#1F9D6B', '#C98A1A', '#3A3A78'];

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.api.getMembers().subscribe(m => this.members.set(m));
    this.api.getClaims().subscribe(c => this.claims.set(c));
  }

  apply() { this.filterTick.update(v => v + 1); }
  open(m: Member) { this.selected.set(m); }
  close() { this.selected.set(null); }
  index(m: Member) { return this.members().indexOf(m); }
  color(i: number) { return this.palette[i % this.palette.length]; }
  initials(n: string) { return n.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase(); }
  dec(d: string) { return d === 'approved' ? 'green' : d === 'warning' ? 'amber' : 'coral'; }
  statusClass(s: string) { return s === 'Active' ? 'green' : s === 'Pending' ? 'amber' : 'gray'; }
  claimsFor(id: string) { return this.claims().filter(c => c.memberId === id); }
}
