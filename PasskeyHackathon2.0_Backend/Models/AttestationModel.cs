using Fido2NetLib;
namespace PasskeyHackathon2._0.Models
{
    public class AttestationModel
    {
        public string EmailAddress { get; set; }
        public PublicKeyCredentialRpEntity Attestation { get; set; }
    }
}
