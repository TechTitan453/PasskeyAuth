namespace PasskeyHackathon2._0.Models
{
    public class VerifyLoginRequest
    {
        public string Id { get; set; }
        public string RawId { get; set; }
        public string Type { get; set; }
        public VerifyLoginResponse Response { get; set; }
    }

    public class VerifyLoginResponse
    {
        public string AuthenticatorData { get; set; }
        public string ClientDataJSON { get; set; }
        public string Signature { get; set; }
        public string? UserHandle { get; set; }
    }
}
