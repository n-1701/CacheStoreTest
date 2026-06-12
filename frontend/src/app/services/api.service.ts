import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Claim, EvaluateRequest, EvaluateResponse, Member, Rule
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly base = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  // --- Rules ---
  getRules(): Observable<Rule[]> {
    return this.http.get<Rule[]>(`${this.base}/rules`);
  }

  getJdm(): Observable<string> {
    return this.http.get(`${this.base}/rules/jdm`, { responseType: 'text' });
  }

  /** Save a rule. The backend rebuilds the JDM, repackages the zip, and uploads it. */
  saveRule(rule: Rule): Observable<Rule> {
    return this.http.put<Rule>(`${this.base}/rules/${encodeURIComponent(rule.code)}`, rule);
  }

  // --- Members ---
  getMembers(): Observable<Member[]> {
    return this.http.get<Member[]>(`${this.base}/members`);
  }

  // --- Claims ---
  getClaims(): Observable<Claim[]> {
    return this.http.get<Claim[]>(`${this.base}/claims`);
  }

  evaluateClaim(req: EvaluateRequest): Observable<EvaluateResponse> {
    return this.http.post<EvaluateResponse>(`${this.base}/claims/evaluate`, req);
  }
}
