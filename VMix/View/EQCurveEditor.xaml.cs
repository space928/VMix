using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VMix
{
    /// <summary>
    /// Interaction logic for EQCurveEditor.xaml
    /// </summary>
    public partial class EQCurveEditor : UserControl
    {
        private int curveResolution = 300;
        private readonly double W_FACTOR = 1000;

        public static readonly DependencyProperty GainRangeProperty = DependencyProperty.Register("GainRange", typeof(double), typeof(EQCurveEditor));
        public static readonly DependencyProperty FreqMinProperty = DependencyProperty.Register("FreqMin", typeof(double), typeof(EQCurveEditor), new PropertyMetadata(new PropertyChangedCallback(OnFreqMinChanged)));
        public static readonly DependencyProperty FreqMaxProperty = DependencyProperty.Register("FreqMax", typeof(double), typeof(EQCurveEditor), new PropertyMetadata(new PropertyChangedCallback(OnFreqMaxChanged)));
        public static readonly DependencyProperty EQObjectProperty = DependencyProperty.Register("EQObject", typeof(EQ), typeof(EQCurveEditor), new PropertyMetadata(new PropertyChangedCallback(OnEQChanged)));
        public static readonly DependencyProperty MultipleDataProperty = DependencyProperty.Register("MultipleData", typeof(bool), typeof(EQCurveEditor));

        public double GainRange
        {
            get { return (double)GetValue(GainRangeProperty); }
            set { SetValue(GainRangeProperty, value); }
        }
        public double FreqMin
        {
            get { return (double)GetValue(FreqMinProperty); }
            set { SetValue(FreqMinProperty, value); }
        }
        public double FreqMax
        {
            get { return (double)GetValue(FreqMaxProperty); }
            set { SetValue(FreqMaxProperty, value); }
        }
        public EQ EQObject
        {
            get { return (EQ)GetValue(EQObjectProperty); }
            set { SetValue(EQObjectProperty, value); }
        }
        public bool MultipleData
        {
            get { return (bool)GetValue(MultipleDataProperty); }
            set { SetValue(MultipleDataProperty, value); }
        }

        public EQCurveEditor()
        {
            InitializeComponent();
        }

        public void UpdateEQCurvePoints()
        {
            PointCollection curve = new PointCollection();
            for(int i = 0; i < curveResolution; i++)
            {
                double x = (i/(float)curveResolution)*(FreqMax-FreqMin)+FreqMin;
                double y = 0;

                if (EQObject?.EqBands != null)
                {
                    foreach (EQ.Band band in EQObject.EqBands)
                    {
                        y -= band.BandType.value switch
                        {
                            EQBandType.Peak => CalculateBellEQGain(band, x),
                            EQBandType.Cut => CalculateFilterEQGain(band, x),
                            EQBandType.Shelf => CalculateShelfEQGain(band, x),
                            _ => throw new NotImplementedException(),
                        };
                    }
                }

                x = ((x - FreqMin) / (FreqMax - FreqMin)) * EQCurveCanvas.ActualWidth;
                y = Math.Clamp((y + GainRange) / (GainRange*2), 0, 1) * EQCurveCanvas.ActualHeight;

                curve.Add(new Point(x,y));
            }

            EQCurveObject.Points = curve;
        }

        private double CalculateBellEQGain(EQ.Band band, double x)
        {
            double freq = LinExpConvert.Convert(band.Freq, band.Freq.Min, band.Freq.Max);
            double gain = LinExpConvert.Convert(band.Gain, band.Gain.Min, band.Gain.Max);
            double q = band.Q;//LinExpConvert.Convert(band.Q, band.Q.Min, band.Q.Max);
            return gain * Math.Exp(-((x-freq)*(x - freq))/(2*(W_FACTOR/q)*(W_FACTOR / q)));
        }

        private double CalculateShelfEQGain(EQ.Band band, double x)
        {
            double freq = LinExpConvert.Convert(band.Freq, band.Freq.Min, band.Freq.Max);
            double gain = LinExpConvert.Convert(band.Gain, band.Gain.Min, band.Gain.Max);
            double q = band.Q;// LinExpConvert.Convert(band.Q, band.Q.Min, band.Q.Max);
            return gain / (1 + Math.Exp(-((x - freq) /(2*W_FACTOR))));
        }

        private double CalculateFilterEQGain(EQ.Band band, double x)
        {
            double freq = LinExpConvert.Convert(band.Freq, band.Freq.Min, band.Freq.Max);
            double gain = LinExpConvert.Convert(band.Gain, band.Gain.Min, band.Gain.Max);
            double q = band.Q;// LinExpConvert.Convert(band.Q, band.Q.Min, band.Q.Max);
            return -Math.Exp(-(x-freq)/(1.5*W_FACTOR));
        }

        private void AddEventHandlers()
        {
            AddLocalPropertyChangeEventHandlers(EQObject);
        }

        private void AddLocalPropertyChangeEventHandlers(object o, string parentAddr = "")
        {
            //Add an event handler to this object since this is called recursively, it's children will also get handlers
            //if (typeof(INotifyPropertyChanged).IsAssignableFrom(o.GetType()))
            if (o.GetType().IsSubclassOf(typeof(Parameter)))
            {
                //Console.WriteLine("[->VM] Added property change notification handler to local: " + parentAddr);
                if (typeof(INotifyPropertyChanged).IsAssignableFrom(o.GetType()))
                    ((INotifyPropertyChanged)o).PropertyChanged += (sender, e) => UpdateEQCurvePoints();
            }
            //Find all the properties which have bindable properties
            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                if (string.IsNullOrEmpty(parentAddr) && p.DeclaringType != typeof(EQ))
                    continue;

                if (typeof(INotifyPropertyChanged).IsAssignableFrom(p.PropertyType))
                {
                    if (typeof(System.Collections.ICollection).IsAssignableFrom(o.GetType()))
                    {
                        //If the property is a collection, add event handlers to each of it's elements
                        int i = 0;
                        foreach (object oi in (System.Collections.ICollection)o)
                        {
                            if (((INotifyPropertyChanged)oi) == null)
                                continue;

                            string newAddr = string.IsNullOrEmpty(parentAddr) ? "" : ($"{parentAddr}[{i}]");

                            //Recursively add more handlers
                            AddLocalPropertyChangeEventHandlers(oi, newAddr);
                            i++;
                        }
                    }
                    else
                    {
                        object propVal = p.GetValue(o);
                        //Add event listners
                        if (((INotifyPropertyChanged)propVal) == null)
                            continue;

                        string newAddr = (string.IsNullOrEmpty(parentAddr) ? "" : (parentAddr + ".")) + p.Name;

                        //Recursively add more handlers
                        AddLocalPropertyChangeEventHandlers(propVal, newAddr);
                    }
                }
            }
        }

        private static void OnFreqMinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EQCurveEditor)d).UpdateEQCurvePoints();
        }

        private static void OnFreqMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EQCurveEditor)d).UpdateEQCurvePoints();
        }

        private static void OnEQChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EQCurveEditor)d).UpdateEQCurvePoints();
            ((EQCurveEditor)d).AddEventHandlers();
        }
    }
}
