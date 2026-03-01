import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RegisterModel {
  emailAddress: string;
}

export interface VerifyPasskeyRequest {
  email: string;
  attestationResponse: any; // browser FIDO attestation object
}

export interface EmailRequestModel {
  EmailAddress: string;
  domain?: string | null;
}
@Injectable({
  providedIn: 'root'
})
export class PasskeyService {
  constructor(private http:HttpClient) {

   } 
   private baseUrl = 'https://localhost:7070/api/PassKey';
   

   

   // POST api/PassKey/create-options
  createOptions(model: RegisterModel): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/create-options`, model);
  }

  // POST api/PassKey/verify-passkey
  verifyPasskey(request: VerifyPasskeyRequest): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/verify-passkey`, request);
  }

  // POST api/PassKey/create-passkey?domain={domain}
  // `response` should be the AuthenticatorAttestationRawResponse from the browser
  createPasskey(response: any, domain?: string | null): Observable<any> {
    let params = new HttpParams();
    if (domain) params = params.set('domain', domain);
    return this.http.post<any>(`${this.baseUrl}/create-passkey`, response, { params });
  }

  // POST api/PassKey/passkey-exists
  passkeyExists(payload: EmailRequestModel): Observable<{ exists: boolean }> {
    return this.http.post<{ exists: boolean }>(`${this.baseUrl}/passkey-exists`, payload);
  }

  // POST api/PassKey/login-options
  loginOptions(payload: EmailRequestModel): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/login-options`, payload);
  }
  verifyLogin(payload: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/verify-login`, payload);  
  }
  
  } 
