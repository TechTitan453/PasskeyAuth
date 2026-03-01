using System;
using System.Threading.Tasks;
using PasskeyHackathon2._0.Models;
using Microsoft.EntityFrameworkCore;

namespace PasskeyHackathon2._0.Repository
{
    public class PasskeyRepository : IPasskeyRepository
    {
        private readonly AppDbContext _context;

        public PasskeyRepository(AppDbContext context)
        {
            _context = context;
        }

        // Persist a full passkey credential model
        public async Task<bool> CreatePasskey(PasskeyCredentialModel credential)
        {
            if (credential == null) return false;

            credential.CreatedAt = credential.CreatedAt == default ? DateTime.UtcNow : credential.CreatedAt;
            credential.CredType = credential.CredType ?? "public-key";

            _context.PasskeyCredentials.Add(credential);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // Detach the failed/invalid entity so it won't be retried on a later SaveChanges
                var entry = _context.Entry(credential);
                if (entry != null) entry.State = EntityState.Detached;
                return false;
            }
        }

        // Persist a passkey record to the database
        public async Task<bool> CreatePasskey(string username, byte[] fingerprintOrHash, string domain)
        {
            var credential = new PasskeyCredentialModel
            {
                Email = username,
                Fingerprint = fingerprintOrHash != null ? Convert.ToBase64String(fingerprintOrHash) : null,
                Domain = domain,
                CredType = "public-key",
                CreatedAt = DateTime.UtcNow
            };

            _context.PasskeyCredentials.Add(credential);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // Detach the failed/invalid entity so it won't be retried on a later SaveChanges
                var entry = _context.Entry(credential);
                if (entry != null) entry.State = EntityState.Detached;
                return false;
            }
        }

        public async Task<bool> VerifyPasskey(string username,string Domain)
        {
            try
            {
                return await _context.PasskeyCredentials.AnyAsync(x => x.Email == username);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsCredentialIdUnique(string credentialId)
        {
            try
            {
                if (string.IsNullOrEmpty(credentialId)) return true;
                return !await _context.PasskeyCredentials.AnyAsync(x => x.CredentialId == credentialId);
            }
            catch
            {
                return false;
            }
        }
        public async Task<PasskeyCredentialModel?> GetPasskeyByEmailAsync(string emailAddress)
        {
            return await _context.PasskeyCredentials
                .FirstOrDefaultAsync(x => x.Email == emailAddress);
        }

        public async Task<PasskeyCredentialModel?> GetByCredentialIdAsync(string credentialId)
        {
            return await _context.PasskeyCredentials
                .FirstOrDefaultAsync(x => x.CredentialId == credentialId);
        }

        public async Task<bool> UpdateSignatureCounterAsync(string credentialId, uint newCounter)
        {
            var cred = await GetByCredentialIdAsync(credentialId);
            if (cred == null) return false;
            cred.SignatureCounter = newCounter;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                var entry = _context.Entry(cred);
                if (entry != null) entry.State = EntityState.Detached;
                return false;
            }
        }
        public async Task StoreChallengeAsync(string email, string optionsJson)
        {
            var existing = await _context.PasskeyChallenges
                .FirstOrDefaultAsync(x => x.Email == email);

            if (existing != null)
            {
                existing.OptionsJson = optionsJson;
                existing.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.PasskeyChallenges.Add(new PasskeyChallengeModel
                {
                    Email = email,
                    OptionsJson = optionsJson,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<string?> GetChallengeAsync(string email)
        {
            var record = await _context.PasskeyChallenges
                .FirstOrDefaultAsync(x => x.Email == email);

            return record?.OptionsJson;
        }

        public async Task DeleteChallengeAsync(string email)
        {
            var record = await _context.PasskeyChallenges
                .FirstOrDefaultAsync(x => x.Email == email);

            if (record != null)
            {
                _context.PasskeyChallenges.Remove(record);
                await _context.SaveChangesAsync();  
            }
        }
    }
}
