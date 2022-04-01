namespace AuthorizationServer.Clients
{
    public class ClientData
    {
        private static List<ClientConfig> _clients=new List<ClientConfig>
        {
            new ClientConfig
            {
                ClientId="client_id_1",
                ClientSecret="client_secret_1",
                RedirectUris=new string[]{"http://localhost:6002/callback"}
            }
        };
        public static IEnumerable<ClientConfig> GetClients()
        {
            return _clients;
        }
        public static ClientConfig GetClient(string clientId)
        {
            return _clients.FirstOrDefault(_=>_.ClientId == clientId);
        }
    }
}
