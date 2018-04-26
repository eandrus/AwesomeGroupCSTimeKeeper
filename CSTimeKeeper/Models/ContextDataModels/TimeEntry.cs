using CSTimeKeeper.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace CSTimeKeeper.Models
{
    public class TimeEntry
    {
        public int Id { get; set; }

        [Required]
        public int GroupMemberId { get; set; }
        public GroupMember GroupMember { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(200)]
        public string Justification { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Stop Time")]
        public DateTime StopTime { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        [NotMapped]
        public double TotalHours
        {
            get
            {
                return Math.Round((StopTime - StartTime).TotalHours, 2);
            }
        }

        /// <summary>
        /// Allow modification for 7 days after creation
        /// </summary>
        public bool AllowModify
        {
            get
            {
                return DateTime.Now < CreatedDate.AddDays(7);
            }
        }

        public List<TimeEntryChange> TimeEntryChanges { get; set; }

        public TimeEntry() => CreatedDate = DateTime.Now;

        #region Navigation Loading Functions

        public void LoadGroupMember(TimeKeeperDbContext context)
        {
            context.Entry(this).Reference(t => t.GroupMember).Load();
        }

        public void LoadChanges(TimeKeeperDbContext context)
        {
            context.Entry(this).Collection(t => t.TimeEntryChanges).Load();
        }

        /// <summary>
        /// Loads the parent class, cascading all the way up to Instructor
        /// </summary>
        /// <param name="context"></param>
        public void LoadCascadeParents(TimeKeeperDbContext context)
        {
            LoadGroupMember(context);
            context.Entry(GroupMember).Reference(m => m.Group).Load();
            context.Entry(GroupMember.Group).Reference(g => g.Project).Load();
            context.Entry(GroupMember.Group.Project).Reference(p => p.Course).Load();
            context.Entry(GroupMember.Group.Project.Course).Reference(c => c.Instructor).Load();
            context.Entry(GroupMember).Reference(m => m.Student).Load();
        }

        #endregion
    }

    public class TimeEntryChange
    {
        public int Id { get; set; }

        [Required]
        public int TimeEntryId { get; set; }
        public TimeEntry TimeEntry { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StopTime { get; set; }

        public DateTime CreatedDate { get; set; }

        [NotMapped]
        public double TotalHours
        {
            get
            {
                return Math.Round((StopTime - StartTime).TotalHours, 2);
            }
        }

        public TimeEntryChange() => CreatedDate = DateTime.Now;

        #region Navigation Loading Functions

        public void LoadTimeEntry(TimeKeeperDbContext context)
        {
            context.Entry(this).Reference(c => c.TimeEntry).Load();
        }

        #endregion
    }
}
