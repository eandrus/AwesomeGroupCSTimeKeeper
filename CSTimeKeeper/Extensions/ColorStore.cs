using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSTimeKeeper.Extensions
{
    public static class ColorStore
    {
        private static Random rng = new Random();

        /// <summary>
        /// Returns a random color in string format
        /// </summary>
        /// <param name="rng"></param>
        /// <returns></returns>
        public static string GetRandomColor()
        {
            return $"rgb({rng.Next(255)},{rng.Next(255)},{rng.Next(255)})";
        }

        public static string GetColorShade(int index)
        {
            switch(index % 7)
            {
                case 0:
                    return $"rgb({rng.Next(128) + 128},{rng.Next(75)},{rng.Next(75)})";
                case 1:
                    return $"rgb({rng.Next(75)},{rng.Next(128) + 128},{rng.Next(75)})";
                case 2:
                    return $"rgb({rng.Next(75)},{rng.Next(75)},{rng.Next(128) + 128})";
                case 3:
                    return $"rgb({rng.Next(128) + 128},{rng.Next(128) + 128},{rng.Next(75)})";
                case 4:
                    return $"rgb({rng.Next(75)},{rng.Next(128) + 128},{rng.Next(128) + 128})";
                case 5:
                    return $"rgb({rng.Next(128) + 128},{rng.Next(75)},{rng.Next(128) + 128})";
                default:
                    return $"rgb({rng.Next(128) + 128},{rng.Next(128) + 128},{rng.Next(128) + 128})";
            }
        }
    }
}
