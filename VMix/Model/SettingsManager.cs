using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace VMix
{
    public class SettingsManager : BindableBase
    {
        private static readonly string settingsFileName = "UserSettings.json";
        private static readonly string mixerProfileDir = "Mixer Profiles\\";

        private static Settings settings;
        public static Settings Settings
        {
            get { return settings ?? CreateNewSettings(); }
            set { settings = value; }
        }

        private static MixerProfile mixerProfile;
        public static MixerProfile MixerProfile
        {
            get { return mixerProfile ?? (mixerProfile = new MixerProfile()); }
            set { mixerProfile = value; }
        }
        public static bool SaveSettingsOnChange { get; set; }

        public static bool LoadSettingsFromFile()
        {
            try
            {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFileName));
                settings.PropertyChanged += Settings_PropertyChanged;
            } catch (Exception e)
            {
                Console.WriteLine("Load settings error: ");
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        public static bool SaveSettingsToFile()
        {
            try
            {
                File.WriteAllText(settingsFileName, JsonConvert.SerializeObject(settings, Formatting.Indented));
            }
            catch (Exception e)
            {
                Console.WriteLine("Save settings error: ");
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        public static string[] GetMixerProfiles()
        {
            try
            {
                return Directory.EnumerateFiles(mixerProfileDir).ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine("Load mixer profiles error: ");
                Console.WriteLine(e);
            }

            return new string[] { };
        }

        public static bool LoadMixerProfile(string profileName)
        {
            try
            {
                mixerProfile = JsonConvert.DeserializeObject<MixerProfile>(File.ReadAllText(profileName));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Load mixer profile error: ");
                Console.WriteLine(e);
            }

            return false;
        }

        public static bool DebugCreateMixerProfile(string profileName = "02r.json")
        {
            try
            {
                File.WriteAllText(mixerProfileDir + profileName, JsonConvert.SerializeObject(new MixerProfile(), Formatting.Indented));
            }
            catch (Exception e)
            {
                Console.WriteLine("Load mixer profile error: ");
                Console.WriteLine(e);
            }

            return false;
        }

        private static void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (SaveSettingsOnChange)
                SaveSettingsToFile();
        }

        private static Settings CreateNewSettings()
        {
            settings = new Settings();
            settings.PropertyChanged += Settings_PropertyChanged;
            return settings;
        }
    }

    [Serializable]
    public class Settings : BindableBase
    {
        private bool darkMode = true;
        private float knobSensistivity = 1.0f;
        private string mixerProfile = "02r.json";

        private string lastMidiInputDevice = string.Empty;
        private string lastMidiOutputDevice = string.Empty;

        public bool DarkMode
        {
            get { return darkMode; }
            set { SetProperty(ref darkMode, value); }
        }
        public float KnobSensistivity
        {
            get { return knobSensistivity; }
            set { SetProperty(ref knobSensistivity, value); }
        }
        public string MixerProfile
        {
            get { return mixerProfile; }
            set { SetProperty(ref mixerProfile, value); }
        }

        public string LastMidiInputDevice
        {
            get { return lastMidiInputDevice; }
            set { SetProperty(ref lastMidiInputDevice, value); }
        }
        public string LastMidiOutputDevice
        {
            get { return lastMidiOutputDevice; }
            set { SetProperty(ref lastMidiOutputDevice, value); }
        }
    }

    [Serializable]
    public class MixerProfile
    {
        public int MixChannels { get; set; }
        public int AuxChannels { get; set; }
        public int BusChannels { get; set; }
        public string[] SpecialChannels { get; set; }
        public string MidiManager { get; set; }
        //This defines what processing is available on a channel strip
        //List: ParametricEQ, Dynamics, Gate, FXReverb, FXDelay, FXChorus, ...
        public string[] ChannelStripHardware { get; set; }
        public FXModule[] FXModules { get; set; }

        public MixerProfile()
        {
            SpecialChannels = new string[0];
            MidiManager = string.Empty;
            ChannelStripHardware = new string[0];
            FXModules = new FXModule[1] { new FXModule() };
        }
    }

    [Serializable]
    public class FXModule
    {
        public string FXName { get; set; }
        public FXParameter[] FXParameters { get; set;}

        public FXModule()
        {
            FXName = "DefaultFX";
            FXParameters = new FXParameter[1] { new FXParameter() };
        }
    }

    [Serializable]
    public class FXParameter
    {
        public string ParamName { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string ParamTypeString { get; set; }
        public string[] EnumValues { get; set; }

        public enum ParamType
        {
            Decimal,
            Integer,
            Enum,
            DecimalWithEnumMinMax
        }
    }
}
