namespace Library
{
    public class JwtOptions

    {
        public string issuer { get; set; }
        public string audience { get; set; }
        public int lifetime { get; set; }
        public string signingKey { get; set; }
    }

}
