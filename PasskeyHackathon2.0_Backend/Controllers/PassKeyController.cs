using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using PasskeyHackathon2._0.Models;
using PasskeyHackathon2._0.Repository;
using Microsoft.EntityFrameworkCore;
namespace PasskeyHackathon2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassKeyController : ControllerBase
    {
        private readonly Fido2 fido2; 
        private readonly IPasskeyRepository passkeyRepository;  
        public PassKeyController(Fido2 _fido2, IPasskeyRepository _passkeyRepository)
        {
                this.fido2 = _fido2; 
                this.passkeyRepository = _passkeyRepository;
        }

        [HttpPost("verify-login")]
        public async Task<IActionResult> VerifyLogin([FromBody] Models.VerifyLoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.RawId))
                return BadRequest("rawId required");

            // Minimal placeholder verification: ensure credential exists in DB.
            // Full FIDO2 assertion verification should be implemented using Fido2.MakeAssertionAsync.
            byte[] rawId;
            try
            {
                var b64 = request.RawId.Replace('-', '+').Replace('_', '/');
                while (b64.Length % 4 != 0) b64 += '=';
                rawId = Convert.FromBase64String(b64);
            }
            catch
            {
                return BadRequest("Invalid rawId");
            }

            var credentialIdBase64 = Convert.ToBase64String(rawId);
            var stored = await passkeyRepository.GetByCredentialIdAsync(credentialIdBase64);
            if (stored == null) return BadRequest("Credential not found");

            return Ok(new { success = true, message = "Credential found (assertion verification not implemented)" });
        }
        //Options for creation 
        [HttpPost("create-options")]
        public async Task<IActionResult> CreateOptions([FromBody] RegisterModel model)
        {
            if (string.IsNullOrEmpty(model.EmailAddress))
                return BadRequest("Email required");

            var user = new Fido2User
            {
                DisplayName = model.EmailAddress,
                Name = model.EmailAddress,
                Id = Encoding.UTF8.GetBytes(model.EmailAddress)
            };

            var options = fido2.RequestNewCredential(new RequestNewCredentialParams
            {
                User = user,
                AuthenticatorSelection = AuthenticatorSelection.Default,
                AttestationPreference = AttestationConveyancePreference.None
            });

            await passkeyRepository.StoreChallengeAsync(
                model.EmailAddress,
                options.ToJson()
            );

            return Ok(options);
        }
        [HttpPost("verify-passkey")]
        public async Task<IActionResult> VerifyPasskey([FromBody] VerifyPasskeyRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Email required");

            var jsonOptions = await passkeyRepository.GetChallengeAsync(request.Email);

            if (jsonOptions == null)
                return BadRequest("Challenge expired or not found");

            var options = CredentialCreateOptions.FromJson(jsonOptions);

            var result = await fido2.MakeNewCredentialAsync(
                new MakeNewCredentialParams
                {
                    AttestationResponse = request.AttestationResponse,
                    OriginalOptions = options,

                    IsCredentialIdUniqueToUserCallback = async (args, cancellationToken) =>
                    {
                        var credentialId = Convert.ToBase64String(args.CredentialId);
                        return await passkeyRepository.IsCredentialIdUnique(credentialId);
                    }
                },
                CancellationToken.None
            );

            var credential = new PasskeyCredentialModel
            {
                Email = request.Email,
                CredentialId = Convert.ToBase64String(result.Id),
                PublicKey = Convert.ToBase64String(result.PublicKey),
                SignatureCounter = result.SignCount,
                CredType = "public-key",
                CreatedAt = DateTime.UtcNow
            };

            await passkeyRepository.CreatePasskey(credential);

            await passkeyRepository.DeleteChallengeAsync(request.Email);
            return Ok(new { message = "Passkey stored successfully" });
        }
        // Endpoint that creates a passkey and persists it via the repository
        [HttpPost("create-passkey")]
        public async Task<IActionResult> CreatePasskey(
            [FromBody] AuthenticatorAttestationRawResponse response,
            [FromServices] AppDbContext _context,
            [FromQuery] string? domain = null)
        {
            var jsonOptions = HttpContext.Session.GetString("fido2.attestationOptions");
            if (jsonOptions == null)
                return BadRequest("Session expired");

            var options = CredentialCreateOptions.FromJson(jsonOptions);

            var result = await fido2.MakeNewCredentialAsync(
                new MakeNewCredentialParams
                {
                    AttestationResponse = response,
                    OriginalOptions = options,

                    IsCredentialIdUniqueToUserCallback = async (args, cancellationToken) =>
                    {
                        var credentialId = Convert.ToBase64String(args.CredentialId);
                         
                        var exists = await _context.PasskeyCredentials
                            .AnyAsync(x => x.CredentialId == credentialId, cancellationToken);

                        return !exists;
                    }
                },
                CancellationToken.None
            );

            var email = Encoding.UTF8.GetString(result.User.Id);

            var credential = new PasskeyCredentialModel
            {
                Email = email,
                UserId = email,
                CredentialId = Convert.ToBase64String(result.Id),
                PublicKey = Convert.ToBase64String(result.PublicKey),
                SignatureCounter = result.SignCount,
                CredType = response.Type.ToString(),
                Fingerprint = Convert.ToBase64String(result.Id),
                Domain = domain,
                CreatedAt = DateTime.UtcNow
            };

            var saved = await passkeyRepository.CreatePasskey(credential);
            if (!saved)
                return BadRequest("Failed to save passkey");

            return Ok("Passkey stored in database successfully");
        }

        // Simple endpoint to check whether a passkey exists in the database for an email
        [HttpPost("passkey-exists")]
        public async Task<IActionResult> PasskeyExists([FromBody] EmailRequestModel email)
        {
            if (string.IsNullOrEmpty(email.EmailAddress)) return BadRequest("email is required");
            var exists = await passkeyRepository.VerifyPasskey(email.EmailAddress,email.Domain);
            return Ok(new { exists });
        }
        [HttpPost("login-options")]
        public async Task<IActionResult> LoginOptions([FromBody] EmailRequestModel email)
        {
            if (string.IsNullOrEmpty(email.EmailAddress))
                return BadRequest("Email required");

            var credential = await passkeyRepository.GetPasskeyByEmailAsync(email.EmailAddress);
            if (credential == null)
                return BadRequest("No passkey registered");

            var options = fido2.GetAssertionOptions(
                new List<PublicKeyCredentialDescriptor>
                {
            new PublicKeyCredentialDescriptor(
                Convert.FromBase64String(credential.CredentialId))
                },
                UserVerificationRequirement.Required
            );

            HttpContext.Session.SetString("fido2.assertionOptions", options.ToJson());

            return Ok(options);
        }
        

    }
}
