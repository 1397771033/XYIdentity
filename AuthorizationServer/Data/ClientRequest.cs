namespace AuthorizationServer.Data
{
    public class ClientRequest
    {
        public string ClientId { get; set; }
        public string RedirectUrl { get; set; }
        public string ResponseType { get; set; }
        public string State { get; set; }
    }
}
