namespace AuthorizationServer.Params
{
    public class TokenParam
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
        public Guid code { get; set; }
    }
}
