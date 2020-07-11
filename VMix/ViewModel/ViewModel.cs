using Commons.Music.Midi;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace VMix.ViewModel
{
    public class ViewModel : BindableBase
    {
        private ObservableCollection<IMidiPortDetails> _MidiInputDevices = new ObservableCollection<IMidiPortDetails>();
        private ObservableCollection<IMidiPortDetails> _MidiOutputDevices = new ObservableCollection<IMidiPortDetails>();

        private ObservableCollection<Channel> _Faders1To8 = new ObservableCollection<Channel>();
        private ObservableCollection<Channel> _Faders9To16 = new ObservableCollection<Channel>();
        private SelectedItemsViewModel _SelectedChannel;

        private DoubleParameter _EQQ = new DoubleParameter(0,0.1,10);
        private DoubleParameter _EQG = new DoubleParameter(0,-18,18);
        private DoubleParameter _EQF = new DoubleParameter(125,20,20000);
        private int _EQBandType;
        private int _EQSelectedBand;
        private bool[] _EQEnabledBands = new bool[] { true, true, true };

        private SettingsWindow settingsWindow;

        private ObservableCollection<Channel> allChannels = new ObservableCollection<Channel>();

        public enum FaderPage
        {
            Channels1to16,
            Channels17to32,
            Master,
            Aux1,
            Aux2,
            Aux3,
            Aux4,
            Aux5,
            Aux6,
            Aux7,
            Aux8,
            Aux9,
            Aux10,
            Aux11,
            Aux12,
            Aux13,
            Aux14,
            Aux15,
            Aux16,
            Custom1,
            Custom2
        }

        //Here we should handle/define all the view button commands. We should also prepare the data from the model for presentation

        public Mixer VMixer { get; set; }
        public MidiManager MidiManager { get; set; }

        #region ### Bindable things ###
        public ObservableCollection<IMidiPortDetails> MidiInputDevices
        {
            get { return _MidiInputDevices; }
            set { SetProperty(ref _MidiInputDevices, value); }
        }
        public ObservableCollection<IMidiPortDetails> MidiOutputDevices
        {
            get { return _MidiOutputDevices; }
            set { SetProperty(ref _MidiOutputDevices, value); }
        }
        public IMidiPortDetails SelectedMidiInputDevice { get; set; }
        public IMidiPortDetails SelectedMidiOutputDevice { get; set; }
        public IMidiPortDetails ConnectedMidiInputDevice
        {
            get { return MidiManager.GetConnectedInput(); }
        }
        public IMidiPortDetails ConnectedMidiOutputDevice
        {
            get { return MidiManager.GetConnectedOutput(); }
        }

        public ObservableCollection<Channel> Faders1to8
        {
            get { return _Faders1To8; }
            set { SetProperty(ref _Faders1To8, value); }
        }
        public ObservableCollection<Channel> Faders9to16
        {
            get { return _Faders9To16; }
            set { SetProperty(ref _Faders9To16, value); }
        }
        public SelectedItemsViewModel SelectedChannel
        {
            get { return _SelectedChannel; }
            set { SetProperty(ref _SelectedChannel, value); }
        }
        public DoubleParameter EQQ
        {
            get { return _EQQ; }
            set { SetProperty(ref _EQQ, value); }
        }
        public DoubleParameter EQG
        {
            get { return _EQG; }
            set { SetProperty(ref _EQG, value); }
        }
        public DoubleParameter EQF
        {
            get { return _EQF; }
            set { SetProperty(ref _EQF, value); }
        }
        public int EQBandType
        {
            get { return _EQBandType; }
            set { SetProperty(ref _EQBandType, value); }
        }
        public int EQSelectedBand
        {
            get { return _EQSelectedBand; }
            set { SetProperty(ref _EQSelectedBand, value); }
        }
        public bool[] EQEnabledBands
        {
            get { return _EQEnabledBands; }
            set { SetProperty(ref _EQEnabledBands, value); }
        }

        public Settings UserSettings
        {
            get { return SettingsManager.Settings; }
            set { SettingsManager.Settings = value; OnPropertyChanged(nameof(UserSettings)); }
        }
        public string[] MixerProfiles
        {
            get { return SettingsManager.GetMixerProfiles(); }
        }
        #endregion

        #region ### Commands ###
        private ICommand settingsCommand;
        public ICommand SettingsCommand
        {
            get
            {
                return settingsCommand ?? (settingsCommand = new MixerCommand((object param) => OpenSettingsWindow(), () => true));
            }
        }
        //TODO: These are currently the same thing maybe in future the UI should reflect this.
        private ICommand midiInputReloadCommand;
        public ICommand MidiInputReloadCommand
        {
            get
            {
                return midiInputReloadCommand ?? (midiInputReloadCommand = new MixerCommand((object param) => ReloadMidiDevice(), () => true));
            }
        }
        private ICommand midiOutputReloadCommand;
        public ICommand MidiOutputReloadCommand
        {
            get
            {
                return midiOutputReloadCommand ?? (midiOutputReloadCommand = new MixerCommand((object param) => ReloadMidiDevice(), () => true));
            }
        }
        private ICommand switchLayerCommand;
        public ICommand SwitchLayerCommand
        {
            get
            {
                return switchLayerCommand ?? (switchLayerCommand = new MixerCommand((object param) => SetFaderPage(param), () => true));
            }
        }
        private ICommand dcaAssignCommand;
        public ICommand DCAAssignCommand
        {
            get
            {
                return dcaAssignCommand ?? (dcaAssignCommand = new MixerCommand((object param) => AssignSelectionToDCA(param), () => true));
            }
        }
        private ICommand channelSelectCommand;
        public ICommand ChannelSelectCommand
        {
            get
            {
                return channelSelectCommand ?? (channelSelectCommand = new MixerCommand((object param) => UpdateSelectedChannelBindings(), () => true));
            }
        }

        private ICommand eQBandSelectCommand;
        public ICommand EQBandSelectCommand
        {
            get
            {
                return eQBandSelectCommand ?? (eQBandSelectCommand = new MixerCommand((object param) => UpdateEQBindings((string)param), () => true));
            }
        }

        private ICommand darkModeToggleCommand;
        public ICommand DarkModeToggleCommand
        {
            get
            {
                return darkModeToggleCommand ?? (darkModeToggleCommand = new MixerCommand((object param) => SetThemeFromSettings(), () => true));
            }
        }
        #endregion

        #region ### Init and model binding ###
        public ViewModel()
        {
            Init();
        }

        public void Init()
        {
            //Load settings from file
            if (!SettingsManager.LoadSettingsFromFile())
                SettingsManager.SaveSettingsToFile();
            SettingsManager.SaveSettingsOnChange = true;
            ApplySettings();

            //Create a new mixer
            VMixer = new Mixer(SettingsManager.MixerProfile.MixChannels, SettingsManager.MixerProfile.AuxChannels, SettingsManager.MixerProfile.BusChannels);
            //TODO: Add to midi settings such that different managers can be picked
            MidiManager = new MidiManager02r(VMixer);
            SelectedChannel = new SelectedItemsViewModel(new MixChannel(0, SettingsManager.MixerProfile.AuxChannels, SettingsManager.MixerProfile.BusChannels));

            ApplySettingsPostInit();

            CreateMidiBindings();

            //Add existing channels to database
            foreach (Channel c in VMixer.MixChannels)
                allChannels.Add(c);
            foreach (Channel c in VMixer.AuxChannels)
                allChannels.Add(c);
            foreach (Channel c in VMixer.MixBusses)
                allChannels.Add(c);
            allChannels.Add(VMixer.STOut);

            UpdateSelectedChannelBindings(VMixer.STOut);
            UpdateFaderBindings(FaderPage.Channels1to16);
        }

        private void CreateSelectedChannelBindings()
        {
            throw new NotImplementedException();
        }

        private void CreateMidiBindings()
        {
            foreach (Channel m in VMixer.MixChannels)
            {
                m.FaderLevel.PropertyChanged += new PropertyChangedEventHandler((sender, e) => MidiManager.HandleSendFaderMsg(sender, e, m));
                m.On.PropertyChanged += new PropertyChangedEventHandler((sender, e) => MidiManager.HandleSendOnMsg(sender, e, m));

                if (m.GetType() == typeof(MixChannel))
                {
                    //MixChannel specific properties are bound here
                    //((MixChannel)m).eq.on.PropertyChanged += new PropertyChangedEventHandler((sender, e) => MidiManager.HandleSendOnMsg(sender, e, m));
                }
                if (m.GetType() == typeof(AuxChannel))
                {
                    //AuxChannel specific properties are bound here
                }
                if (m.GetType() == typeof(MixBus))
                {
                    //MixBus specific properties are bound here
                }
            }
            foreach (DCA dca in VMixer.DCAs)
            {
                dca.FaderLevel.PropertyChanged += new PropertyChangedEventHandler((sender, e) => MidiManager.HandleDCAFaderUpdate(sender, e, dca));
            }
        }
        #endregion

        #region ### Presentation logic ###
        /// <summary>
        /// Opens a new settings window or destroys and recreates one if it is already open.
        /// Additionally we initiallise some of the data required for the bindings in the settings (Populate the midi devices).
        /// </summary>
        public void OpenSettingsWindow()
        {
            if (settingsWindow?.IsVisible ?? false)//if a settings window is already visible close it and make a new one
                settingsWindow.Close();

            settingsWindow = new SettingsWindow();
            settingsWindow.DataContext = this;
            settingsWindow.Show();

            //Get available midi devices
            MidiInputDevices.Clear();
            foreach (IMidiPortDetails device in MidiManager.GetMidiInputDevices())
                MidiInputDevices.Add(device);
            MidiOutputDevices.Clear();
            foreach (IMidiPortDetails device in MidiManager.GetMidiOutputDevices())
                MidiOutputDevices.Add(device);
        }

        public void ReloadMidiDevice()
        {
            MidiManager.OpenMidiDevice(SelectedMidiInputDevice, SelectedMidiOutputDevice);
            //DEBUG
            //MidiManager.OpenMidiDevice(MidiManager.GetMidiInputDevices().Last(), MidiManager.GetMidiOutputDevices().Last());

            SettingsManager.Settings.LastMidiInputDevice = SelectedMidiInputDevice.Id;
            SettingsManager.Settings.LastMidiOutputDevice = SelectedMidiOutputDevice.Id;

            OnPropertyChanged(nameof(ConnectedMidiInputDevice));
            OnPropertyChanged(nameof(ConnectedMidiOutputDevice));
        }

        private void AssignSelectionToDCA(object param)
        {
            int dcaInd = (int)param;
            //Clear existing assignments
            VMixer.DCAs[dcaInd].AssignedChannels.Clear();
            foreach (Channel c in VMixer.MixChannels)
            {
                if (c.Selected)
                {
                    VMixer.DCAs[dcaInd].AssignChannel(c);
                    c.Selected = false;
                }
            }
        }

        public void SetFaderPage(object param)
        {
            Enum.TryParse((string)param, out FaderPage page);
            UpdateFaderBindings(page);
        }

        /// <summary>
        /// Rebinds the static faders to which ever layer of the mixer they are controlling.
        /// </summary>
        /// <param name="faderMode">Which fader layer is being controlled.</param>
        public void UpdateFaderBindings(FaderPage faderMode)
        {
            DateTime st = DateTime.Now;

            if (VMixer == null)
                return;

            //Faders1to8.Clear();
            //Faders9to16.Clear();
            for (int i = 0; i < 16; i++)
            {
                Channel nChannel = null;

                switch (faderMode)
                {
                    case FaderPage.Channels1to16:
                        nChannel = allChannels.FirstOrDefault(x => x.ChannelIndex == i && x.ChannelType == ChannelType.Channels);
                        break;
                    case FaderPage.Channels17to32:
                        nChannel = allChannels.FirstOrDefault(x => x.ChannelIndex == i + 16 && x.ChannelType == ChannelType.Channels);
                        break;
                    case FaderPage.Master:
                        if (i >= 8)
                            nChannel = allChannels.FirstOrDefault(x => x.ChannelIndex == i - 8 && x.ChannelType == ChannelType.MixBusses);//Mix busses in the last 8, auxes in the first 8
                        else
                            nChannel = allChannels.FirstOrDefault(x => x.ChannelIndex == i && x.ChannelType == ChannelType.Auxes);
                        break;
                    case FaderPage.Custom1:
                        throw new NotImplementedException("Haven't finished adding all these fader switch cases yet...");
                        break;
                    case FaderPage.Custom2:
                        throw new NotImplementedException("Haven't finished adding all these fader switch cases yet...");
                        break;
                    case FaderPage.Aux1:
                        throw new NotImplementedException("Haven't finished adding all these fader switch cases yet...");
                        break;
                    case FaderPage.Aux2:
                        throw new NotImplementedException("Haven't finished adding all these fader switch cases yet...");
                        break;
                    case FaderPage.Aux3:
                        throw new NotImplementedException("Haven't finished adding all these fader switch cases yet...");
                        break;
                    default:
                        throw new NotImplementedException("Haven't finished adding all these fader switch cases yet...");
                        break;
                }

                //Pick which bank to add too
                if (i < 8)
                {
                    if (Faders1to8.Count > i)
                        Faders1to8[i] = nChannel;
                    else
                        Faders1to8.Add(nChannel);
                }
                else
                {
                    if (Faders9to16.Count > i - 8)
                        Faders9to16[i - 8] = nChannel;
                    else
                        Faders9to16.Add(nChannel);
                }
                /*if (i < 8)
                    Faders1to8.Add(nChannel);
                else
                    Faders9to16.Add(nChannel);*/
            }

            Console.WriteLine("Fader generation took: " + (DateTime.Now - st).TotalMilliseconds + "ms.");
        }

        private void ApplySettings()
        {
            SetThemeFromSettings();
            SettingsManager.LoadMixerProfile(SettingsManager.Settings.MixerProfile);
        }

        private void ApplySettingsPostInit()
        {
            //Attempt to open the last midi device
            MidiManager.OpenMidiDevice(SettingsManager.Settings.LastMidiInputDevice, SettingsManager.Settings.LastMidiOutputDevice);
        }

        public void SetThemeFromSettings()
        {
            //The compiler really doesn't like this
            //((App)System.Windows.Application.Current).SetTheme(DarkModeToThemePathConverter.StaticConvert(UserSettings.DarkMode));
            string theme = DarkModeToThemePathConverter.StaticConvert(UserSettings.DarkMode);

            //TODO: This still causes major bugs with the designer.
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(System.Windows.Application.Current.Windows[0]))
                if (System.Windows.Application.Current?.Resources?.MergedDictionaries != null)
                    App.Current.Resources.MergedDictionaries[0].Source = new Uri($"/Themes/{theme}.xaml", UriKind.Relative);
                else
                    Console.WriteLine("Visual Studio XAML Designer is being stupid again...");
        }

        /// <summary>
        /// Binds all the data from the model to the view-model for the selected channel(s).
        /// If a channel is passed in this only binds the data for that channel.
        /// </summary>
        /// <param name="channelToView"></param>
        private void UpdateSelectedChannelBindings(Channel channelToView = null)
        {
            SelectedChannel.SelectedChannels.Clear();
            foreach (Channel c in allChannels)
            {
                if (c.GetType() == typeof(MixChannel))
                    if (c.Selected)
                        SelectedChannel.SelectedChannels.Add(c as MixChannel);
            }
        }

        private void UpdateEQBindings(string bandIndex)
        {
            if (!int.TryParse(bandIndex, out int ind))
                return;
            EQSelectedBand = ind;
            if(SelectedChannel.Eq.EqBands.Count> ind)
            {
                //Generalisation
                if(ind == 0 || ind + 1==SelectedChannel.Eq.EqBands.Count)
                {
                    EQEnabledBands = new bool[] { true, true, true };
                } else
                {
                    EQEnabledBands = new bool[] { false, true, false };
                    EQBandType = (int)VMix.EQBandType.Peak;
                }
                EQQ = SelectedChannel.Eq.EqBands[ind].Q;
                EQF = SelectedChannel.Eq.EqBands[ind].Freq;
                EQG = SelectedChannel.Eq.EqBands[ind].Gain;
            }
            else
            {
                Console.WriteLine("Not enough EQ bands available!");
            }
        }
        #endregion
    }
}
