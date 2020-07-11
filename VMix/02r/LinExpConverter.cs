using System;
using System.Diagnostics;
using System.Linq;

namespace VMix
{
    /// <summary>
    /// Calculates the exponential response of the faders. This is approximate, see the excel spreadsheet detailing research into this.
    /// This class should be implemented differently for each mixer.
    /// </summary>
    public static class LinExpConvert
    {
        //Except in the case of the range of -99 to 10, these functions do not handle unbalanced min and max values correctly.
        //Having max=0/min=?; min=0/max=?; max=?/min=-max is fine but for other ranges the full range will not be converted.
        public static double Convert(double value, double min, double max, bool normalizeIn = true, bool normalizeOut = false)
        {
            //return (double)value;
            double x = (double)value;
            /*if (normalizeIn)
                x = (x - min) / (max - min);*/

            if (min == -99 && max == 10)
            {
                if (normalizeIn)
                    x = (x - min) / (max - min);
                x = EmulatedLinExpConvert(x);
                if (!normalizeOut)
                    x = x * (max - min) + min;
            }
            else
            {
                if (normalizeIn)
                    x = (x - Math.Max(min, 0)) / (max - Math.Max(min, 0));
                x = MathematicalLinExpConvert(x);
                if (!normalizeOut)
                    x = x * (max - Math.Max(min, 0)) + Math.Max(min, 0);
            }

            /*if (!normalizeOut)
                x = x * (max - min) + min;*/

            return x;
        }

        public static double ConvertBack(double value, double min, double max, bool normalizeIn = true, bool normalizeOut = false)
        {
            //return (double)value;
            double x = (double)value;
            //if (normalizeIn)
            //    x = (x - min) / (max - min);

            if (min == -99 && max == 10)
            {
                if (normalizeIn)
                    x = (x - min) / (max - min);
                x = EmulatedLinExpConvertBack(x);
                if (!normalizeOut)
                    x = x * (max - min) + min;
            }
            else
            {
                if (normalizeIn)
                    x = (x - Math.Max(min, 0)) / (Math.Max(max, Math.Abs(min)) - Math.Max(min, 0));
                x = MathematicalLinExpConvertBack(x);
                if (!normalizeOut)
                    x = x * (Math.Max(max, Math.Abs(min)) - Math.Max(min, 0)) + Math.Max(min, 0);
            }

            //if (!normalizeOut)
            //    x = x * (max - min) + min;

            return x;
        }

        //The following are mathematical approximations of the behaviour of the 02r
        //TODO: This is super slow when used in the EQ display (part of the preview calculation). Replace with a lookup table.
        private static double MathematicalLinExpConvert(double x)
        {
            //return Math.Exp((x - 4.6293) / 19.661);
            //return Math.Exp((x-0.9468) / 0.1947);
            //Debug.WriteLine("LinExpConvert To: " + x + " => " + Math.Sign(x) * Math.Pow(Math.Abs(x), 3.7));
            //return x * Math.Pow(1.5, 10*(Math.Abs(x)-1)); I want to use this one but it's inverse requires a Lambert function
            return Math.Sign(x) * Math.Pow(Math.Abs(x), 1/3.5);
        }

        private static double MathematicalLinExpConvertBack(double x)
        {
            //return 19.661 * Math.Log(x) + 4.6293;
            //return 0.1947 * Math.Log(x) + 0.9468;
            //Debug.WriteLine("LinExpConvert Back: " + x + " => " + Math.Sign(x) * Math.Pow(Math.Abs(x), 1 / 3.7));
            //This is ugly and still not quite right
            //return (Math.Log(x + 0.01734152992) * 0.1 / Math.Log(1.5) + 1) * 0.995759713646;
            return Math.Sign(x) * Math.Pow(Math.Abs(x), 3.5);
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
