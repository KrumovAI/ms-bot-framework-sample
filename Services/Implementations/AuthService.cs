namespace BasicBot.Services
{
    using BasicBot.Models;
    using BasicBot.State;
    using Microsoft.Extensions.DependencyInjection;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class AuthService : BaseService, IAuthService
    {
        private const string TokenScope = "openid offline_access https://graph.microsoft.com/user.read";
        private const string AdAppId = "db085960-1079-415a-94c5-a22437b986e1";

        public string AuthUri { get => $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={AdAppId}&response_type=code&redirect_uri={RedirectUri}&response_mode=query&scope={TokenScope}&state=12345"; }

        public AuthService(
            ITurnContextResolverService turnContextResolverService,
            BotStateAccessors botState
        ) : base(turnContextResolverService, botState)
        {
        }

        public async Task<TokensModel> RequestTokensAsync(string authCode)
        {
            var codeModel = new RequestTokensModel()
            {
                Assertion = authCode,
                RedirectUri = "http://localhost:4200/bot/code",
            };

            string url = "auth/code";
            return await this.PostAsync<TokensModel>(codeModel, url);
        }

        public async Task RefreshTokensAsync()
        {
            var oldTokens = await this.botState.UserTokensAccessor.GetAsync(this.turnContextResolver.TurnContext);
            string uri = "auth/refresh";

            RefreshTokensModel model = new RefreshTokensModel()
            {
                RedirectUri = RedirectUri,
                RefreshToken = oldTokens.RefreshToken,
            };

            var newTokens = await this.PostAsync<TokensModel>(model, uri);
            await this.botState.UpdateTokensAsync(this.turnContextResolver.TurnContext, newTokens);
        }

        public async Task LogoutAsync()
        {
            await this.botState.ClearTokensAsync(this.turnContextResolver.TurnContext);
        }
    }
}
