using CSTimeKeeper.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using System.Linq;

namespace CSTimeKeeper.Models
{
    public class GroupMember
    {
        public int Id { get; set; }

        [ForeignKey("Student")]
        public string StudentId { get; set; }
        public User Student { get; set; }

        public int StudentCourseId { get; set; }
        public StudentCourse StudentCourse { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        [NotMapped]
        public double TotalHours
        {
            get
            {
                double total = 0;
                foreach (TimeEntry entry in TimeEntries)
                    total += entry.TotalHours;
                return total;
            }
        }

        public List<TimeEntry> TimeEntries { get; set; }

        [NotMapped]
        public string StudentName
        {
            get
            {
                try
                {
                    return Student.FullName;
                }
                catch (Exception)
                {
                    return "";
                }

            }
        }

        public GroupMember() => TimeEntries = new List<TimeEntry>();

        #region Navigation Loading Functions

        /// <summary>
        /// Loads the student and course navigation properties
        /// </summary>
        /// <param name="context"></param>
        public void LoadParents(TimeKeeperDbContext context)
        {
            context.Entry(this).Reference(g => g.Group).Load();
            context.Entry(this).Reference(g => g.Project).Load();
            context.Entry(this).Reference(g => g.StudentCourse).Load();
            context.Entry(this).Reference(g => g.Student).Load();
        }

        public void LoadTimeEntries(TimeKeeperDbContext context)
        {
            context.Entry(this).Collection(g => g.TimeEntries).Load();
        }

        /// <summary>
        /// Load all navigation properties, cascading down to all the things
        /// </summary>
        /// <param name="context"></param>
        public void LoadFullCascade(TimeKeeperDbContext context)
        {
            LoadParents(context);
            LoadTimeEntries(context);
            context.Entry(StudentCourse).Reference(s => s.Course).Load();
            StudentCourse.Student = Student;

            List<TimeEntryChange> changes = context.TimeEntryChange.Where(c => c.TimeEntry.GroupMemberId == Id).ToList();
            foreach (TimeEntry entry in TimeEntries)
            {
                entry.TimeEntryChanges = changes.Where(c => c.TimeEntryId == entry.Id).ToList();

                if (entry.TimeEntryChanges != null)
                    foreach (TimeEntryChange change in entry.TimeEntryChanges)
                        change.TimeEntry = entry;
            }
        }

        #endregion
    }
}
