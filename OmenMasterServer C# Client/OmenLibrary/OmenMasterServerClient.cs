using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Omen
{

    #region DataContracts

    public enum FocusAction : uint
    {
        ActivateVideo   = 0x01,
        DeactivateVideo = 0x02,
        ForceFocus      = 0x03
    }

    [DataContract]
    internal abstract class IOmenCommand
    {
        [DataMember]
        private readonly string Mnemonic;

        public IOmenCommand(string mnemonic)
        {
            Mnemonic = mnemonic;
        }
    }

    [DataContract]
    internal class VolumeCommand : IOmenCommand
    {
        [DataMember]
        public float Delta;

        public VolumeCommand() : base("vol")
        {

        }
    }

    [DataContract]
    internal class GetVolumeCommand : IOmenCommand
    {
        public GetVolumeCommand() : base("get_vol")
        {

        }
    }

    [DataContract]
    internal class VolumeCommandResponse
    {
        [DataMember]
        public string Type { private set; get; }

        [DataMember]
        public float CurrentVolume { private set; get; }
    }

    [DataContract]
    internal class FocusCommand : IOmenCommand
    {
        [DataMember]
        public FocusAction Action;

        [DataMember]
        public MasterServerChannel NewChannel = MasterServerChannel.None;

        public FocusCommand() : base("focus")
        {

        }

        public FocusCommand(FocusAction action, MasterServerChannel newChannel = MasterServerChannel.None) 
            : this()
        {
            Action = action;
            NewChannel = newChannel;
        }
    }

    #endregion

    public class OmenMasterServerClient : OmenConnection
    {
        public void ActivateVideo()
        {
            FocusCommand command = new FocusCommand()
            {
                Action = FocusAction.ActivateVideo
            };

            SendCommand(command);
        }

        public void DeactivateVideo()
        {
            FocusCommand command = new FocusCommand()
            {
                Action = FocusAction.DeactivateVideo
            };

            SendCommand(command);
        }

        public void ForceFocus(MasterServerChannel newChannel = MasterServerChannel.None)
        {
            FocusCommand command = new FocusCommand()
            {
                Action = FocusAction.ForceFocus
            };

            if(newChannel != MasterServerChannel.None)
            {
                command.NewChannel = newChannel;
            }

            SendCommand(command);
        }

        public async Task<float> GetCurrentVolume()
        {
            GetVolumeCommand command = new GetVolumeCommand();
            byte[] rez = await SendCommandWithResponse(command);

            VolumeCommandResponse response = GetObjectBytesFromJSON<VolumeCommandResponse>(rez);

            return response.CurrentVolume;
        }

        public void SetVolume(float delta)
        {
            if(delta != 0.0f)
            {
                VolumeCommand command = new VolumeCommand()
                {
                    Delta = delta
                };

                SendCommand(command);
            }
        }

        private async void SendCommand<T>(T command) where T : IOmenCommand
        {
            await Send(GetJSONBytes(command));
        }

        private async Task<byte[]> SendCommandWithResponse<T>(T command) where T : IOmenCommand
        {
            SendCommand(command);
            return await Receive();
        }

    }
}
