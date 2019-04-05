namespace BasicBot.Dialogs
{
    using BasicBot.Infrastructure.Constants;
    using BasicBot.Services;
    using BasicBot.State;
    using Microsoft.Bot.Builder.Dialogs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class LogoutDialog : ComponentDialog
    {
        private readonly IAuthService authService;
        private BotStateAccessors botStateAccessors;

        private const string LogoutDialogName = "UserLogoutDialog";
        
        public LogoutDialog(IAuthService authService, BotStateAccessors botStateAccessors)
            : base(nameof(LogoutDialog))
        {
            this.authService = authService;
            this.botStateAccessors = botStateAccessors;

            var waterfallSteps = new WaterfallStep[]
            {
                LogoutStepAsync,
            };

            this.AddDialog(new WaterfallDialog(LogoutDialogName, waterfallSteps));
        }

        private async Task<DialogTurnResult> LogoutStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await this.authService.LogoutAsync();
            await stepContext.Context.SendActivityAsync(MessageConstants.SuccessfulLogout);

            return await stepContext.EndDialogAsync();
        }
    }
}
