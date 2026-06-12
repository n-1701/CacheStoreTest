import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
  <div class="app">
    <aside class="sidebar">
      <div class="brand">
        <!-- LOGO PLACEHOLDER — replace with the official mark, e.g.
             <img src="assets/nas-neuron-logo.svg" width="40" height="40" alt="NAS Neuron"> -->
        <div class="logo-slot" title="Logo placeholder — swap with the NAS Neuron mark">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="var(--faint)" stroke-width="1.6">
            <rect x="3" y="3" width="18" height="18" rx="3"/><circle cx="8.5" cy="8.5" r="1.6"/><path d="M21 15l-5-5L5 21"/>
          </svg>
        </div>
        <div class="wm">
          <div class="t1">NAS NEURON</div>
          <div class="t2">Health Services</div>
        </div>
      </div>

      <nav class="nav">
        <div class="nav-label">Claims Engine</div>
        <a class="nav-item" routerLink="/medical-cases" routerLinkActive="active">
          <svg viewBox="0 0 24 24" fill="none" stroke-width="1.8"><path d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2"/><rect x="9" y="3" width="6" height="4" rx="1"/><path d="M9 12l2 2 4-4"/></svg>
          Medical Cases
        </a>
        <a class="nav-item" routerLink="/members" routerLinkActive="active">
          <svg viewBox="0 0 24 24" fill="none" stroke-width="1.8"><circle cx="9" cy="8" r="3.2"/><path d="M3.5 19a5.5 5.5 0 0111 0"/><path d="M16 5.2a3.2 3.2 0 010 6"/><path d="M17 13.5a5.5 5.5 0 013.5 5.5"/></svg>
          Members
        </a>
        <a class="nav-item" routerLink="/claims" routerLinkActive="active">
          <svg viewBox="0 0 24 24" fill="none" stroke-width="1.8"><path d="M6 3h9l4 4v14a1 1 0 01-1 1H6a1 1 0 01-1-1V4a1 1 0 011-1z"/><path d="M14 3v4h4"/><path d="M9 13h6M9 17h4"/></svg>
          Claims
        </a>
      </nav>

      <div class="agent-card">
        <div class="row"><span class="dot pulse"></span><span class="lbl">ZEN Agent · live</span></div>
        <div class="url">agent-latest-jl93.onrender.com<br>polling every 5s · hot-reload on</div>
      </div>
      <div class="site">nasneuron.com</div>
    </aside>

    <main class="main">
      <router-outlet></router-outlet>
    </main>
  </div>
  `
})
export class AppComponent {}
