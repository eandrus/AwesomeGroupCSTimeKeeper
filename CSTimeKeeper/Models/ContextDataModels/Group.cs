using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CSTimeKeeper.Data;
using System.Linq;
using CSTimeKeeper.Extensions;
using Newtonsoft.Json;

namespace CSTimeKeeper.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Group Name")]
        public string Name { get; set; }

        [StringLength(65)]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public List<GroupMember> Members { get; set; }

        public bool StudentInGroup(string UserId)
        {
            foreach (GroupMember member in Members)
            {
                if (member.StudentId == UserId)
                    return true;
            }

            return false;
        }

        [NotMapped]
        public double TotalHours
        {
            get
            {
                try
                {
                    double total = 0;
                    foreach (GroupMember member in Members)
                        total += member.TotalHours;
                    return total;
                }
                catch (Exception)
                {
                    return 0;
                }

            }
        }

        [NotMapped]
        public double AverageHours
        {
            get
            {
                try
                {
                    if (Members.Count < 1)
                        return 0;

                    double total = 0;
                    foreach (GroupMember member in Members)
                        total += member.TotalHours;
                    return Math.Round(total / Members.Count, 2);
                }
                catch (Exception)
                {
                    return 0;
                }

            }
        }

        [NotMapped]
        public string PieChart
        {
            get
            {
                try
                {
                    Chart chart = new Chart();
                    chart.options.title.display = true;
                    chart.options.title.text = "Group Breakdown - " + Name;

                    chart.data.datasets[0] = new Dataset
                    {
                        data = new double[Members.Count],
                        backgroundColor = new string[Members.Count]
                    };
                    chart.data.labels = new string[Members.Count];

                    //get average group times
                    for (int i = 0; i < Members.Count; i++)
                    {
                        var member = Members[i];
                        chart.data.datasets[0].data[i] = member.TotalHours;
                        chart.data.datasets[0].backgroundColor[i] = ColorStore.GetColorShade(i);
                        chart.data.labels[i] = $"{ member.TotalHours } ♦ { member.Student.FullName }";
                    }
                    chart.options.legend.display = true;

                    return JsonConvert.SerializeObject(chart);
                }
                catch (Exception ex)
                {
                    return $"Error generating JSON: { ex.Message }";
                }

            }
        }

        public Group() => CreatedDate = DateTime.Now;

        #region Navigation Loading Functions

        public void LoadProject(TimeKeeperDbContext context)
        {
            context.Entry(this).Reference(g => g.Project).Load();
        }

        public void LoadMembers(TimeKeeperDbContext context)
        {
            context.Entry(this).Collection(g => g.Members).Load();
        }

        /// <summary>
        /// Load all navigation properties, cascading down to all the things
        /// </summary>
        /// <param name="context"></param>
        public void LoadFullCascade(TimeKeeperDbContext context)
        {
            LoadProject(context);
            LoadMembers(context);

            List<TimeEntry> entries = context.TimeEntry.Where(e => e.GroupMember.GroupId == Id).ToList();
            List<TimeEntryChange> changes = context.TimeEntryChange.Where(c => c.TimeEntry.GroupMember.GroupId == Id).ToList();

            foreach (GroupMember member in Members)
            {
                context.Entry(member).Reference(m => m.Student).Load();
                member.TimeEntries = entries.Where(e => e.GroupMemberId == member.Id).ToList();

                foreach (TimeEntry entry in member.TimeEntries)
                {
                    entry.TimeEntryChanges = changes.Where(c => c.TimeEntryId == entry.Id).ToList();
                }
            }
        }

        #endregion
    }
}
