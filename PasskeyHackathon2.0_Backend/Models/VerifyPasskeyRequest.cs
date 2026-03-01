using Fido2NetLib;

namespace PasskeyHackathon2._0.Models
{
    public class VerifyPasskeyRequest
    {
        public string Email { get; set; }
        public AuthenticatorAttestationRawResponse AttestationResponse { get; set; }
    }
}
