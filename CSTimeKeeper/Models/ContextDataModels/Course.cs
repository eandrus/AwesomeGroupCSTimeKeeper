using CSTimeKeeper.Data;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace CSTimeKeeper.Models
{
    public class Course : IComparable<Course>
    {
        public int Id { get; set; }

        [ForeignKey("Instructor")]
        public string InstructorId { get; set; }
        public User Instructor { get; set; }

        [Display(Name = "Course Name")]
        public string Name { get; set; }

        public bool Active { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public List<Project> Projects { get; set; }

        public List<StudentCourse> Students { get; set; }

        [NotMapped]
        public string Status
        {
            get
            {
                return Active ? "Active" : "Inactive";
            }
        }

        public Course()
        {
            CreatedDate = DateTime.Now;
        }

        #region Navigation Loading Functions

        public void LoadInstructor(TimeKeeperDbContext context)
        {
            context.Entry(this).Reference(c => c.Instructor).Load();
        }

        public void LoadProjects(TimeKeeperDbContext context)
        {
            context.Entry(this).Collection(c => c.Projects).Load();
        }

        public void LoadStudents(TimeKeeperDbContext context)
        {
            context.Entry(this).Collection(c => c.Students).Load();
        }

        /// <summary>
        /// Load all navigation properties, cascading down to all the things
        /// </summary>
        /// <param name="context"></param>
        public void LoadFullCascade(TimeKeeperDbContext context)
        {
            LoadInstructor(context);
            LoadProjects(context);
            LoadStudents(context);

            List<User> users = context.User.ToList();
            List<Group> groups = context.Group.Where(g => g.Project.CourseId == Id).ToList();
            List<GroupMember> members = context.GroupMember.Where(m => m.Project.CourseId == Id).ToList();
            List<TimeEntry> entries = context.TimeEntry.Where(t => t.GroupMember.Project.Course.Id == Id).ToList();
            List<TimeEntryChange> changes = context.TimeEntryChange.ToList();

            foreach (StudentCourse student in Students)
            {
                student.Course = this;
                student.Student = users.Where(u => u.Id == student.StudentId).FirstOrDefault();
            }

            foreach (Project p in Projects)
            {
                p.Groups = groups.Where(g => g.ProjectId == p.Id).ToList();
                p.Members = members.Where(m => m.ProjectId == p.Id).ToList();

                foreach (Group g in p.Groups)
                {
                    g.Members = members.Where(m => m.GroupId == g.Id).ToList();

                    foreach (GroupMember m in g.Members)
                    {
                        m.StudentCourse = Students.Where(s => s.Id == m.StudentCourseId).FirstOrDefault();
                        if (m.StudentCourse != null)
                            m.Student = m.StudentCourse.Student;
                        m.TimeEntries = entries.Where(t => t.GroupMemberId == m.Id).ToList();

                        if (m.TimeEntries != null)
                            foreach (TimeEntry e in m.TimeEntries)
                            {
                                e.GroupMember = m;
                                e.TimeEntryChanges = changes.Where(c => c.TimeEntryId == e.Id).ToList();
                            }
                    }
                }
            }
        }

        #endregion

        public int CompareTo(Course other)
        {
            return Name.CompareTo(other.Name);
        }
    }
}
