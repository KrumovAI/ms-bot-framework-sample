using BasicBot.Infrastructure.Constants;
using BasicBot.Infrastructure.Exceptions;
using BasicBot.Infrastructure.Extensions;
using BasicBot.Services;
using BasicBot.State;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BasicBot.Dialogs.Auth
{
    public class LoginDialog : ComponentDialog
    {
        private readonly IAuthService authService;
        private BotStateAccessors botStateAccessors;

        private const string LoginDialogName = "UserLoginDialog";
        private const string AuthCodePrompt = "AuthCodePrompt";
        
        public LoginDialog(IAuthService authService, BotStateAccessors botStateAccessors)
            : base(nameof(LoginDialog))
        {
            this.authService = authService;
            this.botStateAccessors = botStateAccessors;

            var waterfallSteps = new WaterfallStep[]
            {
                SendLoginLinkStepAsync,
                PromptForAuthCodeStepAsync,
                EndLoginStepAsync,
            };

            this.AddDialog(new WaterfallDialog(LoginDialogName, waterfallSteps));
            this.AddDialog(new TextPrompt(AuthCodePrompt, GetTokensAsync));
        }

        private async Task<DialogTurnResult> SendLoginLinkStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            List<CardAction> listButtons = new List<CardAction>();
            listButtons.Add(new CardAction
            {
                Value = this.authService.AuthUri,
                Type = "singin",
                Title = MessageConstants.Login,
            });

            var card = new HeroCard(title: MessageConstants.Login, text: MessageConstants.PleaseLogin, buttons: listButtons);
            var reply = stepContext.Context.Activity.CreateReply();
            reply.Attachments.Add(card.ToAttachment());

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> PromptForAuthCodeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = MessageConstants.PleaseInsertAuthCode,
                },
            };

            return await stepContext.PromptAsync(AuthCodePrompt, opts);
        }

        private async Task<DialogTurnResult> EndLoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"{MessageConstants.SuccessfulLogin} {MessageConstants.WhatCanIDoForYou}");
            return await stepContext.EndDialogAsync();
        }

        private async Task<bool> GetTokensAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            // Validate that the user entered a minimum length for their name.
            string code = promptContext.Recognized.Value?.Trim() ?? string.Empty;

            try
            {
                var newTokens = await this.authService.RequestTokensAsync(code);
                await this.botStateAccessors.UpdateTokensAsync(promptContext.Context, newTokens);
                return true;
            }
            catch (BadRequestException)
            {
                await promptContext.Context.SendActivityAsync(MessageConstants.InvalidAuthCode);
                return false;
            }
        }
    }
}
