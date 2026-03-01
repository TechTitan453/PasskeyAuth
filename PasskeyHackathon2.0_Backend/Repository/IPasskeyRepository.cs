using System.Threading.Tasks;
using PasskeyHackathon2._0.Models;

namespace PasskeyHackathon2._0.Repository
{
    public interface IPasskeyRepository
    {
        // Save a passkey record containing the username, fingerprint or hash, and the domain
        public Task<bool> CreatePasskey(string EmailAddress, byte[] fingerprintOrHash, string domain);

        // Save a full passkey credential model
        public Task<bool> CreatePasskey(PasskeyCredentialModel credential);

        public Task<bool> VerifyPasskey(string username,string domain);

        // Return true if a credential with the given base64 credentialId does NOT exist (unique)
        public Task<bool> IsCredentialIdUnique(string credentialId);
        public Task<PasskeyCredentialModel?> GetPasskeyByEmailAsync(string emailAddress);

        // Get credential by its base64 credential id
        public Task<PasskeyCredentialModel?> GetByCredentialIdAsync(string credentialId);

        // Update signature counter after a successful assertion
        public Task<bool> UpdateSignatureCounterAsync(string credentialId, uint newCounter);

        public Task StoreChallengeAsync(string email, string optionsJson);
        public Task<string?> GetChallengeAsync(string email);
        public Task DeleteChallengeAsync(string email);
    }
}
