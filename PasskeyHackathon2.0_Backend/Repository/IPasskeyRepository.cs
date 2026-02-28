namespace PasskeyHackathon2._0.Repository
{
    public interface IPasskeyRepository
    {
        public bool CreatePasskey(string username,string hash);
        public bool VerifyPasskey(string username); 

    }
}
