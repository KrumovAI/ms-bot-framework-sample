namespace BasicBot.Services
{
    using BasicBot.Models;
    using BasicBot.State;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class TeacherService : BaseService, ITeacherService
    {
        public TeacherService(
            ITurnContextResolverService turnContextResolver,
            BotStateAccessors botState
        ) : base(turnContextResolver, botState)
        {
        }

        public async Task<IEnumerable<TeacherViewModel>> GetTeachersByNameAsync(string teacherName)
        {
            string uri = $"teachers/byName?teacherName={teacherName}";

            var teachers = await this.GetAsync<IEnumerable<TeacherViewModel>>(uri);

            return teachers;
        }
    }
}
