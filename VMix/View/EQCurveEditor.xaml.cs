using System;
using System.Collections.Generic;
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

namespace VMix.View
{
    /// <summary>
    /// Interaction logic for EQCurveEditor.xaml
    /// </summary>
    public partial class EQCurveEditor : UserControl
    {
        public static readonly DependencyProperty GainRangeProperty = DependencyProperty.Register("GainRange", typeof(double), typeof(EQCurveEditor), new PropertyMetadata(new PropertyChangedCallback(OnGainRangeChanged)));
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
        public double GainMin
        {
            get { return -(double)GetValue(GainRangeProperty); }
        }
        public PointCollection Points
        {
            get { return GetEQCurvePoints(); }
        }

        private PointCollection GetEQCurvePoints()
        {
            throw new NotImplementedException();
        }

        private static void OnGainRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void OnFreqMinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void OnFreqMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void OnEQChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public EQCurveEditor()
        {
            InitializeComponent();
        }
    }
}
