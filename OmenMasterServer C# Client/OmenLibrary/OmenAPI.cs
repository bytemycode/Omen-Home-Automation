using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Omen
{
    public static class OmenAPI
    {
        private static readonly int     SSDP_PORT       = 1900;
        private static readonly string  SSDP_GROUP      = "239.255.255.250";
        private static readonly string  SSDP_GROUPIPV6  = "[FF0E::C]";

        private static readonly string  SSDP_ST         = "upnp:omen-masterserver";
        private static readonly int     SSDP_MX         = 0;
        private static readonly string  SSDP_REQUEST    = "M-SEARCH * HTTP/1.1\r\n" +
                                                          "HOST: {0}:{1}\r\n" +
                                                          "MAN: \"ssdp:discover\"\r\n" +
                                                          "ST: {2}\r\n" +
                                                          "MX: {3}\r\n";

        private static IPAddress GetLocalIPAddress()
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                if (adapter.OperationalStatus == OperationalStatus.Up && adapter.SupportsMulticast)
                {
                    foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
                    {
                        if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetworkV6 &&
                            unicastAddress.Address != IPAddress.IPv6Loopback)
                        {
                            // IPV6 still has some issues
                            //return unicastAddress.Address;

                        }
                        if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork &&
                            unicastAddress.Address != IPAddress.Loopback)
                        {
                            return unicastAddress.Address;
                        }
                    }
                }
            }

            return null;
        }

        public static async Task<string> FindMasterServer()
        {
            IPAddress local = GetLocalIPAddress();
            if(local == null)
            {
                return null;
            }
            IPEndPoint endPoint = new IPEndPoint(local, 0);

            IPAddress group = null;
            if(local.AddressFamily == AddressFamily.InterNetworkV6)
            {
                group = IPAddress.Parse(SSDP_GROUPIPV6);
            }
            else
            {
                group = IPAddress.Parse(SSDP_GROUP);
            }

            // Create a local socket bound to our multicast interface
            Socket socket = new Socket(local.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(endPoint);

            // Open an UDP client and join an udp group
            UdpClient client = new UdpClient() { Client = socket };
            client.JoinMulticastGroup(group);

            // Open an multicast endpoint to be able to send data
            IPEndPoint endpoint = new IPEndPoint(group, SSDP_PORT);

            // Prepare the request string
            string formatedReq = string.Format(SSDP_REQUEST, SSDP_GROUP, SSDP_PORT, SSDP_ST, SSDP_MX);

            // Send the request to the group
            await client.SendAsync(Encoding.ASCII.GetBytes(formatedReq), formatedReq.Length, endpoint);

            // Receive the response
            Task<UdpReceiveResult> receiveTask;
            string resultStr = null;

            try
            {
                receiveTask = client.ReceiveAsync();
            }
            catch(SocketException)
            {
                client.DropMulticastGroup(group);
                client.Close();
                return null;
            }

            await Task.WhenAny(Task.Delay(SSDP_MX + 100), receiveTask);

            if(receiveTask.IsCompleted)
            {
                resultStr = Encoding.Default.GetString(receiveTask.Result.Buffer);
            }

            // Close client
            client.DropMulticastGroup(group);
            client.Close();

            Regex rx = new Regex(@"^LOCATION:\s*(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})", RegexOptions.Compiled | RegexOptions.Multiline);
            MatchCollection matches = rx.Matches(resultStr);

            foreach (Match match in matches)
            {
                return match.Groups["ip"].Value;
            }

            return null;
        }
    }
}
