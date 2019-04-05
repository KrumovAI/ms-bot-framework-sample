using BasicBot.Infrastructure.Constants;
using BasicBot.Infrastructure.Extensions;
using BasicBot.Models;
using BasicBot.Services;
using BasicBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BasicBot.Dialogs
{
    public class TeacherBaseDialog : ComponentDialog
    {
        private string teacherNameKey = "TeacherName";
        protected readonly ITeacherService teacherService;
        protected readonly ITeacherScheduleService teacherScheduleService;

        protected BotStateAccessors botStateAccessors;

        private const string GetTeacherDialogName = "GetTeacherDialog";
        private const string TeacherDialogName = "TeacherDialog";
        private const string TooManyTeachersFoundPromptName = "TooManyTeachersFoundPrompt";

        public TeacherBaseDialog(
            ITeacherService teacherService,
            ITeacherScheduleService teacherScheduleService,
            BotStateAccessors botStateAccessors,
            string name = nameof(TeacherBaseDialog)
        ) : base(name)
        {
            this.teacherService = teacherService;
            this.teacherScheduleService = teacherScheduleService;

            this.botStateAccessors = botStateAccessors;
        }

        protected async Task<DialogTurnResult> GetTeacherIdsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = (Dictionary<string, object>)stepContext.Options;

            if (!options.ContainsKey(this.teacherNameKey))
            {
                await stepContext.Context.SendActivityAsync(MessageConstants.TeacherNotParsed);
                return await stepContext.EndDialogAsync();
            }

            string teacherName = ((string)options[this.teacherNameKey]).ToCyrillic();
            var teachers = (await this.teacherService.GetTeachersByNameAsync(teacherName)).ToList();
            await this.botStateAccessors.MatchingTeachersAccessor.SetAsync(stepContext.Context, teachers);

            if (teachers.Count() == 0)
            {
                await stepContext.Context.SendActivityAsync(MessageConstants.TeacherNotFound);
                return await stepContext.EndDialogAsync();
            }
            else if (teachers.Count > 1)
            {
                return await stepContext.PromptAsync(
                    TooManyTeachersFoundPromptName,
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text(MessageConstants.TooManyTeachersFound),
                        RetryPrompt = MessageFactory.Text(MessageConstants.InvalidChoice),
                        Choices = ChoiceFactory.ToChoices(teachers.Select(t => t.Name).ToList()),
                    },
                    cancellationToken);
            }

            return await stepContext.NextAsync(teachers[0]);
        }

        protected void AddPromptDialogs()
        {
            this.AddDialog(new ChoicePrompt(TooManyTeachersFoundPromptName));
        }

        protected Attachment GetTeacherClassHeroCard(TeacherClassViewModel teacherClass)
        {
            var heroCard = new HeroCard
            {
                Title = teacherClass.TeacherName,
                Subtitle = teacherClass.ToString(),
                Text = teacherClass.Topic,

                // Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Learn More", value: "http://www.devenvexe.com"), new CardAction(ActionTypes.OpenUrl, "C# Corner", value: "http://www.c-sharpcorner.com/members/suthahar-j"), new CardAction(ActionTypes.OpenUrl, "MSDN", value: "https://social.msdn.microsoft.com/profile/j%20suthahar/") }
            };

            return heroCard.ToAttachment();
        }
    }
}
