﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSTimeKeeper.Models
{
    public class InstructorRequestsViewModel
    {
        public List<User> Requests { get; set; }

        public string StatusMessage { get; set; }
    }
}
