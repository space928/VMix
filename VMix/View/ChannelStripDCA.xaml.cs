using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VMix.ViewModel;

namespace VMix
{
    /// <summary>
    /// Interaction logic for ChannelStripDCA.xaml
    /// </summary>
    public partial class ChannelStripDCA : UserControl
    {
        //Dep Properties
        public static readonly DependencyProperty ChannelNumberProperty = DependencyProperty.Register("ChannelNumber", typeof(int), typeof(ChannelStripDCA));
        public static readonly DependencyProperty ChannelLabelProperty = DependencyProperty.Register("ChannelLabel", typeof(string), typeof(ChannelStripDCA));
        public static readonly DependencyProperty OnProperty = DependencyProperty.Register("On", typeof(bool), typeof(ChannelStripDCA));
        public static readonly DependencyProperty SoloProperty = DependencyProperty.Register("Solo", typeof(bool), typeof(ChannelStripDCA));
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(ChannelStripDCA));
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(ChannelStripDCA));
        public static readonly DependencyProperty FaderValueProperty = DependencyProperty.Register("FaderValue", typeof(double), typeof(ChannelStripDCA));
        public static readonly DependencyProperty AssignCommandProperty = DependencyProperty.Register("AssignCommand", typeof(MixerCommand), typeof(ChannelStripDCA));
        public static readonly DependencyProperty LabelFormatCommandProperty = DependencyProperty.Register("LabelFormatCommand", typeof(ICommand), typeof(ChannelStripDCA));
        public static readonly DependencyProperty LabelFontSizeProperty = DependencyProperty.Register("LabelFontSize", typeof(double), typeof(ChannelStripDCA));
        public static readonly DependencyProperty LabelBackgroundProperty = DependencyProperty.Register("LabelBackground", typeof(SolidColorBrush), typeof(ChannelStripDCA));

        private static readonly RoutedEvent SelectEvent = EventManager.RegisterRoutedEvent("SelectButton", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChannelStripDCA));
        private static readonly RoutedEvent OnEvent = EventManager.RegisterRoutedEvent("OnButton", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChannelStripDCA));
        private static readonly RoutedEvent SoloEvent = EventManager.RegisterRoutedEvent("SoloButton", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChannelStripDCA));
        private static readonly RoutedEvent FaderEvent = EventManager.RegisterRoutedEvent("FaderSlider", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChannelStripDCA));

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

        public ChannelStripDCA()
        {
            InitializeComponent();

            Init();
        }

        public int ChannelNumber
        {
            get { return (int)GetValue(ChannelNumberProperty); }
            set { SetValue(ChannelNumberProperty, value); }
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
        public MixerCommand AssignCommand
        {
            get { return (MixerCommand)GetValue(AssignCommandProperty); }
            set { SetValue(AssignCommandProperty, value); }
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

        //Handle scribble strip colour changes
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            float mul = 0.5f;// ((MainWindow)Application.Current.MainWindow).darkMode ? 0.5f : 1.5f;

            switch (item.Header as string)
            {
                case "Red":
                    ChannelName.Background = new SolidColorBrush(Color.Multiply(Color.FromRgb(255, 5, 10), mul));
                    break;
                case "Yellow":
                    ChannelName.Background = new SolidColorBrush(Color.Multiply(Color.FromRgb(240, 240, 0), mul));
                    break;
                case "Green":
                    ChannelName.Background = new SolidColorBrush(Color.Multiply(Color.FromRgb(0, 240, 15), mul));
                    break;
                case "Aqua":
                    ChannelName.Background = new SolidColorBrush(Color.Multiply(Color.FromRgb(2, 240, 180), mul));
                    break;
                case "Blue":
                    ChannelName.Background = new SolidColorBrush(Color.Multiply(Color.FromRgb(5, 15, 250), mul));
                    break;
                case "Purple":
                    ChannelName.Background = new SolidColorBrush(Color.Multiply(Color.FromRgb(180, 5, 240), mul));
                    break;
                default:
                    ChannelName.Background = Application.Current.Resources["ControlFill"] as SolidColorBrush;
                    break;
            }
        }

        private void ChannelNameFontSize_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item.Header as string == "Font Size +")
            {
                ChannelName.FontSize *= 1.2;
            }
            else
            {
                ChannelName.FontSize /= 1.2;
            }
        }
    }
}
