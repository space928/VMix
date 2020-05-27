using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace VMix
{
    public enum ChannelType
    {
        Channels,
        Auxes,
        MixBusses,
        Stereo,
        Special
    }

    //Important terminology: ChannelIndex=index of channel starting from 0; ChannelNumber=Always starts from 1
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SetProperty<T>(ref T member, T val,
         [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(member, val)) return;

            member = val;
            this.OnPropertyChanged(propertyName);

            //Property notification propagation test
            if (member.GetType().IsSubclassOf(typeof(BindableBase)))
                (member as BindableBase).PropertyChanged += BindableBase_PropertyChanged;
        }

        private void BindableBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(sender) + "." + e.PropertyName);
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            //Console.WriteLine("Property change: " + propertyName);
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        //Will probably fail
        /*public void ForcePropertyChangeNotification(string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }*/
    }

    [Serializable]
    public class Mixer : BindableBase
    {
        //Fields
        private ObservableCollection<MixChannel> _mixChannels = new ObservableCollection<MixChannel>();
        private ObservableCollection<AuxChannel> _auxChannels = new ObservableCollection<AuxChannel>();
        private ObservableCollection<MixBus> _mixBusses = new ObservableCollection<MixBus>();
        private StereoChannel _stOut;
        private ObservableCollection<DCA> _DCAs = new ObservableCollection<DCA>();

        /// <summary>
        /// This field is set to true if the last property set on this object was done by a remote midi device. It must be set to false as soon as the remote device receives the property change notification.
        /// </summary>
        public bool remoteSet = false;

        //Properties
        public ObservableCollection<MixChannel> MixChannels
        {
            get { return _mixChannels; }
            set { SetProperty(ref _mixChannels, value); }
        }

        public ObservableCollection<AuxChannel> AuxChannels
        {
            get { return _auxChannels; }
            set { SetProperty(ref _auxChannels, value); }
        }

        public ObservableCollection<MixBus> MixBusses
        {
            get { return _mixBusses; }
            set { SetProperty(ref _mixBusses, value); }
        }

        public StereoChannel STOut
        {
            get { return _stOut; }
            set { SetProperty(ref _stOut, value); }
        }

        public ObservableCollection<DCA> DCAs
        {
            get { return _DCAs; }
            set { SetProperty(ref _DCAs, value); }
        }

        //Constructors
        public Mixer()
        {
            for (int i = 0; i < 8; i++)
                DCAs.Add(new DCA(i));
        }

        public Mixer(int mixChannels, int auxChannels, int mixBusses)
        {
            for (int i = 0; i < mixChannels; i++)
                this._mixChannels.Add(new MixChannel(i, auxChannels, mixBusses));
            for (int i = 0; i < auxChannels; i++)
                this._auxChannels.Add(new AuxChannel(i));
            for (int i = 0; i < mixBusses; i++)
                this._mixBusses.Add(new MixBus(i));
            for (int i = 0; i < 8; i++)
                DCAs.Add(new DCA(i));
            this._stOut = new StereoChannel();
        }
    }

    [Serializable]
    public class Channel : BindableBase
    {
        private DoubleParameter faderLevel = new DoubleParameter(0, -99, 10);
        private BoolParameter on = new BoolParameter();
        private BoolParameter solo = new BoolParameter();

        private ScribbleStrip scribbleStrip = new ScribbleStrip();
        private BoolParameter selected = false;

        private int channelIndex;
        private ChannelType channelType;

        //Properties
        public DoubleParameter FaderLevel
        {
            get { return faderLevel; }
            set { SetProperty(ref faderLevel, value); }
        }
        private void FaderLevel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(sender) + e.PropertyName);
        }
        public BoolParameter On
        {
            get { return on; }
            set { SetProperty(ref on, value); }
        }
        public BoolParameter Selected
        {
            get { return selected; }
            set { SetProperty(ref selected, value); }
        }
        public BoolParameter Solo
        {
            get { return solo; }
            set { SetProperty(ref solo, value); }
        }
        public ScribbleStrip ScribbleStrip
        {
            get { return scribbleStrip; }
            set { SetProperty(ref scribbleStrip, value); }
        }
        public int ChannelIndex
        {
            get { return channelIndex; }
            set { SetProperty(ref channelIndex, value); OnPropertyChanged(nameof(ChannelNoString)); }
        }
        public ChannelType ChannelType
        {
            get { return channelType; }
            set { SetProperty(ref channelType, value); OnPropertyChanged(nameof(ChannelNoString)); }
        }
        public string ChannelNoString
        {
            get
            {
                switch (ChannelType)
                {
                    case ChannelType.Channels:
                        return (channelIndex + 1).ToString();
                    case ChannelType.Auxes:
                        return "AUX " + (channelIndex + 1).ToString();
                    case ChannelType.MixBusses:
                        return "BUS " + (channelIndex + 1).ToString();
                    case ChannelType.Stereo:
                        return "STEREO";
                    default:
                        return (channelIndex + 1).ToString();
                }
            }
        }

        public double FaderLevel01
        {
            get { return LinExpConvert.Convert(FaderLevel.Value, FaderLevel.Min, FaderLevel.Max, true, true); }//(FaderLevel.Value - FaderLevel.Min)/(FaderLevel.Max-FaderLevel.Min); }
            set { FaderLevel.Value = LinExpConvert.ConvertBack(value, FaderLevel.Min, FaderLevel.Max, false, false); OnPropertyChanged("FaderLevel"); }
        }

        public Channel()
        {
            this.on.Value = true;
            this.solo.Value = false;
        }

        public Channel(int channelIndex)
        {
            this.channelIndex = channelIndex;
            this.on.Value = true;
            this.solo.Value = false;
        }

        public Channel(Channel src)
        {
            faderLevel = new DoubleParameter(src.faderLevel);
            on = new BoolParameter(src.on);
            solo = new BoolParameter(src.solo);

            scribbleStrip = new ScribbleStrip(src.scribbleStrip);
            selected = new BoolParameter(src.selected);

            channelIndex = src.channelIndex;
            channelType = src.channelType;
        }

        //Hashing this ways ensures we hash based on channel identity not on channel content
        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + channelIndex.GetHashCode();
            hash = hash * 31 + channelType.GetHashCode();
            return hash;
        }

        public double GetPostDCAFaderLevel01(DCA[] dcas)
        {
            if (dcas.Length == 0)
                return FaderLevel01;

            //Inclusive OR mixing. If any of the DCAs are up then the fader level is up
            double x = FaderLevel01;

            if (!dcas.Any(y => y.AssignedChannels.Contains(this)))
                return x;

            double att = 1;
            foreach (DCA dca in dcas)
                if (dca.AssignedChannels.Contains(this))
                    att *= 1 - dca.FaderLevel01;
            att = 1 - att;
            return x * att;

            //Simple AND mixing (multiplicative). Requires all dcas to be up for the fader level to be up.
            //TODO: Make this a preference
            /*double x = FaderLevel01;
            foreach (DCA dca in dcas)
                x *= dca.FaderLevel01;
            return x;*/
        }

        public void SetPostDCAFaderLevel(DCA[] dcas, double newVal)
        {
            if (dcas.Length == 0)
            {
                FaderLevel01 = newVal;
                return;
            }

            //Inclusive OR mixing. If any of the DCAs are up then the fader level is up
            double x = newVal;

            if (!dcas.Any(y => y.AssignedChannels.Contains(this)))
            {
                FaderLevel01 = newVal;
                return;
            }

            double att = 1;
            foreach (DCA dca in dcas)
                if (dca.AssignedChannels.Contains(this))
                    att *= 1 - dca.FaderLevel01;
            att = 1 - att;
            FaderLevel01 = newVal / att;
        }
    }

    public class ChannelSend : BindableBase
    {
        private EnumParameter<ChannelType> targetType;
        private IntParameter targetIndex;
        private DoubleParameter level;
        private BoolParameter on = false;

        public EnumParameter<ChannelType> TargetType
        {
            get { return targetType; }
            set { SetProperty(ref targetType, value); }
        }
        public IntParameter TargetIndex
        {
            get { return targetIndex; }
            set { SetProperty(ref targetIndex, value); }
        }
        public DoubleParameter Level
        {
            get { return level; }
            set { SetProperty(ref level, value); }
        }
        public BoolParameter On
        {
            get { return on; }
            set { SetProperty(ref on, value); }
        }

        public ChannelSend(int index, ChannelType type)
        {
            targetIndex = index;
            targetType = type;
            level = 0;
            level.Min = -99;
            level.Max = 10;
            on = true;
        }

        public ChannelSend(ChannelSend src)
        {
            targetIndex = new IntParameter(src.targetIndex);
            targetType = new EnumParameter<ChannelType>(src.targetType);
            level = new DoubleParameter(src.level);
            on = new BoolParameter(src.on);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + Level.GetHashCode();
            hash = hash * 31 + On.GetHashCode();
            hash = hash * 31 + TargetType.GetHashCode();
            hash = hash * 31 + TargetIndex.GetHashCode();
            return hash;
        }
    }

    public class ChannelRouting : BindableBase
    {
        private IntParameter busIndex;
        private BoolParameter on = false;

        public IntParameter BusIndex
        {
            get { return busIndex; }
            set { SetProperty(ref busIndex, value); }
        }
        public BoolParameter On
        {
            get { return on; }
            set { SetProperty(ref on, value); }
        }

        public ChannelRouting(int index)
        {
            busIndex = index;
            on = false;
            if (index == -1)
                on = true;
        }

        public ChannelRouting(ChannelRouting src)
        {
            busIndex = new IntParameter(src.busIndex);
            on = new BoolParameter(src.on);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + BusIndex.GetHashCode();
            hash = hash * 31 + On.GetHashCode();
            return hash;
        }
    }

    public class MixChannel : Channel
    {
        //### Channel Properties ###
        private DoubleParameter pan;
        //Here we could have consolidated everything into the sendLevels array but this makes it more complicated to bind to (since both busses and auxes are in the same array and ItemsControl doesn't have filtering) henceforth we shall use two arrays.
        private ObservableCollection<ChannelSend> sendLevels = new ObservableCollection<ChannelSend>();
        private ObservableCollection<ChannelRouting> routing = new ObservableCollection<ChannelRouting>();
        private EQ eq;
        private Dynamics dynamics;
        //TODO: Implement insert FX
        private string insertFX;

        //Note: Properties are marked as virtual so that they can be overriden by the viewmodel
        public virtual DoubleParameter Pan
        {
            get { return pan; }
            set { SetProperty(ref pan, value); }
        }
        public virtual ObservableCollection<ChannelSend> SendLevels
        {
            get { return sendLevels; }
            set { SetProperty(ref sendLevels, value); }
        }
        public virtual ObservableCollection<ChannelRouting> Routing
        {
            get { return routing; }
            set { SetProperty(ref routing, value); }
        }
        public virtual EQ Eq
        {
            get { return eq; }
            set { SetProperty(ref eq, value); }
        }
        public virtual Dynamics Dynamics
        {
            get { return dynamics; }
            set { SetProperty(ref dynamics, value); }
        }

        public MixChannel(int channelIndex) : base(channelIndex)
        {
            ChannelType = ChannelType.Channels;
            Pan = 0;
            Pan.Min = -50;
            Pan.Max = 50;
            Eq = new EQ()
            {
                On = true,
                Gain = 0,
                Mix = 1,
                EqBands = new ObservableCollection<EQ.Band>()
                {
                    //TODO: Import EQ bands and param minmax
                    new EQ.Band()//The defaults are fine for now
                    {

                    },
                    new EQ.Band()
                    {

                    },
                    new EQ.Band()
                    {

                    },
                    new EQ.Band()
                    {

                    }
                }
            };
            Dynamics = new Dynamics()
            {

            };
        }

        public MixChannel(int channelIndex, int auxes, int mixBusses) : base(channelIndex)
        {
            ChannelType = ChannelType.Channels;
            for (int i = 0; i < mixBusses; i++)
                routing.Add(new ChannelRouting(i));
            //Add STOut routing
            routing.Add(new ChannelRouting(-1));
            for (int i = 0; i < auxes; i++)
                sendLevels.Add(new ChannelSend(i, ChannelType.Auxes));

            Pan = 0;
            Pan.Min = -50;
            Pan.Max = 50;
            Eq = new EQ()
            {
                On = true,
                Gain = 0,
                Mix = 1,
                EqBands = new ObservableCollection<EQ.Band>()
                {
                    //TODO: Import EQ bands and param minmax
                    new EQ.Band()//The defaults are fine for now
                    {

                    },
                    new EQ.Band()
                    {

                    },
                    new EQ.Band()
                    {

                    },
                    new EQ.Band()
                    {

                    }
                }
            };
            Dynamics = new Dynamics()
            {

            };
        }

        public MixChannel(MixChannel src) : base(src)
        {
            pan = new DoubleParameter(src.pan);
            sendLevels = new ObservableCollection<ChannelSend>(src.sendLevels.Select(x => new ChannelSend(x)));
            routing = new ObservableCollection<ChannelRouting>(src.routing.Select(x => new ChannelRouting(x)));
            eq = new EQ(src.eq);
            dynamics = new Dynamics(src.dynamics);
            //TODO: Implement insert FX
            //private string insertFX;
        }
    }

    public class AuxChannel : Channel
    {
        public AuxChannel(int channelIndex) : base(channelIndex)
        {
            ChannelType = ChannelType.Auxes;
        }
    }

    public class MixBus : Channel
    {
        public MixBus(int channelIndex) : base(channelIndex)
        {
            ChannelType = ChannelType.MixBusses;
        }
    }

    public class StereoChannel : Channel
    {
        private DoubleParameter pan;
        private EQ eq;
        private Dynamics dynamics;

        public DoubleParameter Pan
        {
            get { return pan; }
            set { SetProperty(ref pan, value); }
        }
        public EQ Eq
        {
            get { return eq; }
            set { SetProperty(ref eq, value); }
        }
        public Dynamics Dynamics
        {
            get { return dynamics; }
            set { SetProperty(ref dynamics, value); }
        }

        public StereoChannel()
        {
            ChannelType = ChannelType.Stereo;
        }
    }

    public class EQ : BindableBase
    {
        public enum BandType
        {
            Peak = 1,
            Shelf = 2,
            Cut = 0
        };

        //TODO: Flesh out EQ band
        public class Band : BindableBase
        {
            private DoubleParameter q = new DoubleParameter(0,0.1,10);
            private DoubleParameter freq = new DoubleParameter(0,-18,18);
            private DoubleParameter gain = new DoubleParameter(125, 20, 20000);
            private EnumParameter<BandType> bandType = new EnumParameter<BandType>(EQ.BandType.Peak);

            public DoubleParameter Gain
            {
                get { return gain; }
                set { SetProperty(ref gain, value); }
            }
            public DoubleParameter Q
            {
                get { return q; }
                set { SetProperty(ref q, value); }
            }
            public DoubleParameter Freq
            {
                get { return freq; }
                set { SetProperty(ref freq, value); }
            }
            public EnumParameter<BandType> BandType
            {
                get { return bandType; }
                set { SetProperty(ref bandType, value); }
            }

            public Band()
            {

            }

            public Band(Band src)
            {
                gain = new DoubleParameter(src.gain);
                q = new DoubleParameter(src.q);
                freq = new DoubleParameter(src.freq);
                bandType = new EnumParameter<BandType>(src.bandType);
            }
        }

        private ObservableCollection<Band> eqBands = new ObservableCollection<Band>();
        private BoolParameter on = true;
        private DoubleParameter mix = 1;
        private DoubleParameter gain = 0;

        public EQ()
        {

        }

        public EQ(EQ src)
        {
            eqBands = new ObservableCollection<Band>(src.eqBands.Select(x => new Band(x)));
            on = new BoolParameter(src.on);
            mix = new DoubleParameter(src.mix);
            gain = new DoubleParameter(src.gain);
        }

        public ObservableCollection<Band> EqBands
        {
            get { return eqBands; }
            set { SetProperty(ref eqBands, value); }
        }
        public BoolParameter On
        {
            get { return on; }
            set { SetProperty(ref on, value); }
        }
        public DoubleParameter Mix
        {
            get { return mix; }
            set { SetProperty(ref mix, value); }
        }
        public DoubleParameter Gain
        {
            get { return gain; }
            set { SetProperty(ref gain, value); }
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + Mix.GetHashCode();
            hash = hash * 31 + On.GetHashCode();
            hash = hash * 31 + Gain.GetHashCode();
            //TODO: This might have to be improved to refelect the hashcodes of the contents not the identity of the collection
            hash = hash * 31 + EqBands.GetHashCode();
            return hash;
        }
    }

    public class Dynamics : BindableBase
    {

        public Dynamics()
        {

        }

        public Dynamics(Dynamics dynamics)
        {

        }
    }

    public class DCA : BindableBase
    {
        private DoubleParameter faderLevel = new DoubleParameter(0, -99, 0);
        private BoolParameter on = new BoolParameter();
        private ObservableCollection<Channel> assignedChannels = new ObservableCollection<Channel>();
        private ScribbleStrip scribbleStrip = new ScribbleStrip();

        private int dcaIndex;

        //Properties
        public DoubleParameter FaderLevel
        {
            get { return faderLevel; }
            set
            {
                var old = faderLevel;
                SetProperty(ref faderLevel, value);

                // Remove the event subscription from the old instance.
                if (old != null) old.PropertyChanged -= FaderLevel_PropertyChanged;

                // Add the event subscription to the new instance.
                if (faderLevel != null) faderLevel.PropertyChanged += FaderLevel_PropertyChanged;
            }
        }
        private void FaderLevel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(sender) + e.PropertyName);
        }
        public BoolParameter On
        {
            get { return on; }
            set { SetProperty(ref on, value); }
        }
        public ScribbleStrip ScribbleStrip
        {
            get { return scribbleStrip; }
            set { SetProperty(ref scribbleStrip, value); }
        }
        public ObservableCollection<Channel> AssignedChannels
        {
            get { return assignedChannels; }
            set { SetProperty(ref assignedChannels, value); }
        }
        public int DCAIndex
        {
            get { return dcaIndex; }
            set { SetProperty(ref dcaIndex, value); }
        }

        public double FaderLevel01
        {
            get { return LinExpConvert.Convert(FaderLevel.Value, FaderLevel.Min, FaderLevel.Max, true, true); }//(FaderLevel.Value - FaderLevel.Min)/(FaderLevel.Max-FaderLevel.Min); }
            set { FaderLevel.Value = LinExpConvert.ConvertBack(value, FaderLevel.Min, FaderLevel.Max, false, false); OnPropertyChanged("FaderLevel"); }
        }

        public DCA()
        {
            On = true;
        }

        public DCA(int index)
        {
            On = true;
            DCAIndex = index;
        }

        public void AssignChannelByID(Mixer mixer, int index, ChannelType channelType)
        {
            if (mixer.MixChannels.Any(y => y.ChannelIndex == index && y.ChannelType == channelType)
                && !AssignedChannels.Any(y => y.ChannelIndex == index && y.ChannelType == channelType))
                AssignedChannels.Add(mixer.MixChannels.First(x => x.ChannelIndex == index && x.ChannelType == channelType));
        }

        public void AssignChannel(Channel channel)
        {
            if (!AssignedChannels.Contains(channel))
                AssignedChannels.Add(channel);
        }

        public void RemoveChannelByID(Mixer mixer, int index, ChannelType channelType)
        {
            if (mixer.MixChannels.Any(y => y.ChannelIndex == index && y.ChannelType == channelType))
                AssignedChannels.Remove(mixer.MixChannels.First(x => x.ChannelIndex == index && x.ChannelType == channelType));
        }

        public void RemoveChannel(Channel channel)
        {
            AssignedChannels.Add(channel);
        }
    }

    public class ScribbleStrip : BindableBase
    {
        private StringParameter content;
        private ColorParameter color;
        private DoubleParameter fontSize = 14;
        //Don't know if this is strictly speaking mvvm, but I'm trying to keep it's use limited...
        private ICommand scribbleStripCommand;

        public StringParameter Content
        {
            get { return content; }
            set { SetProperty(ref content, value); }
        }
        public ColorParameter BackgroundColor
        {
            get { return color; }
            set { SetProperty(ref color, value); }
        }
        public DoubleParameter FontSize
        {
            get { return fontSize; }
            set { SetProperty(ref fontSize, value); }
        }

        public ICommand ScribbleStripCommand
        {
            get
            {
                return scribbleStripCommand ?? (scribbleStripCommand = new ViewModel.MixerCommand((object param) => ExecuteScribbleStripCommand(param), () => true));
            }
        }

        private void ExecuteScribbleStripCommand(object param)
        {
            float mul = 0.95f;
            switch ((string)param)
            {
                case "SizeUp":
                    FontSize.Value *= 1.2;
                    break;
                case "SizeDown":
                    FontSize.Value /= 1.2;
                    break;
                case "ColRed":
                    BackgroundColor = Color.Multiply(Color.FromArgb(170, 255, 5, 10), mul);
                    break;
                case "ColYellow":
                    BackgroundColor = Color.Multiply(Color.FromArgb(170, 240, 240, 0), mul);
                    break;
                case "ColGreen":
                    BackgroundColor = Color.Multiply(Color.FromArgb(170, 0, 240, 15), mul);
                    break;
                case "ColAqua":
                    BackgroundColor = Color.Multiply(Color.FromArgb(170, 2, 240, 180), mul);
                    break;
                case "ColBlue":
                    BackgroundColor = Color.Multiply(Color.FromArgb(170, 5, 15, 250), mul);
                    break;
                case "ColPurple":
                    BackgroundColor = Color.Multiply(Color.FromArgb(170, 180, 5, 240), mul);
                    break;
                case "ColDefault":
                    BackgroundColor = (System.Windows.Application.Current.Resources["ControlFill"] as SolidColorBrush).Color;
                    break;
                default:
                    break;
            }
        }

        public ScribbleStrip()
        {
            Content = string.Empty;
            BackgroundColor = (System.Windows.Application.Current.Resources["ControlFill"] as SolidColorBrush).Color;
            FontSize = 18;
        }

        public ScribbleStrip(ScribbleStrip src)
        {
            content = new StringParameter(src.content);
            color = new ColorParameter(src.color);
            fontSize = new DoubleParameter(src.fontSize);
            scribbleStripCommand = src.scribbleStripCommand;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + FontSize.GetHashCode();
            hash = hash * 31 + Content.GetHashCode();
            hash = hash * 31 + BackgroundColor.GetHashCode();
            return hash;
        }
    }
}