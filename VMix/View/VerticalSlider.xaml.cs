using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VMix
{
    /// <summary>
    /// Interaction logic for VerticalSlider.xaml
    /// </summary>
    [DefaultEvent("ValueChanged"), DefaultProperty("Value")]
    public partial class VerticalSlider : UserControl
    {
        public VerticalSlider()
        {
            InitializeComponent();
        }

        //private double lastFaderHeight = 0;
        private MouseButtonEventHandler valueTypeInCloseHandler;

        //Component Dependancy Properties
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(VerticalSlider));
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(VerticalSlider));
        public static readonly DependencyProperty SmallIncrementProperty = DependencyProperty.Register("SmallIncrement", typeof(double), typeof(VerticalSlider));
        public static readonly DependencyProperty LargeIncrementProperty = DependencyProperty.Register("LargeIncrement", typeof(double), typeof(VerticalSlider));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(VerticalSlider), new PropertyMetadata(0d, new PropertyChangedCallback(OnSliderValueChanged), new CoerceValueCallback(CoerceSliderValue)));
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(VerticalSlider));
        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit", typeof(string), typeof(VerticalSlider));
        public static readonly DependencyProperty TickSpacingProperty = DependencyProperty.Register("TickSpacing", typeof(double), typeof(VerticalSlider));
        public static readonly DependencyProperty ValueSpacingProperty = DependencyProperty.Register("ValueSpacing", typeof(double), typeof(VerticalSlider));
        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register("DecimalPlaces", typeof(int), typeof(VerticalSlider));
        //public static readonly DependencyProperty ValueChangedProp =    DependencyProperty.Register("ValueChanged",     typeof(RoutedPropertyChangedEventHandler<double>), typeof(VerticalSlider));

        private static readonly RoutedEvent ValueChangeEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerticalSlider));

        //Events
        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangeEvent, value); }
            remove { RemoveHandler(ValueChangeEvent, value); }
        }

        //Properties
        [Description("Gets or sets title for the Vertical Slider."), Category("Vertical Slider")]
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); Update(); }
        }

        [Description("Gets or sets unit text for the Vertical Slider."), Category("Vertical Slider")]
        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); Update(); }
        }

        [Description("Gets or sets value for the Vertical Slider."), Category("Vertical Slider")]
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
                Update();

                RaiseEvent(new RoutedEventArgs(ValueChangeEvent));
            }
        }

        [Description("Gets or sets the minimum value for the Vertical Slider. It can not be more than the maximum."), Category("Vertical Slider")]
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); VSlider.Minimum = value; DrawValueLabels(); Update(); }
        }

        [Description("Gets or sets the maximum value for the Vertical Slider. It can not be less than the maximum."), Category("Vertical Slider")]
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); VSlider.Maximum = value; DrawValueLabels(); Update(); }
        }

        [Description("Gets or sets the number of decimal places for the Vertical Slider."), Category("Vertical Slider")]
        public int DecimalPlaces
        {
            get { return (int)GetValue(DecimalPlacesProperty); }
            set { SetValue(DecimalPlacesProperty, value); Update(); }
        }

        [Description("Gets or sets the smallest increment for the Vertical Slider."), Category("Vertical Slider")]
        public double SmallIncrement
        {
            get { return (double)GetValue(SmallIncrementProperty); }
            set { SetValue(SmallIncrementProperty, value); Update(); }
        }

        [Description("Gets or sets the quick scrubbing increment for the Vertical Slider."), Category("Vertical Slider")]
        public double LargeIncrement
        {
            get { return (double)GetValue(LargeIncrementProperty); }
            set { SetValue(LargeIncrementProperty, value); Update(); }
        }

        [Description("Gets or sets the the distance between ticks for the Vertical Slider."), Category("Vertical Slider")]
        public double TickSpacing
        {
            get { return (double)GetValue(TickSpacingProperty); }
            set { SetValue(TickSpacingProperty, value); Update(); }
        }

        [Description("Gets or sets the the distance between values for the Vertical Slider."), Category("Vertical Slider")]
        public double ValueSpacing
        {
            get { return (double)GetValue(ValueSpacingProperty); }
            set { SetValue(ValueSpacingProperty, value); DrawValueLabels(); Update(); }
        }

        /// <summary>
        /// Clamp the input between min and max.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="baseValue"></param>
        /// <returns></returns>
        private static object CoerceSliderValue(DependencyObject d, object baseValue)
        {
            VerticalSlider v = (VerticalSlider)d;
            double x = (double)baseValue;

            x = x < v.Minimum ? v.Minimum : x;
            x = x > v.Maximum ? v.Maximum : x;
            return x;
        }

        private static void OnSliderValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VerticalSlider v = (VerticalSlider)d;
            d.CoerceValue(ValueProperty);
        }

        public void DrawValueLabel(double value)
        {
            if (Maximum - Minimum <= 0)
                return;

            Label nLabel = new Label();
            nLabel.Content = Math.Round(LinExpConvert.ConvertBack(value, Minimum, Maximum, true, false), DecimalPlaces).ToString();
            nLabel.FontSize = 8;
            nLabel.HorizontalAlignment = HorizontalAlignment.Stretch;
            nLabel.VerticalAlignment = VerticalAlignment.Bottom;
            nLabel.Foreground = (SolidColorBrush)Application.Current.Resources["Text"];
            nLabel.Tag = "ValueLabel";
            nLabel.VerticalContentAlignment = VerticalAlignment.Center;
            nLabel.Padding = new Thickness(0);

            Slider s = (Slider)FindName("VSlider");
            double newY = (value / (Maximum - Minimum)) * (s.ActualHeight - 20) - (s.ActualHeight - 20) * (Minimum / (Maximum - Minimum)) + 10;//+5 to account for internal padding in the slider
            newY -= (nLabel.FontSize + nLabel.Padding.Bottom + nLabel.Padding.Top) / 2;//This should work to calculate the height of the element
            //Console.WriteLine("Creating value label @" + newY + " //Value=" + LinToExp(value).ToString());

            nLabel.Margin = new Thickness(0, 0, 0, newY);
            Grid g = (Grid)FindName("SliderGrid");
            Grid.SetRow(nLabel, 0);
            Grid.SetColumn(nLabel, 2);

            g.Children.Add(nLabel);
        }

        //TODO: Improve speed
        public void DrawValueLabels()
        {
            if (ValueSpacing <= 0)
                return;

            //Delete old labels
            Label[] oldLabels = ((Grid)FindName("SliderGrid")).Children.OfType<Label>().Where<Label>(x => (string)x.Tag == "ValueLabel").ToArray();
            foreach (Label l in oldLabels)
                ((Grid)FindName("SliderGrid")).Children.Remove(l);

            //Place new labels at regular linear interval
            int valuesToPlace = (int)Math.Floor((Maximum - Minimum) / ValueSpacing);
            //Splitting into two ensures that 0 will always be marked
            for (double i = Math.Max(Minimum, 0); i <= Maximum + .1; i += ValueSpacing)
            {
                DrawValueLabel(i);
            }
            for (double i = Minimum; i < 0; i += ValueSpacing)
            {
                DrawValueLabel(i);
            }
        }

        public void Update()
        {
            // for Title Label
            try
            {
                VSliderLabel.Text = string.Empty;
                if (Title.Length > 0)
                {
                    VSliderLabel.Text = Title;
                }

                // for Value Label
                VSliderLabel.Text += "\n" + Math.Round(Value, DecimalPlaces).ToString();

                if (Unit.Length > 0)
                {
                    VSliderLabel.Text += "[" + Unit + "]";
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Slider Not Initialised Yet!");
            }
        }

        //TODO: Not working
        private void SliderGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox valueEntry = ((TextBox)this.FindName("SliderValueEntry"));
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
            TextBox valueEntry = ((TextBox)this.FindName("SliderValueEntry"));
            if (e != null)
            {
                if (Mouse.DirectlyOver == valueEntry)
                    return;
            }
            double tmpVal;
            double.TryParse(valueEntry.Text, out tmpVal);
            tmpVal = Math.Max(Math.Min(tmpVal, Maximum), Minimum);
            Value = tmpVal;
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
            //TODO: VS Designer still doesn't really like this
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                if (Application.Current?.MainWindow != null)
                    Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }

        //Update labels if the window changes size
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawValueLabels();
        }

        private void VSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Update();
        }
    }
}
