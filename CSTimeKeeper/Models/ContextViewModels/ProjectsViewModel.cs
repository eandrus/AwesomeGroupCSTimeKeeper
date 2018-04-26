using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSTimeKeeper.Models
{
    public class ProjectsViewModel
    {
        public Course Course { get; set; }

        public List<Project> Projects { get; set; }

        public string StatusMessage { get; set; }
    }
}
