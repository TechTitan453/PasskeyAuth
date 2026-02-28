using System.ComponentModel.DataAnnotations;

namespace PasskeyHackathon2._0.Models
{
    public class RegisterModel
    {
        [EmailAddress(ErrorMessage ="Email is required.")]
        public string EmailAddress { get; set; } = string.Empty;
        public string Domain  { get; set; }
    }

}
