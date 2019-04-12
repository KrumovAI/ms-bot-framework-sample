namespace BasicBot.Services
{
    using BasicBot.Infrastructure.Configs;
    using BasicBot.Infrastructure.Exceptions;
    using BasicBot.Models;
    using BasicBot.State;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class BaseService : IService
    {
        private readonly IOptions<AuthAppConfigs> authAppConfigs;
        private const string BaseUri = "https://localhost:44348/api/"; // Set your backend's base uri here

        protected readonly ITurnContextResolverService turnContextResolver;
        protected readonly BotStateAccessors botState;

        public BaseService(
            ITurnContextResolverService turnContextResolver,
            BotStateAccessors botState,
            IOptions<AuthAppConfigs> authAppOptions
        ) {
            this.turnContextResolver = turnContextResolver;
            this.botState = botState;
            this.authAppConfigs = authAppOptions;
        }

        protected async Task<T> GetAsync<T>(string url, bool isAuthenticated = true)
        {
            using (HttpClient client = this.GetHttpClient(isAuthenticated))
            {
                var result = await client.GetAsync(BaseUri + url);

                if (!result.IsSuccessStatusCode)
                {
                    this.HandleExceptions(result.StatusCode);
                }

                string resultContent = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(resultContent);
            }
        }

        protected async Task<T> PostAsync<T>(object data, string url, bool isAuthenticated = true)
        {
            using (HttpClient client = this.GetHttpClient(isAuthenticated))
            {
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var result = await client.PostAsync(BaseUri + url, content);

                if (!result.IsSuccessStatusCode)
                {
                    this.HandleExceptions(result.StatusCode);
                }

                string resultContent = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(resultContent);
            }
        }

        private HttpClient GetHttpClient(bool isAuthenticated = true)
        {
            var tokens = this.GetTokensAsync().Result;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(BaseUri);

            if (isAuthenticated && tokens.IdToken != null && tokens.AccessToken != null)
            {
                if (tokens.ExpiresAt.CompareTo(DateTime.Now) <= 0)
                {
                    AuthService authService = new AuthService(this.turnContextResolver, this.botState, this.authAppConfigs);
                    authService.RefreshTokensAsync().RunSynchronously();
                    tokens = this.GetTokensAsync().Result;
                }

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokens.IdToken}");
                client.DefaultRequestHeaders.Add("GraphToken", tokens.AccessToken);
                client.DefaultRequestHeaders.Add("SchoolId", tokens.SchoolId.ToString());
            }

            return client;
        }

        private void HandleExceptions(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedException();
                default:
                    throw new BadRequestException();
            }
        }

        private async Task<TokensModel> GetTokensAsync()
        {
            return await this.botState.UserTokensAccessor.GetAsync(this.turnContextResolver.TurnContext, () => new TokensModel());
        }
    }
}
