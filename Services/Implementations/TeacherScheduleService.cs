namespace BasicBot.Services
{
    using BasicBot.Models;
    using BasicBot.State;
    using System;
    using System.Threading.Tasks;

    public class TeacherScheduleService : BaseService, ITeacherScheduleService
    {
        public TeacherScheduleService(
            ITurnContextResolverService turnContextResolverService,
            BotStateAccessors botState
        ) : base(turnContextResolverService, botState)
        {
        }

        public async Task<TeacherClassViewModel> GetTeacherCurrentClassAsync(Guid teacherId)
        {
            string uri = $"schedulehour/current?teacherId={teacherId}";

            var teacherClass = await this.GetAsync<TeacherClassViewModel>(uri);

            return teacherClass;
        }

        public async Task<TeacherClassViewModel> GetTeacherNextClassAsync(Guid teacherId)
        {
            string uri = $"schedulehour/next?teacherId={teacherId}";

            var teacherClass = await this.GetAsync<TeacherClassViewModel>(uri);

            return teacherClass;
        }
    }
}
