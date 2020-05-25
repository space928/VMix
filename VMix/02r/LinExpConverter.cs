using System;
using System.Linq;

namespace VMix
{
    /// <summary>
    /// Calculates the exponential response of the faders. This is approximate, see the excel spreadsheet detailing research into this.
    /// This class should be implemented differently for each mixer.
    /// </summary>
    public static class LinExpConvert
    {
        public static double Convert(double value, double min, double max, bool normalizeIn = true, bool normalizeOut = false)
        {
            //return (double)value;
            double x = (double)value;
            if (normalizeIn)
                x = (x - min) / (max - min);
            x = EmulatedLinExpConvert(x);
            if (!normalizeOut)
                x = x * (max - min) + min;

            return x;
        }

        public static double ConvertBack(double value, double min, double max, bool normalizeIn = true, bool normalizeOut = false)
        {
            //return (double)value;
            double x = (double)value;
            if (normalizeIn)
                x = (x - min) / (max - min);
            x = EmulatedLinExpConvertBack(x);
            if (!normalizeOut)
                x = x * (max - min) + min;

            return x;
        }

        //The following are mathematical approximations of the behaviour of the 02r
        private static double MathematicalLinExpConvert(double x)
        {
            return Math.Exp((x - 4.6293) / 19.661);
        }

        private static double MathematicalLinExpConvertBack(double x)
        {
            return 19.661 * Math.Log(x) + 4.6293;
        }

        //The following methods are an emulation of the behaviour of the 02r based on measured values
        private struct LUTItem
        {
            public double x;
            public double y;
        }
        private static LUTItem[] _02rFaderLUT = new LUTItem[] {
            new LUTItem() { x = 0           , y = 0.000000 },
            new LUTItem() { x = 0.00833333  , y = 0.009901 },
            new LUTItem() { x = 0.04166667  , y = 0.405941 },
            new LUTItem() { x = 0.16666667  , y = 0.603960 },
            new LUTItem() { x = 0.30833333  , y = 0.702970 },
            new LUTItem() { x = 0.70000000  , y = 0.851485 },
            new LUTItem() { x = 0.80833333  , y = 0.900990 },
            new LUTItem() { x = 0.90000000  , y = 0.950495 },
            new LUTItem() { x = 1.00000000  , y = 1.000000 }
        };
        /*private static LUTItem[] _02rFaderLUTExpanded = new LUTItem[] {
            new LUTItem() { x = 0           , y=-99 },
            new LUTItem() { x = 0.00833333  , y=-90 },
            new LUTItem() { x = 0.04166667  , y=-50 },
            new LUTItem() { x = 0.16666667  , y=-30 },
            new LUTItem() { x = 0.30833333  , y=-20 },
            new LUTItem() { x = 0.70000000  , y=-5 },
            new LUTItem() { x = 0.80833333  , y=0 },
            new LUTItem() { x = 0.90000000  , y=5 },
            new LUTItem() { x = 1.00000000  , y=10 }
        };
        private static LUTItem[] _02rFaderLUTExpandedReverse = new LUTItem[] {
            new LUTItem() { x = -99.0000    , y = 0.000000 },
            new LUTItem() { x = -98.0917    , y = 0.009901 },
            new LUTItem() { x = -94.4583    , y = 0.405941 },
            new LUTItem() { x = -80.8333    , y = 0.603960 },
            new LUTItem() { x = -65.3917    , y = 0.702970 },
            new LUTItem() { x = -22.7000    , y = 0.851485 },
            new LUTItem() { x = -10.8917    , y = 0.900990 },
            new LUTItem() { x = -0.90000    , y = 0.950495 },
            new LUTItem() { x = 10.00000    , y = 1.000000 }
        };*/

        private static double EmulatedLinExpConvertBack(double x)
        {
            //This works assuming that the LUT is in order.
            LUTItem minBound = _02rFaderLUT.LastOrDefault(y => y.x <= x);
            LUTItem maxBound = _02rFaderLUT.FirstOrDefault(y => y.x > x);

            //Edge case
            if (x >= 1)
                return minBound.y;

            //Interpolate
            double xfrac = (x - minBound.x) / (maxBound.x - minBound.x);
            double interp = minBound.y * (1 - xfrac) + maxBound.y * xfrac;

            return interp;
        }

        private static double EmulatedLinExpConvert(double x)
        {
            //This works assuming that the LUT is in order.
            LUTItem minBound = _02rFaderLUT.LastOrDefault(y => y.y <= x);
            LUTItem maxBound = _02rFaderLUT.FirstOrDefault(y => y.y > x);

            //Edge case
            if (x >= 1)
                return minBound.x;

            //Interpolate
            double xfrac = (x - minBound.y) / (maxBound.y - minBound.y);
            double interp = minBound.x * (1 - xfrac) + maxBound.x * xfrac;

            return interp;
        }
    }
}
