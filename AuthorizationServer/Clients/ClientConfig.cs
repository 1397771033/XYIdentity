namespace AuthorizationServer.Clients
{
    public class ClientConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string[] RedirectUris { get; set; }
    }
}
