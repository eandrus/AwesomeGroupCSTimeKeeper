using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSTimeKeeper.Models
{
    public class CoursesViewModel
    {
        public User CurrentUser { get; set; }

        public List<Course> MyCourses { get; set; }

        public List<Course> MyPastCourses { get; set; }

        public List<Course> AvailableCourses { get; set; }

        public List<Course> PastCourses { get; set; }

        public List<Course> PendingRegistrations { get; set; }

        public string StatusMessage { get; set; }
    }
}
