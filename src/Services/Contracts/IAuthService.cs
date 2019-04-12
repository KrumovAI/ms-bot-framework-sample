namespace BasicBot.Services
{
    using System.Threading.Tasks;
    using BasicBot.Models;

    public interface IAuthService : IService
    {
        string AuthUri { get;  }

        Task<TokensModel> RequestTokensAsync(string authCode);

        Task RefreshTokensAsync();

        Task LogoutAsync();
    }
}
