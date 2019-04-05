namespace BasicBot.Models
{
    using System;

    public class TeacherClassViewModel
    {
        public int Id { get; set; }

        public Guid TeacherId { get; set; }

        public string TeacherName { get; set; }

        public int SubjectId { get; set; }

        public string SubjectName { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public byte Duration { get; set; }

        public DateTime Date { get; set; }

        public string ClassName { get; set; }

        public string Topic { get; set; }

        public override string ToString()
        {
            return $"{this.StartTime} - {this.EndTime} {this.SubjectName} при {this.ClassName}";
        }
    }
}
