using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMSnake
{
    public static class GameSettings
    {
        public static int BoostSpeed { get; set; } = 50;
        public static double WallDensity { get; set; } = .1;
        public static bool WallFatality { get; set; } = false;
    }
}
