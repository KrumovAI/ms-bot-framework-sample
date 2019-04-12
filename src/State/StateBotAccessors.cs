using BasicBot.Infrastructure.Constants;
using BasicBot.Infrastructure.Extensions;
using BasicBot.Models;
using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicBot.State
{
    public class BotStateAccessors
    {
        public BotStateAccessors(UserState userState, ConversationState conversationState)
        {
            this.UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            this.ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }

        public static string UserTokensName { get; } = "UserTokens";

        public IStatePropertyAccessor<TokensModel> UserTokensAccessor { get; set; }

        public UserState UserState { get; }

        public ConversationState ConversationState { get; }

        public async Task UpdateTokensAsync(ITurnContext turnContext, TokensModel newTokens)
        {
            newTokens.ExpiresAt = DateTime.Now.AddSeconds(newTokens.ExpiresIn);
            newTokens.UserId = new Guid(newTokens.IdToken.GetClaim(TokenClaimConstants.UserIdClaim));
            newTokens.UserName = newTokens.IdToken.GetClaim(TokenClaimConstants.UserNameClaim);
            newTokens.SchoolId = new Guid(newTokens.IdToken.GetClaim(TokenClaimConstants.SchoolIdClaim));
            newTokens.IsAuthenticated = true;

            await this.UserTokensAccessor.SetAsync(turnContext, newTokens);
        }

        public async Task ClearTokensAsync(ITurnContext turnContext)
        {
            await this.UserTokensAccessor.SetAsync(turnContext, new TokensModel());
        }
    }
}
