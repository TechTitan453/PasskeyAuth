using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using PasskeyHackathon2._0.Models;
using PasskeyHackathon2._0.Repository;
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
        [HttpPost("CreatePasskey")]
        public async Task<IActionResult> CreatePasskey([FromBody ] RegisterModel requestModel)
        {
            try
            {
                if (string.IsNullOrEmpty(requestModel.EmailAddress))
                {
                    return BadRequest("Email Address is required");
                }

                var user = new Fido2User
                {
                    DisplayName = requestModel.EmailAddress,
                    Name = requestModel.EmailAddress,
                    Id = Encoding.UTF8.GetBytes(requestModel.EmailAddress),
                };

                var options = fido2.RequestNewCredential(new RequestNewCredentialParams
                {
                    User = user,
                    ExcludeCredentials = new List<PublicKeyCredentialDescriptor>(),
                    AuthenticatorSelection = AuthenticatorSelection.Default,
                    AttestationPreference = AttestationConveyancePreference.None
                });

                //HttpContext.Session.SetString("fido2.attestationOptions", options.ToJson());
                var IsPasskeySaved =await passkeyRepository.CreatePasskey(requestModel.EmailAddress,user.Id , requestModel.Domain);
                if (IsPasskeySaved) {
                    return Ok(options);
                }
                return BadRequest("Something went wrong! Please check the Model Again !");
            } catch (Exception ex) { 
                return BadRequest(ex.Message);  
            }

        }
        [HttpPost("VerifyPasskey")]
        public async Task<IActionResult> VerifyPasskey([FromBody] string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("username is required");

            try
            {
                var exists = await passkeyRepository.VerifyPasskey(username);
                if (exists)
                    return Ok(new { verified = true });

                return NotFound(new { verified = false });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
