namespace BasicBot.Services
{
    using BasicBot.Models;
    using System;
    using System.Threading.Tasks;

    public interface ITeacherScheduleService
    {
        Task<TeacherClassViewModel> GetTeacherCurrentClassAsync(Guid teacherId);

        Task<TeacherClassViewModel> GetTeacherNextClassAsync(Guid teacherId);
    }
}
