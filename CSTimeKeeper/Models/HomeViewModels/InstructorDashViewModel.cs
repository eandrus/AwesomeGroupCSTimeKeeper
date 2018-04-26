using System;
using System.Collections.Generic;

namespace CSTimeKeeper.Models
{
    public class InstructorDashViewModel
    {
        public string StatusMessage { get; set; }
        public int CourseRegistrations { get; set; }
        public List<Course> ActiveCourses { get; set; }
        public User Instructor { get; set; }

        public InstructorDashViewModel()
        {
            ActiveCourses = new List<Course>();
        }
    }
}
