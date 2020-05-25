using Commons.Music.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace VMix
{
    public abstract class MidiManager
    {
        private IMidiOutput midiOutput;
        private IMidiInput midiInput;
        private Dictionary<int, byte[]> midiMsgQueue = new Dictionary<int, byte[]>();
        private float msgRate = 100;
        //private Dictionary<int,Task> midiMsgTasks = new Dictionary<int, Task>();
        private Task midiMsgTask = Task.CompletedTask;

        internal Mixer vMixer;

        public MidiManager(Mixer mixer)
        {
            vMixer = mixer;
        }

        /// <summary>
        /// Opens a new midi device and *should* automatically close the previous connection
        /// </summary>
        /// <param name="deviceInput">Midi input device ID</param>
        /// <param name="deviceOutput">Midi output device ID</param>
        public void OpenMidiDevice(IMidiPortDetails deviceInput, IMidiPortDetails deviceOutput)
        {
            if (midiOutput != null)
                midiOutput.CloseAsync();//Will this wait for the port to be closed or does it really matter?
            if (midiInput != null)
            {
                midiInput.MessageReceived -= MidiInput_MessageReceived;
                midiInput.CloseAsync();//Will this wait for the port to be closed or does it really matter?
            }

            var access = MidiAccessManager.Default;

            try
            {
                midiOutput = access.OpenOutputAsync(deviceOutput.Id).Result;
                midiInput = access.OpenInputAsync(deviceInput.Id).Result;

                midiInput.MessageReceived += MidiInput_MessageReceived;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Opens a new midi device from a device name and *should* automatically close the previous connection
        /// </summary>
        /// <param name="deviceInput">Midi input device name</param>
        /// <param name="deviceOutput">Midi output device name</param>
        public void OpenMidiDevice(string deviceInput, string deviceOutput)
        {
            if (midiOutput != null)
                midiOutput.CloseAsync();//Will this wait for the port to be closed or does it really matter?
            if (midiInput != null)
            {
                midiInput.MessageReceived -= MidiInput_MessageReceived;
                midiInput.CloseAsync();//Will this wait for the port to be closed or does it really matter?
            }

            var access = MidiAccessManager.Default;

            try
            {
                midiOutput = access.OpenOutputAsync(deviceOutput).Result;
                midiInput = access.OpenInputAsync(deviceInput).Result;

                midiInput.MessageReceived += MidiInput_MessageReceived;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public IMidiPortDetails[] GetMidiOutputDevices()
        {
            var access = MidiAccessManager.Default;
            return access.Outputs.ToArray();
        }

        public IMidiPortDetails[] GetMidiInputDevices()
        {
            var access = MidiAccessManager.Default;
            return access.Inputs.ToArray();
        }

        public IMidiPortDetails GetConnectedInput()
        {
            return midiInput?.Details ?? null;
        }

        public IMidiPortDetails GetConnectedOutput()
        {
            return midiOutput?.Details ?? null;
        }

        private void MidiInput_MessageReceived(object sender, MidiReceivedEventArgs e)
        {
            if (FilterMidiMsg(e.Data))
            {
                vMixer.remoteSet = true;
                DecodeMsg(e.Data);
                //Just in case the message cannot be decoded, reset the flag
                vMixer.remoteSet = false;
            }
        }

        public void SendRawMidiMessage(byte[] bytes, int hashCode)
        {
            midiMsgQueue.Remove(hashCode);
            midiMsgQueue.Add(hashCode, bytes);

            if (midiMsgTask.IsCompleted)
            {
                midiMsgTask = Task.Delay((int)(1000f / msgRate));
                midiMsgTask.ContinueWith(x => SendMessageQueue());
            }

            /*if(!midiMsgTasks.ContainsKey(hashCode))
                midiMsgTasks.Add(hashCode, Task.CompletedTask);
            if (midiMsgTasks[hashCode].IsCompleted)
            {
                midiMsgTasks[hashCode] = Task.Delay((int)(1000f / msgRate));
                midiMsgTasks[hashCode].ContinueWith(x => SendMidiBytes(midiMsgQueue[hashCode]));
            }*/
        }

        private void SendMessageQueue()
        {
            List<byte[]> msgs = midiMsgQueue.Values.ToList();

            PostProcessMsg(ref msgs);

            foreach (byte[] msg in msgs)
                SendMidiBytes(msg);

            midiMsgQueue.Clear();
        }

        private void SendMidiBytes(byte[] bytes)
        {
            try
            {
                if (midiOutput?.Connection == MidiPortConnectionState.Open)
                    midiOutput.Send(bytes, 0, bytes.Length, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void HandleSendFaderMsg(object sender, PropertyChangedEventArgs e, Channel m)
        {
            if (!vMixer.remoteSet)
            {
                vMixer.remoteSet = false;
                SendFaderMsg(m);
            }
        }

        public void HandleDCAFaderUpdate(object sender, PropertyChangedEventArgs e, DCA dca)
        {
            if (!vMixer.remoteSet)
            {
                vMixer.remoteSet = false;
                foreach (Channel c in dca.AssignedChannels)
                    SendFaderMsg(c);
            }
        }

        public void HandleSendPanMsg(object sender, PropertyChangedEventArgs e, MixChannel m)
        {
            if (!vMixer.remoteSet)
            {
                vMixer.remoteSet = false;
                SendPanMsg(m);
            }
        }

        public void HandleSendAuxMsg(object sender, PropertyChangedEventArgs e, MixChannel m)
        {
            if (!vMixer.remoteSet)
            {
                vMixer.remoteSet = false;
                SendAuxMsg(m);
            }
        }

        public void HandleSendOnMsg(object sender, PropertyChangedEventArgs e, Channel m)
        {
            if (!vMixer.remoteSet)
            {
                vMixer.remoteSet = false;
                SendOnMsg(m);
            }
        }

        /// <summary>
        /// Use to filter out unwanted midi messages. Return true to decode incomming message.
        /// </summary>
        /// <param name="msg">Midi message</param>
        /// <returns></returns>
        public abstract bool FilterMidiMsg(byte[] msg);

        /// <summary>
        /// Decode an incomming message.
        /// </summary>
        /// <param name="msg"></param>
        public abstract void DecodeMsg(byte[] msg);

        public abstract void PostProcessMsg(ref List<byte[]> msgs);

        public abstract void SendFaderMsg(Channel m);

        public abstract void SendPanMsg(MixChannel m);

        public abstract void SendAuxMsg(MixChannel m);

        public abstract void SendOnMsg(Channel m);

        //public abstract void SendRoutingMsg(object sender, PropertyChangedEventArgs e);
    }
}
