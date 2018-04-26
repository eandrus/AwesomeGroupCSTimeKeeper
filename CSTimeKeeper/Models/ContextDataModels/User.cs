using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CSTimeKeeper.Data;

namespace CSTimeKeeper.Models
{
    public class User : IdentityUser, IComparable<User>
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return $"{ FirstName } { LastName }";
            }
        }

        [NotMapped]
        public string Roles
        {
            get
            {
                if (Administrator && !Instructor)
                    return "Administrator";
                else if (Administrator && Instructor)
                    return "Administrator, Instructor";
                else if (!Administrator && Instructor)
                    return "Instructor";
                else
                    return "Student";
            }
        }

        public bool Administrator { get; set; }

        public bool Instructor { get; set; }

        public bool Verified { get; set; }

        public List<Course> InstructorCourses { get; set; }

        public List<StudentCourse> StudentCourses { get; set; }

        public List<GroupMember> StudentGroups { get; set; }

        #region Navigation Loading Functions

        /// <summary>
        /// Loads relevant collections for this User
        /// </summary>
        /// <param name="context"></param>
        public void LoadCollections(TimeKeeperDbContext context)
        {
            if (Instructor)
            {
                context.Entry(this).Collection(u => u.InstructorCourses).Load();
            }

            if (Administrator)
                return;
            else
            {
                context.Entry(this).Collection(u => u.StudentCourses).Load();
                context.Entry(this).Collection(u => u.StudentGroups).Load();
            }

        }

        /// <summary>
        /// Load navigation properties with select collections cascading
        /// </summary>
        /// <param name="context"></param>
        public void LoadCascade(TimeKeeperDbContext context)
        {
            LoadCollections(context);

            if (!Administrator && !Instructor)
            {
                foreach (GroupMember member in StudentGroups)
                {
                    member.LoadFullCascade(context);
                }
            }
        }

        #endregion

        /// <summary>
        /// Sort by last name, then first name
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(User other)
        {
            if (LastName.ToLower().Equals(other.LastName.ToLower()))
                return FirstName.CompareTo(other.FirstName);
            else
                return LastName.CompareTo(other.LastName);
        }
    }
}
