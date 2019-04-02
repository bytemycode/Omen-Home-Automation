using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceProcess;

namespace Omen
{
    partial class OmenDaemon : ServiceBase
    {
        const int SERVICE_ACCEPT_PRESHUTDOWN = 0x100;
        const int SERVICE_CONTROL_PRESHUTDOWN = 0xf;

        private readonly OmenSettings Settings = new OmenSettings()
        {
            Name = "PC",
            Channel = MasterServerChannel.BluRay,
            WantsVideo = false
        };
        private bool HasVideo = false;

        private string MasterServerAddress = null;
        private OmenMasterServerClient Client = null;
        private bool IsPendingStartup = false;
        
        public OmenDaemon()
        {
            InitializeComponent();

            HasVideo = Settings.WantsVideo;
        }

        ~OmenDaemon()
        {
            Disconnect();
        }

        #region API

        public void Connect()
        {
            if (MasterServerAddress == null)
            {
                MasterServerAddress = OmenAPI.FindMasterServer().GetAwaiter().GetResult();
                EventLog.WriteEntry("Got MasterServer " + MasterServerAddress);
            }

            if (MasterServerAddress != null && Client == null)
            {
                try
                {
                    Client = new OmenMasterServerClient();
                    Client.Connect("ws://" + MasterServerAddress + ":666/", Settings).GetAwaiter().GetResult();
                    EventLog.WriteEntry("Connected", EventLogEntryType.Warning);
                    IsPendingStartup = false;
                }
                catch (Exception)
                {
                    Client = null;
                    IsPendingStartup = true;
                }
            }
        }

        public void Disconnect()
        {
            IsPendingStartup = false;

            if (Client != null)
            {
                try
                {
                    Client.Disconnect().GetAwaiter().GetResult();
                }
                catch
                {

                }
                finally
                {
                    Client = null;
                }
               EventLog.WriteEntry("Disconnected", EventLogEntryType.Warning);
            }
        }

        public bool ToggleProjector()
        {
            if(HasVideo)
            {
                Client.DeactivateVideo();
            }
            else
            {
                Client.ActivateVideo();
            }

            HasVideo = !HasVideo;
            return HasVideo;
        }

        #endregion

        #region Overrides

        protected override void OnStart(string[] args)
        {
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(NetworkAddressChanged);
            EventLog.WriteEntry("Start");
            Connect();
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("Stop");
            Disconnect();
        }

        protected override void OnShutdown()
        {
            EventLog.WriteEntry("Shutdown");
            Disconnect();
            base.OnShutdown();
        }

        protected override void OnPause()
        {
            EventLog.WriteEntry("Pause");
            Disconnect();
        }

        protected override void OnContinue()
        {
            EventLog.WriteEntry("Continue");
            Connect();
            base.OnContinue();
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            if (powerStatus.HasFlag(PowerBroadcastStatus.ResumeSuspend))
            {
                EventLog.WriteEntry("ResumeSuspend");
                Connect();

                return true;
            }
            else if(powerStatus.HasFlag(PowerBroadcastStatus.Suspend))
            {
                EventLog.WriteEntry("Suspend");

                Disconnect();

                return true;
            }
            
            return base.OnPowerEvent(powerStatus);
        }

        #endregion

        #region Callbacks

        private void NetworkAddressChanged(object sender, EventArgs e)
        {
            if (IsPendingStartup)
            {
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in adapters)
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    if (adapter.OperationalStatus == OperationalStatus.Up && adapter.SupportsMulticast)
                    {
                        foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
                        {
                            if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork &&
                                unicastAddress.Address != IPAddress.Loopback)
                            {
                                EventLog.WriteEntry("NetWorkChange Connecting");

                                Connect();
                                return;
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
