using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace VMix.ViewModel
{
    [ValueConversion(typeof(object), typeof(object))]
    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Debugger.Break();
            Console.WriteLine("Binding data (to binding): " + value + " // Type: " + value.GetType() + " -> " + targetType + " // Target: " + parameter);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Debugger.Break();
            Console.WriteLine("Binding data (from binding): " + value + " Type: " + value.GetType() + " <- " + targetType + " // Target: " + parameter);
            return value;
        }
    }

    [ValueConversion(typeof(EQ.BandType), typeof(bool))]
    public class EQBandTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            EQ.BandType bandType = (EQ.BandType)Enum.Parse(typeof(EQ.BandType), parameter.ToString());
            return ((EQ.BandType)value) == bandType;
        }

        //We can only return a known value if the value selected is the value of the control
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)value)
            {
                return (EQ.BandType)Enum.Parse(typeof(EQ.BandType), parameter.ToString());
            }
            return null;
        }
    }

    /// <summary>
    /// Converts and index to a number by adding 1 and casting to a string. Will also concatinate the value of the parameter before the value.
    /// For special values of the index (value < 0) then specific conversions are applied
    /// </summary>
    [ValueConversion(typeof(int), typeof(int))]
    public class IndexToNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Special conversions
            if ((int)value == -1)
                return "ST";
            //Generic conversion
            if(parameter != null && (int)value < 0)
                return (string)parameter;
            else if(parameter != null)
                return (string)parameter + ((int)value + 1);
            else
                return (int)value + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value - 1;
        }
    }

    /// <summary>
    /// Converts an array of bools and an index into a single bool.
    /// </summary>
    [ValueConversion(typeof(bool[]), typeof(bool))]
    public class BoolArrayIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(parameter as string, out int bInd) && value != null)
            {
                return ((bool[])value)[bInd];
            }
            return false;
        }

        //This conversion is not possible
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(int), typeof(bool))]
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(parameter as string, out int bInd) && value != null)
            {
                return ((int)value) == bInd;
            }
            return false;
        }

        //This conversion is not possible
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)value)
            {
                return int.Parse(parameter as string);
            }
            return null;
        }
    }

    /// <summary>
    /// Applies the slight exponential falloff to the faders
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class LinExpConverter : IValueConverter
    {
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double Power { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return (double)value;
            double x = (double)value;

            x = LinExpConvert.Convert(x, Minimum, Maximum);

            return x;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return (double)value;
            double x = (double)value;

            x = LinExpConvert.ConvertBack(x, Minimum, Maximum);

            return x;
        }
    }

    /// <summary>
    /// Applies the slight exponential falloff to the faders
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class KnobValueToAngleConverter : IValueConverter
    {
        private int arcEndAngle = 120;
        private int arcStartAngle = -120;

        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public bool LinToExp { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return (double)value;
            double x = (double)value;

            if(LinToExp)
                x = LinExpConvert.Convert(x, Minimum, Maximum);

            x = (arcEndAngle - arcStartAngle) / (Maximum - Minimum) * (x - Minimum) + arcStartAngle;

            return x;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return (double)value;
            double x = (double)value;

            if (LinToExp)
                x = LinExpConvert.ConvertBack(x, Minimum, Maximum);

            x = (x-arcStartAngle) / ((arcEndAngle - arcStartAngle) / (Maximum - Minimum)) + Minimum;

            return x;
        }
    }

    /// <summary>
    /// Converts a title, a unit and a value into a single display string
    /// Arguments:
    /// 0 - Title
    /// 1 - Value (double)
    /// 2 - Unit
    /// 3 - Decimal Places (int)
    /// 4 - Metric Truncation (bool)
    /// </summary>
    [ValueConversion(typeof(object[]), typeof(string))]
    public class KnobTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string ret = values[0] as string;

            // for Value Label
            ret += "\n" + Math.Round((double)values[1] / (((bool)values[4] && ((double)values[1]) > 1000) ? 1000 : 1), (int)values[3]).ToString();

            if (((string)values[2]).Length > 0)
            {
                ret += "[" + (((bool)values[4] && ((double)values[1] > 1000)) ? "K" : "") + (string)values[2] + "]";
            }

            return ret;
        }

        //No way
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Takes an input value and a unit and concatinates them
    /// </summary>
    [ValueConversion(typeof(int), typeof(int))]
    public class ValueAndUnitConverter : IValueConverter
    {
        public int DecimalPlaces { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string o = Math.Round((double)value, DecimalPlaces).ToString();

            if (((string)parameter).Length > 0)
            {
                o += " [" + ((string)parameter) + "]";
            }

            return o;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Converts between a BackgroundColor and a SolidColorBrush
    /// </summary>
    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((SolidColorBrush)value).Color;
        }
    }

    /// <summary>
    /// Converts between a BackgroundColor and a SolidColorBrush
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class DarkModeToThemePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value)?"DarkTheme":"LightTheme";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }

        public static string StaticConvert(bool value)
        {
            return value ? "DarkTheme" : "LightTheme";
        }

        public static bool StaticConvertBack(string value)
        {
            return true;
        }
    }

    /// <summary>
    /// Converts between a BackgroundColor and a SolidColorBrush
    /// </summary>
    [ValueConversion(typeof(int), typeof(string))]
    public class IndexToMixerProfileConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] profiles = SettingsManager.GetMixerProfiles();
            return Array.IndexOf(profiles, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] profiles = SettingsManager.GetMixerProfiles();
            return profiles[(int)value];
        }
    }
}
