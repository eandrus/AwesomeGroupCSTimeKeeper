using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSTimeKeeper.Models
{
    public class Chart
    {
        public string type = "doughnut";
        public Data data { get; set; }
        public Options options { get; set; }

        public Chart()
        {
            data = new Data();
            options = new Options();
        }
    }

    public class Data
    {
        public Dataset[] datasets { get; set; }
        public string[] labels { get; set; }

        public Data()
        {
            datasets = new Dataset[1];
        }
    }

    public class Dataset
    {
        public double[] data { get; set; }
        public string[] backgroundColor { get; set; }
        public string label = "DonutChart";
    }

    public class Options
    {
        public bool responsive = true;
        public Legend legend { get; set; }
        public Title title { get; set; }
        public Animation animation { get; set; }

        public Options()
        {
            legend = new Legend();
            title = new Title();
            animation = new Animation();
        }
    }

    public class Legend
    {
        public bool display = true;
        public string position = "right";
    }

    public class Title
    {
        public bool display = false;
        public string text { get; set; }
    }

    public class Animation
    {
        public bool animateScale = true;
        public bool animateRotate = true;
    }
}
