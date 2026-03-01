import { Component } from '@angular/core';
import { EmailRequestModel, PasskeyService } from './services/passkey.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'PasskeyFrontend';  
   email: string = ""; 
   domainName :any = location.hostname;
  constructor(private passkeyService:PasskeyService){

  }
 

 private base64ToArrayBuffer(base64url: string): ArrayBuffer {

  // Convert Base64URL to Base64
  let base64 = base64url
    .replace(/-/g, '+')
    .replace(/_/g, '/');

  // Add padding if missing
  while (base64.length % 4 !== 0) {
    base64 += '=';
  }

  const binary = atob(base64);
  const bytes = new Uint8Array(binary.length);

  for (let i = 0; i < binary.length; i++) {
    bytes[i] = binary.charCodeAt(i);
  }

  return bytes.buffer;
}

  private arrayBufferToBase64(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    bytes.forEach(b => binary += String.fromCharCode(b));
    return window.btoa(binary);
  }

  // =============================
  // 🔐 REGISTER PASSKEY
  // =============================

  async registerPasskey() {
    try {
      var emailRequestmodel:EmailRequestModel = {
        EmailAddress: this.email,
        domain: this.domainName
      }
      this.passkeyService.passkeyExists(emailRequestmodel).subscribe((existsRes) => {
        if (existsRes.exists) {
          alert("Passkey already exists for this email.");
          return;
        }
      });
     var registermodel ={
      emailAddress: this.email,
      domain: this.domainName
     }

      this.passkeyService.createOptions(registermodel).subscribe(async (options) => {
      options.challenge = this.base64ToArrayBuffer(options.challenge);
      options.user.id = this.base64ToArrayBuffer(options.user.id);
     const credential = await navigator.credentials.create({
  publicKey: options
}) as PublicKeyCredential;

const response = credential.response as AuthenticatorAttestationResponse;

const data = {
  email: this.email,
  attestationResponse: {
    id: credential.id,
    rawId: this.arrayBufferToBase64Url(credential.rawId),
    type: credential.type,
    response: {
      attestationObject: this.arrayBufferToBase64Url(response.attestationObject),
      clientDataJSON: this.arrayBufferToBase64Url(response.clientDataJSON),
      transports: response.getTransports ? response.getTransports() : []
    },
    clientExtensionResults: credential.getClientExtensionResults()
  }
};    
      alert("Passkey registered successfully!");
      this.passkeyService.verifyPasskey(data).subscribe(res => {
        console.log("Verification response:", res);
      });

    });

    


    } catch (error) {
      console.error(error);
      alert("Registration failed");
    }
  }
  private buildAssertionPayload(credential: PublicKeyCredential) {
    const resp = credential.response as AuthenticatorAssertionResponse;
    return {
      id: credential.id,
      rawId: this.arrayBufferToBase64(credential.rawId),
      type: credential.type,
      response: {
        authenticatorData: this.arrayBufferToBase64(resp.authenticatorData),
        clientDataJSON: this.arrayBufferToBase64(resp.clientDataJSON),
        signature: this.arrayBufferToBase64(resp.signature),
        userHandle: resp.userHandle
          ? this.arrayBufferToBase64(resp.userHandle)
          : null
      }
    };
  }
    async loginPasskey(): Promise<void> {

    try {
      // 1. request assertion options from the server 
      var emailRequestmodel :EmailRequestModel= {
        EmailAddress: this.email,
        domain: this.domainName
      }
      console.log(emailRequestmodel.EmailAddress);
      const opts = await firstValueFrom(
        this.passkeyService.loginOptions(emailRequestmodel)
      );

      // 2. normalize the values we’ll feed to the WebAuthn API
      const publicKey = this.normalizeAssertionOptions(opts);

      // 3. open the device popup
      const credential:any = await navigator.credentials.get({ publicKey })
         PublicKeyCredential;

      if (!credential) {
        throw new Error('No credential returned from authenticator');
      }

      // 4. build JSON payload and send to backend for verification
      const payload = this.buildAssertionPayload(credential);
      await firstValueFrom(this.passkeyService.verifyLogin(payload));

      alert('Login successful!');
    } catch (err) {
      console.error('loginPasskey error', err);
      alert('Login failed – see console for details');
    }
  }
  private normalizeAssertionOptions(opts: any): PublicKeyCredentialRequestOptions {
    opts.challenge = this.base64ToArrayBuffer(opts.challenge);
    if (Array.isArray(opts.allowCredentials)) {
      opts.allowCredentials = opts.allowCredentials.map((c: any) => ({
        ...c,
        id: this.base64ToArrayBuffer(c.id)
      }));
    }
    return opts;
  }

  private arrayBufferToBase64Url(buffer: ArrayBuffer): string {
  const bytes = new Uint8Array(buffer);
  let binary = '';
  bytes.forEach(b => binary += String.fromCharCode(b));

  let base64 = window.btoa(binary);

  // Convert to Base64URL
  return base64
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '');
}
}
