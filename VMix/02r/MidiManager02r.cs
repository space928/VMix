using System;
using System.Collections.Generic;
using System.Linq;

namespace VMix
{
    public class MidiManager02r : MidiManager
    {
        private static readonly byte[] _02rMsgStart = new byte[] { 0xF0, 0x43, 0x10, 0x3D };

        public MidiManager02r(Mixer mixer) : base(mixer)
        {
            base.vMixer = mixer;
        }

        #region MidiDecoder
        public override bool FilterMidiMsg(byte[] msg)
        {
            return true;
        }

        public override void DecodeMsg(byte[] msg)
        {
            //Simple sanity checks to ensure this is a message destined for us
            if (msg.Length < 7)
                return;//Don't log this it probably wasn't meant for us anyway, no need to pollute logs
            if (!Enumerable.SequenceEqual(msg.Take(_02rMsgStart.Length).ToArray(), _02rMsgStart))
                return;//Don't log this it probably wasn't meant for us anyway, no need to pollute logs

            switch (msg[4])
            {
                case 0x00://Numeric data
                    // ===== FADER
                    if (msg[5] == 0x0 && msg[6] < 0x57)
                    {
                        DecodeFaderMsg(msg.Skip(6).ToArray());
                    }// ===== SEND
                    else if (msg[5] == 0x0 && msg[6] >= 0x57)
                    {
                        DecodeSendMsg(msg.Skip(6).ToArray());
                    }// ===== PAN
                    else if (msg[5] == 0x04)
                    {
                        DecodePanMsg(msg.Skip(6).ToArray());
                    }// ===== EQ
                    else if (msg[5] == 0x0A)
                    {
                        DecodeEqMsg(msg.Skip(6).ToArray());
                    }
                    else
                    {
                        Console.WriteLine("Warning illegal message received from device: \n" + msg);
                        return;
                    }
                    break;
                case 0x40://Bool data
                    // ===== CHANNEL ON
                    if (msg[5] == 0x03 && msg[6] <= 0x0D)
                    {
                        DecodeOnMsg(msg.Skip(6).ToArray());
                    }// ===== SENDS/ROUTING????
                    else if (msg[5] == 0x03 && msg[6] > 0x0D)
                    {

                    }// ===== ROUTE ST/Direct
                    else if (msg[5] == 0x04)
                    {

                    }// ===== EQ ON
                    else if (msg[5] == 0x06)
                    {

                    }
                    break;
                default:
                    Console.WriteLine("Warning illegal message received from device: \n" + msg);
                    return;
            }
        }

        /// <summary>
        /// This method returns the channel number assuming the message starts with the channel number and has enough bytes to contain a channel number
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private int ParseChannelNumber(byte[] msg, int startIndex = 0)
        {
            int channelNo = -1;

            if (msg[startIndex] <= 0x43)
                channelNo = msg[startIndex] - 0x20;
            //In this case, channelNo will be 1-16 for MIC 1-16 17-32 for TAPE 1-16 and 33-40 for LINE 17-24
            if (msg[startIndex] >= 0x44)
                //FX return channels then ST
                channelNo = msg[startIndex] - 0x20;

            return channelNo;
        }

        private void DecodeFaderMsg(byte[] msg)
        {
            int channelNo = ParseChannelNumber(msg);

            //Returns a float between 0-1 for the fader value
            double faderVal = (msg[1] * 0xf + msg[2]) / 120.0;
            //Remap faderVal between min and max
            //faderVal = faderVal * (99 + 10) - 99;

            vMixer.MixChannels[channelNo].SetPostDCAFaderLevel(vMixer.DCAs.ToArray(), faderVal);

            //Check if multiple adjacent faders are being sent
            if (msg.Length > 4)
            {
                int cbyte = 3;
                while (msg[cbyte] != 0xf7 && cbyte < 100)//Sanity check in case the message has no end
                {
                    channelNo++;

                    faderVal = (msg[cbyte] * 0xf + msg[cbyte + 1]) / 120.0;

                    vMixer.MixChannels[channelNo].SetPostDCAFaderLevel(vMixer.DCAs.ToArray(), faderVal);

                    cbyte += 2;
                }
            }
        }

        private void DecodeSendMsg(byte[] msg)
        {
            Console.WriteLine("Received unknown send midi message: " + msg);
            LogMsg(msg);
        }

        private void DecodePanMsg(byte[] msg)
        {
            Console.WriteLine("Received unknown pan midi message: " + msg);
            LogMsg(msg);
        }

        private void DecodeEqMsg(byte[] msg)
        {
            Console.WriteLine("Received unknown eq midi message: " + msg);
            LogMsg(msg);
        }

        private void DecodeDynamicsMsg(byte[] msg)
        {
            Console.WriteLine("Received unknown dyn midi message: " + msg);
            LogMsg(msg);
        }

