using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSTimeKeeper.Models
{
    public class CourseRequestsViewModel
    {
        public List<StudentCourse> Requests { get; set; }

        public string StatusMessage { get; set; }
    }
}
