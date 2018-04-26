using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSTimeKeeper.Models
{
    public class GroupsViewModel
    {
        public Project Project { get; set; }

        public List<Group> Groups { get; set; }

        public string StatusMessage { get; set; }
    }
}
