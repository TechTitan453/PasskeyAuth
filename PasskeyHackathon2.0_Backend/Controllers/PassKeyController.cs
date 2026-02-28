using Fido2NetLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PasskeyHackathon2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassKeyController : ControllerBase
    {

        public PassKeyController()
        {
                
        }
        [HttpPost("CreatePasskey")]
        public async Task<IActionResult> CreatePasskey(string username)
        {
          Fido2 fido = 

           
            //  If Service is true 
            
        }
        [HttpPost("VerifyPasskey")]
        public async Task<IActionResult> VerifyPasskey(string username)
        {

        }
    }
}
