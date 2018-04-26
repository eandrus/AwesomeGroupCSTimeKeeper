using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSTimeKeeper.Models
{
    public class StudentDashViewModel
    {
        public string StatusMessage { get; set; }
        public List<GroupMember> ActiveGroups { get; set; }
        public List<StudentCourse> ActiveCourses { get; set; }

        public StudentDashViewModel()
        {
            ActiveGroups = new List<GroupMember>();
            ActiveCourses = new List<StudentCourse>();
        }
    }
}
