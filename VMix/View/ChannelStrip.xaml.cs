using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VMix
{
    /// <summary>
    /// Interaction logic for ChannelStrip.xaml
    /// </summary>
    public partial class ChannelStrip : UserControl
    {
        //Dep Properties
        public static readonly DependencyProperty ChannelNumberProperty = DependencyProperty.Register("ChannelNumber", typeof(string), typeof(ChannelStrip));
        public static readonly DependencyProperty ChannelSelectedProperty = DependencyProperty.Register("ChannelSelected", typeof(bool), typeof(ChannelStrip));
        public static readonly DependencyProperty ChannelSelectCommandProperty = DependencyProperty.Register("ChannelSelectCommand", typeof(ICommand), typeof(ChannelStrip));
        public static readonly DependencyProperty ChannelLabelProperty = DependencyProperty.Register("ChannelLabel", typeof(string), typeof(ChannelStrip));
        public static readonly DependencyProperty OnProperty = DependencyProperty.Register("On", typeof(bool), typeof(ChannelStrip));
        public static readonly DependencyProperty SoloProperty = DependencyProperty.Register("Solo", typeof(bool), typeof(ChannelStrip));
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(ChannelStrip));
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(ChannelStrip));
        public static readonly DependencyProperty FaderValueProperty = DependencyProperty.Register("FaderValue", typeof(double), typeof(ChannelStrip));
        public static readonly DependencyProperty LabelFormatCommandProperty = DependencyProperty.Register("LabelFormatCommand", typeof(ICommand), typeof(ChannelStrip));
        public static readonly DependencyProperty LabelFontSizeProperty = DependencyProperty.Register("LabelFontSize", typeof(double), typeof(ChannelStrip));
        public static readonly DependencyProperty LabelBackgroundProperty = DependencyProperty.Register("LabelBackground", typeof(SolidColorBrush), typeof(ChannelStrip));

        private static readonly RoutedEvent SelectEvent = EventManager.RegisterRoutedEvent("SelectButton", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChannelStrip));
        private static readonly RoutedEvent OnEvent = EventManager.RegisterRoutedEvent("OnButton", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChannelStrip));
        private static readonly RoutedEvent SoloEvent = EventManager.RegisterRoutedEvent("SoloButton", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChannelStrip));
        private static readonly RoutedEvent FaderEvent = EventManager.RegisterRoutedEvent("FaderSlider", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChannelStrip));

        //Events
        public event RoutedEventHandler SelectChange
        {
            add { AddHandler(SelectEvent, value); }
            remove { RemoveHandler(SelectEvent, value); }
        }

        public event RoutedEventHandler OnChange
        {
            add { AddHandler(OnEvent, value); }
            remove { RemoveHandler(OnEvent, value); }
        }

        public event RoutedEventHandler SoloChange
        {
            add { AddHandler(SoloEvent, value); }
            remove { RemoveHandler(SoloEvent, value); }
        }

        public event RoutedEventHandler FaderChange
        {
            add { AddHandler(FaderEvent, value); }
            remove { RemoveHandler(FaderEvent, value); }
        }

        public ChannelStrip()
        {
            InitializeComponent();

            Init();
        }

        public string ChannelNumber
        {
            get { return (string)GetValue(ChannelNumberProperty); }
            set { SetValue(ChannelNumberProperty, value); }
        }
        public bool ChannelSelected
        {
            get { return (bool)GetValue(ChannelSelectedProperty); }
            set { SetValue(ChannelSelectedProperty, value); }
        }
        public ICommand ChannelSelectCommand
        {
            get { return (ICommand)GetValue(ChannelSelectCommandProperty); }
            set { SetValue(ChannelSelectCommandProperty, value); }
        }
        public string ChannelLabel
        {
            get { return (string)GetValue(ChannelLabelProperty); }
            set { SetValue(ChannelLabelProperty, value); }
        }
        public bool On
        {
            get { return (bool)GetValue(OnProperty); }
            set { SetValue(OnProperty, value); }
        }
        public bool Solo
        {
            get { return (bool)GetValue(SoloProperty); }
            set { SetValue(SoloProperty, value); }
        }
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public double FaderValue
        {
            get { return (double)GetValue(FaderValueProperty); }
            set { SetValue(FaderValueProperty, value); }
        }
        public ICommand LabelFormatCommand
        {
            get { return (ICommand)GetValue(LabelFormatCommandProperty); }
            set { SetValue(LabelFormatCommandProperty, value); }
        }
        public double LabelFontSize
        {
            get { return (double)GetValue(LabelFontSizeProperty); }
            set { SetValue(LabelFontSizeProperty, value); }
        }
        public SolidColorBrush LabelBackground
        {
            get { return (SolidColorBrush)GetValue(LabelBackgroundProperty); }
            set { SetValue(LabelBackgroundProperty, value); }
        }

        public void Init()
        {

        }

        public void Update()
        {
            ChannelNo.Content = ChannelNumber;
            ChannelFader.Minimum = Minimum;
            ChannelFader.Maximum = Maximum;
            //On = (bool)ChannelOn.IsChecked;
            //Solo = (bool)ChannelSolo.IsChecked;
            //Pan = ChannelPan.Value;
            //FaderValue = ChannelFader.Value;
        }

        //Bubble up some events...
        private void ChannelSelect_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SelectEvent));
        }

        private void ChannelOn_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnEvent));
        }

        private void ChannelSolo_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SoloEvent));
        }

        private void ChannelFader_ValueChanged(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(FaderEvent));
        }
    }
}
