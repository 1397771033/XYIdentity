using AuthorizationServer.Clients;
using AuthorizationServer.Data;
using AuthorizationServer.Params;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AuthorizationServer.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthorizeController : ControllerBase
    {
        private readonly IMemoryCache _memory;
        public AuthorizeController(IMemoryCache memory)
        {
            _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        }
        [Route("authorize")]
        [HttpGet]
        public IActionResult Authorization(string client_id, string redirect_uri, string response_type, string state)
        {
            var client = ClientData.GetClient(client_id);
            if (client == null)
            {
                return NotFound("Unknown client");
            }
            else if (!client.RedirectUris.Contains(redirect_uri))
            {
                return BadRequest("Invalid redirect_uri");
            }
            else
            {
                // 生成requestId，后续用户通过表单提交时可以获取到保存的客户端信息
                var requestId = Guid.NewGuid();
                _memory.Set(requestId, new ClientRequest
                {
                    ClientId = client_id,
                    RedirectUrl = redirect_uri,
                    ResponseType = response_type,
                    State = state
                });

                return Ok(new { client = client, requestId = requestId });
            }
        }
        [Route("approve")]
        [HttpPost]
        public IActionResult Approve([FromBody] ApproveParam param)
        {
            // 如果是authorize发出的requestId的话
            if (_memory.TryGetValue(param.request_id, out ClientRequest client))
            {
                _memory.Remove(param.request_id);
                if (client.ResponseType == "code")
                {
                    var code = Guid.NewGuid();
                    _memory.Set(code, new ClientRequest
                    {
                        ClientId=client.ClientId,
                        RedirectUrl=client.RedirectUrl,
                        ResponseType=client.ResponseType,
                        State=client.State
                    });
                    Console.WriteLine(client.RedirectUrl + $"?code={code}&state={client.State}");
                    return Redirect(client.RedirectUrl + $"?code={code}&state={client.State}");
                }
                else
                {
                    Console.WriteLine("unsupported_response_type");
                    return Redirect(client.RedirectUrl);
                }
            }
            else
            {
                return NotFound("No matching authorization request");
            }
        }
        [Route("token")]
        [HttpPost]
        public IActionResult Token(TokenParam param)
        {
            var client = ClientData.GetClient(param.client_id);
            if(client == null)
            {
                return Unauthorized("invalid_client");
            }
            if(client.ClientSecret != param.client_secret)
            {
                return Unauthorized("invalid_client");
            }
            if(param.grant_type== "authorization_code")
            {
                var clientRequest= _memory.Get<ClientRequest>(param.code);
                if (clientRequest!=null)
                {
                    _memory.Remove(param.code);
                    if(clientRequest.ClientId== param.client_id)
                    {
                        var accessToken =Guid.NewGuid().ToString();
                        _memory.TryGetValue($"{param.client_id}_accesstoken", out List<AccessToken> accessTokens);
                        if(accessTokens==null)
                            accessTokens = new List<AccessToken>();
                        accessTokens.Add(new AccessToken { access_token=accessToken });
                        _memory.Set($"{param.client_id}_accesstoken",accessTokens);


                        var refreshToken =Guid.NewGuid().ToString();
                        _memory.TryGetValue($"{param.client_id}_refreshtoken", out List<AccessToken> refreshTokens);
                        if (refreshTokens == null)
                            refreshTokens = new List<AccessToken>();
                        refreshTokens.Add(new AccessToken { access_token = accessToken });
                        _memory.Set($"{param.client_id}_refreshtoken", refreshTokens);

                        return Ok(new
                        {
                            access_token = accessToken,
                            refresh_token = refreshToken,
                            token_type = "Bearer"
                        });
                    }
                    else
                    {
                        return BadRequest("invalid_grant");
                    }
                }
                else
                {
                    return BadRequest("invalid_grant");
                }
            }
            else
            {
                return BadRequest("unsupported_grant_type");
            }
        }
    }
}