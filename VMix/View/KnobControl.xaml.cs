/// Simple WPF/C# Knob Control 
/// Author: n37jan (n37jan@gmail.com) & Thomas
/// License: MIT License

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace VMix
{
    /// <summary>
    /// Interaction logic for KnobControl.xaml
    /// </summary>
    [DefaultEvent("ValueChanged"), DefaultProperty("Value")]
    public partial class KnobControl : UserControl
    {
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePos()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        #region Private Fields
        private bool isMouseDown = false;
        private Point previousMousePosition;
        private double mouseMoveThreshold = 3;
        private bool init = true;
        private MouseButtonEventHandler valueTypeInCloseHandler;
        #endregion Private Fields

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(KnobControl));
        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit", typeof(string), typeof(KnobControl));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(KnobControl), new PropertyMetadata(0d, new PropertyChangedCallback(OnKnobValueChanged), new CoerceValueCallback(CoerceKnobValue)));
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(KnobControl));
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(KnobControl));
        public static readonly DependencyProperty StepProperty = DependencyProperty.Register("Step", typeof(double), typeof(KnobControl));
        public static readonly DependencyProperty LabelFontSizeProperty = DependencyProperty.Register("LabelFontSize", typeof(double), typeof(KnobControl));
        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register("DecimalPlaces", typeof(int), typeof(KnobControl));
        public static readonly DependencyProperty MetricTruncationProperty = DependencyProperty.Register("MetricTruncation", typeof(bool), typeof(KnobControl));
        public static readonly DependencyProperty ExponentialProperty = DependencyProperty.Register("Exponential", typeof(bool), typeof(KnobControl));
        public static readonly DependencyProperty MultipleDataProperty = DependencyProperty.Register("MultipleData", typeof(bool), typeof(KnobControl));

        private static readonly RoutedEvent KnobValueChangedEvent = EventManager.RegisterRoutedEvent("KnobValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(KnobControl));

        public KnobControl()
        {
            InitializeComponent();
        }

        //Events
        public event RoutedEventHandler KnobValueChanged
        {
            add { AddHandler(KnobValueChangedEvent, value); }
            remove { RemoveHandler(KnobValueChangedEvent, value); }
        }

        //Propertis
        [Description("Gets or sets title for the knob control."), Category("Knob Control")]
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        [Description("Gets or sets unit text for the knob control."), Category("Knob Control")]
        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }
        [Description("Gets or sets value for the knob control."), Category("Knob Control")]
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        [Description("Gets or sets the minimum value for the knob control. It can not be more than the maximum."), Category("Knob Control")]
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        [Description("Gets or sets the maximum value for the knob control. It can not be less than the maximum."), Category("Knob Control")]
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        [Description("Gets or sets a step for the knob control."), Category("Knob Control")]
        public double Step
        {
            get { return (double)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }
        [Description("Gets or sets a font size for the knob control."), Category("Knob Control")]
        public double LabelFontSize
        {
            get { return (double)GetValue(LabelFontSizeProperty); }
            set { SetValue(LabelFontSizeProperty, value); }
        }
        [Description("Gets or sets the number of decimal places for the knob control."), Category("Knob Control")]
        public int DecimalPlaces
        {
            get { return (int)GetValue(DecimalPlacesProperty); }
            set { SetValue(DecimalPlacesProperty, value); }
        }
        [Description("Gets or sets whether or not to show the value in thousands when necessary for the knob control."), Category("Knob Control")]
        public bool MetricTruncation
        {
            get { return (bool)GetValue(MetricTruncationProperty); }
            set { SetValue(MetricTruncationProperty, value); }
        }
        [Description("Gets or sets whether or not the knob has an exponential response."), Category("Knob Control")]
        public bool Exponential
        {
            get { return (bool)GetValue(ExponentialProperty); }
            set { SetValue(ExponentialProperty, value); }
        }
        [Description("Gets or sets whether or not the knob represents multiple values."), Category("Knob Control")]
        public bool MultipleData
        {
            get { return (bool)GetValue(MultipleDataProperty); }
            set { SetValue(MultipleDataProperty, value); }
        }

        private static object CoerceKnobValue(DependencyObject d, object baseValue)
        {
            KnobControl v = (KnobControl)d;
            double x = (double)baseValue;

            x = x < v.Minimum ? v.Minimum : x;
            x = x > v.Maximum ? v.Maximum : x;
            return x;
        }

        private static void OnKnobValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            KnobControl v = (KnobControl)d;
            d.CoerceValue(ValueProperty);
        }

        private void Ellipse_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double d = e.Delta / 120; // Mouse wheel 1 click (120 delta) = 1 step
            d *= 2; //Just make it a bit more sensitive
            double nVal = Value;
            if (Exponential)
                nVal = LinExpConvert.Convert(nVal, Minimum, Maximum);
            nVal += Math.Sign(d) * Step * SettingsManager.Settings.KnobSensistivity;
            if (Exponential)
                nVal = LinExpConvert.ConvertBack(Math.Clamp(nVal, Minimum, Maximum), Minimum, Maximum);
            SetCurrentValue(ValueProperty, nVal);
        }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            (sender as Ellipse).CaptureMouse();
            previousMousePosition = e.GetPosition((Ellipse)sender);
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point newMousePosition = GetMousePos();// e.GetPosition((Ellipse)sender); This is better

                double dY = previousMousePosition.Y - newMousePosition.Y;//(previousMousePosition.Y - newMousePosition.Y);

                if (newMousePosition.Y >= System.Windows.SystemParameters.PrimaryScreenHeight - 1)
                {
                    int x = (int)newMousePosition.X;
                    int y = 0;
                    SetCursorPos(x, y);
                    previousMousePosition.Y = -1;
                }
                else if (newMousePosition.Y <= 1)
                {
                    int x = (int)newMousePosition.X;
                    int y = (int)System.Windows.SystemParameters.PrimaryScreenHeight;
                    SetCursorPos(x, y);
                    previousMousePosition.Y = System.Windows.SystemParameters.PrimaryScreenHeight + 1;
                }

                if (Math.Abs(dY) > mouseMoveThreshold)
                {
                    double aval = Math.Abs(Value);
                    //Value += Math.Sign(dY) * Step * SettingsManager.Settings.KnobSensistivity;
                    //Conver to linear space before adding the change to create the exponential response
                    double nVal = Value;
                    if (Exponential)
                        nVal = LinExpConvert.Convert(nVal, Minimum, Maximum);
                    nVal += Math.Sign(dY) * Step * SettingsManager.Settings.KnobSensistivity;
                    if(Exponential)
                        nVal = LinExpConvert.ConvertBack(Math.Clamp(nVal, Minimum, Maximum), Minimum, Maximum);

                    if (double.IsNaN(nVal))
                        nVal = 0;

                    SetCurrentValue(ValueProperty, nVal);
                    previousMousePosition = newMousePosition;
                }
            }
        }

        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
            (sender as Ellipse).ReleaseMouseCapture();
        }

        private void Ellipse_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox valueEntry = ((TextBox)this.FindName("KnobValueEntry"));
            valueEntry.Text = Math.Round(Value, DecimalPlaces).ToString();
            valueEntry.Visibility = Visibility.Visible;
            valueTypeInCloseHandler = new MouseButtonEventHandler(CloseValueTypeIn);
            AddHandler(Mouse.MouseDownEvent, valueTypeInCloseHandler, true);
            Mouse.Capture(this);
        }

        private void CloseValueTypeIn(object sender, MouseButtonEventArgs e)
        {
            CloseValueTypeIn(sender, e, true);
        }

        private void CloseValueTypeIn(object sender, MouseButtonEventArgs e, bool setVal)
        {
            TextBox valueEntry = ((TextBox)this.FindName("KnobValueEntry"));
            if (e != null)
            {
                if (Mouse.DirectlyOver == valueEntry)
                    return;
            }
            double tmpVal;
            if (double.TryParse(valueEntry.Text, out tmpVal))
            {
                tmpVal = Math.Max(Math.Min(tmpVal, Maximum), Minimum);
                Value = tmpVal;
                SetCurrentValue(ValueProperty, tmpVal);
            }
            valueEntry.Visibility = Visibility.Hidden;

            ReleaseMouseCapture();
            RemoveHandler(Mouse.MouseDownEvent, valueTypeInCloseHandler);
        }

        private void KnobValueEntry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CloseValueTypeIn(this, null);
            else if (e.Key == Key.Escape)
                CloseValueTypeIn(this, null, false);
        }
    }
}