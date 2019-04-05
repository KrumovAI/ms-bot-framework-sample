namespace BasicBot.Dialogs
{
    using System;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using System.Threading;
    using Microsoft.Bot.Schema;
    using BasicBot.Services;
    using System.Collections.Generic;
    using BasicBot.State;
    using BasicBot.Models;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using System.Linq;
    using BasicBot.Infrastructure.Constants;

    public class TeacherNextClassDialog : TeacherBaseDialog
    {
        private const string GetNextClassDialog = "GetNextClassDialog";

        private const string TeacherPrompt = "TeacherPrompt";
        
        public TeacherNextClassDialog(
            ITeacherService teacherService,
            ITeacherScheduleService teacherScheduleService,
            BotStateAccessors botStateAccessors,
            ILoggerFactory loggerFactory
        ) : base(teacherService, teacherScheduleService, botStateAccessors, nameof(TeacherNextClassDialog))
        {

            // Add control flow dialogs
            var waterfallSteps = new WaterfallStep[]
            {
                this.GetTeacherIdsStepAsync,
                this.GetTeacherScheduleStepAsync,
            };

            AddDialog(new WaterfallDialog(GetNextClassDialog, waterfallSteps));
            this.AddPromptDialogs();
        }

        protected async Task<DialogTurnResult> GetTeacherScheduleStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            TeacherViewModel selectedTeacher = null;

            if (stepContext.Result is TeacherViewModel)
            {
                selectedTeacher = stepContext.Result as TeacherViewModel;
            }
            else
            {
                var result = stepContext.Result as FoundChoice;
                var teachers = (await this.botStateAccessors.MatchingTeachersAccessor.GetAsync(stepContext.Context, () => new List<TeacherViewModel>())).ToList();

                selectedTeacher = teachers[result.Index];
            }

            TeacherClassViewModel teacherClass = await this.teacherScheduleService.GetTeacherNextClassAsync(selectedTeacher.Id);

            if (teacherClass == null)
            {
                await stepContext.Context.SendActivityAsync(MessageConstants.TeacherHasNoMoreClassesForToday);
                return await stepContext.EndDialogAsync();
            }

            teacherClass.TeacherId = selectedTeacher.Id;
            teacherClass.TeacherName = selectedTeacher.Name;

            Attachment card = this.GetTeacherClassHeroCard(teacherClass);
            var reply = stepContext.Context.Activity.CreateReply();
            reply.Attachments.Add(card);

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return await stepContext.EndDialogAsync();
        }
    }
}
