using CSTimeKeeper.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using CSTimeKeeper.Extensions;

namespace CSTimeKeeper.Models
{
    public class Project : IComparable<Project>
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Project Name")]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}", ApplyFormatInEditMode = false)]
        public DateTime Deadline { get; set; }

        public bool Active { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public List<Group> Groups { get; set; }

        public List<GroupMember> Members { get; set; }

        [NotMapped]
        public string Status
        {
            get
            {
                return Active ? "Active" : "Inactive";
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
                    chart.options.title.text = "Project Breakdown - " + Name;

                    chart.data.datasets[0] = new Dataset
                    {
                        data = new double[Groups.Count],
                        backgroundColor = new string[Groups.Count]
                    };
                    chart.data.labels = new string[Groups.Count];

                    //get average group times
                    for (int i = 0; i < Groups.Count; i++)
                    {
                        var group = Groups[i];
                        chart.data.datasets[0].data[i] = group.AverageHours;
                        chart.data.datasets[0].backgroundColor[i] = ColorStore.GetColorShade(i);
                        chart.data.labels[i] = $"{ group.AverageHours } ♦ { group.Name }";
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

        [NotMapped]
        public double TotalHours
        {
            get
            {
                try
                {
                    double total = 0;

                    foreach (Group group in Groups)
                        total += group.TotalHours;

                    return total;
                }
                catch (Exception)
                {
                    return 0;
                }

            }
        }

        [NotMapped]
        public bool IsPastdue
        {
            get
            {
                return Deadline < DateTime.Now;
            }
        }

        public bool StudentInProject(string userId)
        {
            foreach (GroupMember member in Members)
                if (member.StudentId == userId)
                    return true;

            return false;
        }

        public Project() => CreatedDate = DateTime.Now;

        #region Navigation Loading Functions

        public void LoadCourse(TimeKeeperDbContext context)
        {
            context.Entry(this).Reference(p => p.Course).Load();
        }

        public void LoadGroups(TimeKeeperDbContext context)
        {
            context.Entry(this).Collection(p => p.Groups).Load();
        }

        /// <summary>
        /// Load all navigation properties, cascading down to all the things
        /// </summary>
        /// <param name="context"></param>
        public void LoadFullCascade(TimeKeeperDbContext context)
        {
            LoadCourse(context);
            LoadGroups(context);

            foreach (Group group in Groups)
            {
                group.LoadFullCascade(context);
            }
        }

        #endregion

        /// <summary>
        /// Sort by Deadline
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Project other)
        {
            return Deadline.CompareTo(other.Deadline);
        }

    }
}
