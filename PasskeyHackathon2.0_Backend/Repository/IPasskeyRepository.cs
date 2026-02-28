using System.Threading.Tasks;

namespace PasskeyHackathon2._0.Repository
{
    public interface IPasskeyRepository
    {
        // Save a passkey record containing the username, fingerprint or hash, and the domain
        public Task<bool> CreatePasskey(string EmailAddress, byte[] fingerprintOrHash, string domain);
        public Task<bool> VerifyPasskey(string username);

    }
}
