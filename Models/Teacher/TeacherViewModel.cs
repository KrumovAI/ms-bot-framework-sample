namespace BasicBot.Models
{
    using System;
    using System.Collections.Generic;

    public class TeacherViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<SchoolSubjectViewModel> Subjects { get; set; }
    }
}
