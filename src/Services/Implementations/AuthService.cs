namespace BasicBot.Services
{
    using BasicBot.Infrastructure.Configs;
    using BasicBot.Models;
    using BasicBot.State;
    using Microsoft.Extensions.Options;
    using System.Threading.Tasks;

    public class AuthService : BaseService, IAuthService
    {
        private AuthAppConfigs authAppConfigs;

        public AuthService(
            ITurnContextResolverService turnContextResolverService,
            BotStateAccessors botState,
            IOptions<AuthAppConfigs> authAppConfigs
        ) : base(turnContextResolverService, botState, authAppConfigs)
        {
            this.authAppConfigs = authAppConfigs.Value;

            // Change if not using Azure AD v2.0
            this.AuthUri = $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={this.authAppConfigs.AppId}&response_type=code&redirect_uri={this.authAppConfigs.RedirectUri}&response_mode=query&scope={this.authAppConfigs.GraphTokenScope}&state=12345";
        }

        public string AuthUri { get; private set; }

        public async Task<TokensModel> RequestTokensAsync(string authCode)
        {
            var codeModel = new RequestTokensModel()
            {
                Assertion = authCode,
                RedirectUri = this.authAppConfigs.RedirectUri,
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
                RedirectUri = this.authAppConfigs.RedirectUri,
                RefreshToken = oldTokens.RefreshToken,
            };

            var newTokens = await this.PostAsync<TokensModel>(model, uri, isAuthenticated: false);
            await this.botState.UpdateTokensAsync(this.turnContextResolver.TurnContext, newTokens);
        }

        public async Task LogoutAsync()
        {
            await this.botState.ClearTokensAsync(this.turnContextResolver.TurnContext);
        }
    }
}
