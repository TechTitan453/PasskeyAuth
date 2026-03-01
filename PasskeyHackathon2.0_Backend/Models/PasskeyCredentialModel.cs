namespace PasskeyHackathon2._0.Models
{
    public class PasskeyCredentialModel
    {
        
            public int Id { get; set; }

            public string Email { get; set; }

            public string UserId { get; set; }

            public string CredentialId { get; set; }

            public string PublicKey { get; set; }

            public uint SignatureCounter { get; set; }

            public string CredType { get; set; }

            // Optional: store a fingerprint or hash associated with this credential
            public string Fingerprint { get; set; }


        // Optional: domain or tenant for multi-tenant scenarios
            public string ? Domain { get; set; } 

            public DateTime CreatedAt { get; set; }
        
    }
}
