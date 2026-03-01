namespace PasskeyHackathon2._0.Models
{
    public class PasskeyChallengeModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string OptionsJson { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
