namespace BasicBot.Services
{
    using BasicBot.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITeacherService : IService
    {
        Task<IEnumerable<TeacherViewModel>> GetTeachersByNameAsync(string teacherName);
    }
}
