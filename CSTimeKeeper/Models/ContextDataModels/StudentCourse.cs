using CSTimeKeeper.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSTimeKeeper.Models
{
    public class StudentCourse : IComparable<StudentCourse>
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Student")]
        public string StudentId { get; set; }
        public User Student { get; set; }

        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; }

        public bool Approved { get; set; }

        #region Navigation Loading Functions

        /// <summary>
        /// Loads the student and course navigation properties
        /// </summary>
        /// <param name="context"></param>
        public void LoadParents(TimeKeeperDbContext context)
        {
            context.Entry(this).Reference(c => c.Course).Load();
            context.Entry(this).Reference(c => c.Student).Load();
        }

        #endregion

        public int CompareTo(StudentCourse other)
        {
            if (Course.Name.Equals(other.Course.Name))
                if (Course.InstructorId.Equals(other.Course.InstructorId))
                    return StudentId.CompareTo(other.StudentId);
                else
                    return Course.InstructorId.CompareTo(other.Course.InstructorId);
            else
                return Course.Name.CompareTo(other.Course.Name);
        }
    }
}