        private void DecodeOnMsg(byte[] msg)
        {
            Console.WriteLine("Received unknown on midi message: ");
            LogMsg(msg);

            //The channel number is made up of part msg[0] for which block of 8 it falls under and the first 3 bits of msg[1]
            int channelBank = -1;
            switch (msg[0])
            {
                case 0x09://MIC 1-8
                    channelBank = 0;
                    break;
                case 0x08://MIC 9-16
                    channelBank = 8;
                    break;
                case 0x0b://TAPE 1-8
                    channelBank = 16;
                    break;
                case 0x0a://TAPE 9-16
                    channelBank = 24;
                    break;
                case 0x0d://LINE 17-24 / EFF1-2 / ST
                    channelBank = 32;
                    break;
                default:
                    return;
            }
            int channelNo = channelBank + (msg[1] & 0b111);

            //Whether or not it is on is determined by bit 4 of msg[1]
            bool on = (msg[1] & 0b00001000) != 0;

            if (channelNo == 38)
                vMixer.STOut.On = on;
            else
                vMixer.MixChannels[channelNo].On = on;

            if (msg.Length > 3)
            {
                int cbyte = 2;
                while (msg[cbyte] != 0xf7 && cbyte < 100)//Sanity check in case the message has no end
                {
                    channelNo = channelBank + (msg[cbyte] & 0b111);

                    //Whether or not it is on is determined by bit 4 of msg[1]
                    on = (msg[cbyte] & 0b00001000) != 0;

                    vMixer.MixChannels[channelNo].On = on;

                    cbyte++;
                }
            }
        }

        private void LogMsg(byte[] msg)
        {
            foreach (byte b in msg)
                Console.Write(Convert.ToString(b, 16) + " ");
            Console.WriteLine();
        }
        #endregion

        #region MidiEncoder
        public override void PostProcessMsg(ref List<byte[]> msgs)
        {
            Queue<byte[]> faderMsgs = new Queue<byte[]>(msgs.Where(msg => msg[4] == 0 && msg[5] == 0));
            //Remove all the fader messages
            msgs = msgs.Except(faderMsgs).ToList();

            faderMsgs.OrderBy(x => x[6]);
            while (faderMsgs.Count > 0)
            {
                List<byte> newMsg = new List<byte>(faderMsgs.Dequeue());
                //Remove stop byte
                newMsg.RemoveAt(newMsg.Count - 1);
                int lastIndex = ParseChannelNumber(newMsg.ToArray(), 6);

                bool contiguous = true;
                while (contiguous && faderMsgs.Count > 0)
                {
                    byte[] nextMsg = faderMsgs.Dequeue();
                    if (ParseChannelNumber(nextMsg, 6) == lastIndex + 1)
                    {
                        lastIndex++;
                        newMsg.Add(nextMsg[7]);
                        newMsg.Add(nextMsg[8]);
                    }
                    else
                    {
                        contiguous = false;
                    }
                }
                //Stop byte
                newMsg.Add(0xf7);
                msgs.Add(newMsg.ToArray());
            }
        }

        public override void SendFaderMsg(Channel m)
        {
            //Begin msg
            List<byte> msg = new List<byte>();
            msg.AddRange(_02rMsgStart);

            msg.Add(0x00);//Numeric data
            msg.Add(0x00);//Fader

            msg.Add(Convert.ToByte(m.ChannelIndex + 0x20));//Channel index

            double faderVal = Math.Round(m.GetPostDCAFaderLevel01(vMixer.DCAs.ToArray()) * 127);
            double msb = Math.Floor(faderVal / 16.0);
            msg.Add(Convert.ToByte((int)msb));//MSB
            msg.Add(Convert.ToByte((int)(faderVal - msb * 16)));//LSB

            msg.Add(0xf7);//End msg
            SendRawMidiMessage(msg.ToArray(), m.GetHashCode());
        }

        public override void SendPanMsg(MixChannel m)
        {
            throw new NotImplementedException();
        }

        public override void SendAuxMsg(MixChannel m)
        {
            throw new NotImplementedException();
        }

        public override void SendOnMsg(Channel m)
        {
            //Begin msg
            List<byte> msg = new List<byte>();
            msg.AddRange(_02rMsgStart);

            msg.Add(0x40);//Bool data
            msg.Add(0x03);//On

            //See decoder for implementation details
            if (m.ChannelIndex < 8)
                msg.Add(0x09);
            else if (m.ChannelIndex < 16)
                msg.Add(0x08);
            else if (m.ChannelIndex < 24)
                msg.Add(0x0b);
            else if (m.ChannelIndex < 32)
                msg.Add(0x0a);
            else if (m.ChannelIndex < 40)
                msg.Add(0x0d);

            msg.Add(Convert.ToByte((m.ChannelIndex & 0b111) + (m.On ? 8 : 0)));

            msg.Add(0xf7);//End msg
            SendRawMidiMessage(msg.ToArray(), m.GetHashCode());
        }
        #endregion
    }
}
