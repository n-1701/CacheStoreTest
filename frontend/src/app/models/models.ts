export type Decision = 'approved' | 'rejected' | 'warning';
export type RuleKind = 'simple' | 'daterange' | 'countries' | 'catch';

export interface Rule {
  code: string;
  kind: RuleKind;
  condition: string;
  decision: Decision;
  reason: string;
  enabled: boolean;
  gender?: string | null;
  claimType?: string | null;
  ageTest?: string | null;
  treatmentOlderThanOneYear?: boolean;
  dateFrom?: string | null;
  dateTo?: string | null;
  included?: string[] | null;
  excluded?: string[] | null;
}

export interface Member {
  id: string;
  name: string;
  gender: string;
  age: number;
  dob: string;
  policy: string;
  status: string;
  plan: string;
  email: string;
  phone: string;
  joined: string;
  dependents: number;
}

export interface Claim {
  id: string;
  memberId: string;
  name: string;
  type: string;
  country: string;
  date: string;
  decision: Decision;
  rule: string;
  reason: string;
}

export interface EvaluateRequest {
  memberId: string;
  gender: string;
  age: number;
  claimType: string;
  country: string;
  treatmentDate: string;
  amount: number;
  record: boolean;
}

export interface EvaluationResult {
  decision: Decision;
  reason: string;
  ruleCode: string;
}

export interface EvaluateResponse {
  result: EvaluationResult;
  claim: Claim | null;
}

/** Maps a decision to its pill / result colour class. */
export function decClass(d: Decision | string): 'green' | 'amber' | 'coral' {
  return d === 'approved' ? 'green' : d === 'warning' ? 'amber' : 'coral';
}
