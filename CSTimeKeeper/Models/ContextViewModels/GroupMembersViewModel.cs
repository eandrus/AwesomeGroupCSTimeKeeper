using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSTimeKeeper.Models
{
    public class GroupMembersViewModel
    {
		public Group group { get; set; }
		public string StatusMessage { get; set; }
		public User currentUser { get; set; }
		public bool userInGroup { get; set; }
    }
}
