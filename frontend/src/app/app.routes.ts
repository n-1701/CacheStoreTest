import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'medical-cases' },
  {
    path: 'medical-cases',
    loadComponent: () =>
      import('./pages/medical-cases/medical-cases.component').then(m => m.MedicalCasesComponent)
  },
  {
    path: 'members',
    loadComponent: () =>
      import('./pages/members/members.component').then(m => m.MembersComponent)
  },
  {
    path: 'claims',
    loadComponent: () =>
      import('./pages/claims/claims.component').then(m => m.ClaimsComponent)
  },
  { path: '**', redirectTo: 'medical-cases' }
];
