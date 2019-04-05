// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BasicBot.Dialogs;
using BasicBot.Dialogs.Auth;
using BasicBot.Infrastructure.Constants;
using BasicBot.Infrastructure.Extensions;
using BasicBot.Models;
using BasicBot.NLP;
using BasicBot.Services;
using BasicBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Main entry point and orchestration for bot.
    /// </summary>
    public class BasicBot : IBot
    {
        // Supported LUIS Intents
        public const string HelpIntent = "General.Help";
        public const string NoneIntent = "None";

        public const string Login = "Login";
        public const string TeacherGetCurrentClass = "Teacher_CurrentClass";
        public const string TeacherGetNextClass = "Teacher_NextClass";
        public const string StatisticsGetClassStatistics = "Statistics_GetClassStatistics";

        /// <summary>
        /// Key in the bot config (.bot file) for the LUIS instance.
        /// In the .bot file, multiple instances of LUIS can be configured.
        /// </summary>
        public static readonly string LuisConfiguration = "Hristoforus";
        
        private readonly IStatePropertyAccessor<DialogState> dialogStateAccessor;
        private readonly UserState userState;
        private readonly ConversationState conversationState;
        private readonly BotServices services;

        private readonly BotStateAccessors stateBotAccessors;
        private readonly ITurnContextResolverService turnContextResolverService;

        private readonly IAuthService authService;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicBot"/> class.
        /// </summary>
        /// <param name="botServices">Bot services.</param>
        /// <param name="accessors">Bot State Accessors.</param>
        public BasicBot(
            BotServices services,
           
            UserState userState,
            ConversationState conversationState,
            BotStateAccessors stateBotAccessors,
            ITeacherService teacherService,
            ITeacherScheduleService teacherScheduleService,
            ITurnContextResolverService turnContextResolverService,
            IAuthService authService,
            ILoggerFactory loggerFactory
        ) {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
            this.userState = userState ?? throw new ArgumentNullException(nameof(userState));
            this.conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            this.authService = authService;
            
            this.turnContextResolverService = turnContextResolverService;
            dialogStateAccessor = this.conversationState.CreateProperty<DialogState>(nameof(DialogState));
            this.stateBotAccessors = stateBotAccessors ?? throw new System.ArgumentNullException(nameof(stateBotAccessors));
            
            // Verify LUIS configuration.
            if (!this.services.LuisServices.ContainsKey(LuisConfiguration))
            {
                throw new InvalidOperationException($"The bot configuration does not contain a service type of `luis` with the id `{LuisConfiguration}`.");
            }

            Dialogs = new DialogSet(dialogStateAccessor);
            Dialogs.Add(new LoginDialog(this.authService, this.stateBotAccessors));
            Dialogs.Add(new LogoutDialog(this.authService, this.stateBotAccessors));
            Dialogs.Add(new TeacherCurrentClassDialog(teacherService, teacherScheduleService, this.stateBotAccessors, loggerFactory));
            Dialogs.Add(new TeacherNextClassDialog(teacherService, teacherScheduleService, this.stateBotAccessors, loggerFactory));

            // Use this method to initilize the intents field in order to automize intent and entity parsing
            this.DeclareIntents();
        }

        private DialogSet Dialogs { get; set; }
        private List<Intent> Intents { get; set; }

        /// <summary>
        /// Run every turn of the conversation. Handles orchestration of messages.
        /// </summary>
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            this.turnContextResolverService.UpdateTurnContext(turnContext);
            await this.userState.LoadAsync(turnContext);
            var activity = turnContext.Activity;

            // Create a dialog context
            var dialogContext = await Dialogs.CreateContextAsync(turnContext);

            try
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    // Continue the current dialog
                    var dialogResult = await dialogContext.ContinueDialogAsync();

                    // if no one has responded,
                    if (!dialogContext.Context.Responded)
                    {
                        // examine results from active dialog
                        switch (dialogResult.Status)
                        {
                            case DialogTurnStatus.Empty:
                                // Perform a call to LUIS to retrieve results for the current activity message.
                                var luisResults = await services.LuisServices[LuisConfiguration].RecognizeAsync(dialogContext.Context, cancellationToken);
                                var topScoringIntent = luisResults?.GetTopScoringIntent();

                                var topIntent = this.Intents.FirstOrDefault(i => i.Id == topScoringIntent.Value.intent);

                                if (topIntent == null)
                                {
                                    await dialogContext.Context.SendActivityAsync(MessageConstants.MessageNotUnderstood);
                                }
                                else
                                {
                                    var parsedEntities = this.ParseLuisForEntities(luisResults);
                                    await dialogContext.BeginDialogAsync(topIntent.DialogId, parsedEntities);
                                }

                                break;

                            case DialogTurnStatus.Waiting:
                                // The active dialog is waiting for a response from the user, so do nothing.
                                break;

                            case DialogTurnStatus.Complete:
                            default:
                                await dialogContext.CancelAllDialogsAsync();
                                break;
                        }
                    }
                }
                else if (activity.Type == ActivityTypes.ConversationUpdate)
                {
                    if (activity.MembersAdded != null)
                    {
                        // Iterate over all new members added to the conversation.
                        foreach (var member in activity.MembersAdded)
                        {
                            // Greet anyone that was not the target (recipient) of this message.
                            if (member.Id != activity.Recipient.Id)
                            {
                                var userTokens = await this.stateBotAccessors.UserTokensAccessor.GetAsync(turnContext, () => new TokensModel());

                                if (userTokens.IsAuthenticated)
                                {
                                    // Send options for what the bot can do through adaptive cards for example
                                }
                                else
                                {
                                    await dialogContext.BeginDialogAsync(nameof(LoginDialog));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await dialogContext.CancelAllDialogsAsync();
                await turnContext.SendActivityAsync(ex.Message);
            }

            await conversationState.SaveChangesAsync(turnContext);
            await userState.SaveChangesAsync(turnContext);
        }
        
        private void DeclareIntents()
        {
            this.Intents = new List<Intent>()
                {
                    new Intent()
                    {
                        Id = "Login",
                        Properties = new List<Property>(),
                        DialogId = nameof(LoginDialog),
                    },
                new Intent()
                {
                    Id = "Logout",
                    Properties = new List<Property>(),
                    DialogId = nameof(LogoutDialog),
                },
                new Intent()
                {
                    Id = "Teacher_CurrentClass",
                    Properties = new List<Property>()
                    {
                        new Property("TeacherName", typeof(string), "personName"),
                    },
                    DialogId = nameof(TeacherCurrentClassDialog),
                },
                new Intent()
                {
                    Id = "Teacher_NextClass",
                    Properties = new List<Property>()
                    {
                        new Property("TeacherName", typeof(string), "personName"),
                    },
                    DialogId = nameof(TeacherNextClassDialog),
                },
                new Intent()
                {
                    Id = "Statistics_GetClassStatistics",
                    Properties = new List<Property>()
                    {
                        new Property("SchoolClass", typeof(string), "schoolClass"),
                    },
                },
            };
        }

        private Dictionary<string, object> ParseLuisForEntities(RecognizerResult recognizerResult)
        {
            Dictionary<string, object> parsedEntites = new Dictionary<string, object>();
            string intentName = recognizerResult.GetTopScoringIntent().intent;
            var intent = this.Intents.FirstOrDefault(i => i.Id == intentName);

            if (intent != null)
            {
                foreach (var property in intent.Properties)
                {
                    var value = recognizerResult.Entities[property.EntityType];

                    if (value == null)
                    {
                        continue;
                    }

                    int index = intent.Properties.Where(p => p.EntityType == property.EntityType).ToList().IndexOf(property);
                    var result = value[index].ToString();
                    parsedEntites.Add(property.Name, result);
                }
            }

            return parsedEntites;
        }
        
        private Attachment ShowWelcomeMessage()
        {
            List<CardAction> listButtons = new List<CardAction>();
            listButtons.Add(new CardAction
            {
                Type = "messageBack",
                Title = MessageConstants.Login,
                Text = CommandConstants.Login,
            });

            var card = new HeroCard(title: MessageConstants.Login, text: MessageConstants.PleaseLogin, buttons: listButtons);
            return card.ToAttachment();
        }
        
        // Create an attachment message response.
        private Activity CreateResponse(Activity activity, Attachment attachment)
        {
            var response = activity.CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }
    }
}
