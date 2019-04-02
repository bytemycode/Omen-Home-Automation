using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace Omen
{
    public enum MasterServerChannel : int
    {
        None    =   -1,
        CBLSAT      = 0x01,
        DVD         = 0x02,
        BluRay      = 0x03, // PC
        Game        = 0x04, // Switch
        MediaPlayer = 0x05, // MiBox
        AUX         = 0x06,
    }

    [DataContract]
    public class OmenSettings
    {
        [DataMember]
        public string Name = "Unknown";

        [DataMember]
        public MasterServerChannel Channel = MasterServerChannel.None;

        [DataMember]
        public bool WantsAudio { get; private set; } = true; // Clients will always want audio

        [DataMember]
        public bool WantsVideo = false;
    }

    public abstract class OmenConnection
    {
        private ClientWebSocket WebSocket = null;

        protected byte[] GetJSONBytes<T>(T obj)
        {
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
            MemoryStream msObj = new MemoryStream();
            js.WriteObject(msObj, obj);
            msObj.Position = 0;
            StreamReader sr = new StreamReader(msObj);

            string json = sr.ReadToEnd();

            sr.Close();
            msObj.Close();

            return Encoding.ASCII.GetBytes(json);
        }

        protected T GetObjectBytesFromJSON<T>(byte[] json)
        {
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
            MemoryStream msObj = new MemoryStream(json);
            js.ReadObject(msObj);

            msObj.Close();

            return (T)js.ReadObject(msObj);
        }

        protected async Task<bool> Send(byte[] rawData)
        {
            ArraySegment<byte> buff = new ArraySegment<byte>(rawData, 0, rawData.Length);
            await WebSocket.SendAsync(buff, WebSocketMessageType.Text, true, CancellationToken.None);

            return true;
        }

        protected async Task<byte[]> Receive()
        {
            ArraySegment<byte> buff = new ArraySegment<byte>();

            WebSocketReceiveResult res = await WebSocket.ReceiveAsync(buff, CancellationToken.None);
            return buff.Array;
        }

        public async Task<bool> Connect(string url, OmenSettings settings)
        {
            if(WebSocket != null) // Already connected
            {
                return false;
            }

            WebSocket = new ClientWebSocket();

            await WebSocket.ConnectAsync(new Uri(url), CancellationToken.None);

            if(WebSocket.State == WebSocketState.Open)
            {
                // Encode our message
                await Send(GetJSONBytes(settings));

                return true;
            }
            else
            {
                WebSocket.Dispose();
                WebSocket = null;

                return false;
            }

        }


        public async Task<bool> Disconnect()
        {
            if (WebSocket == null) // Already disconnected
            {
                return false;
            }


            await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            WebSocket.Dispose();
            WebSocket = null;

            return true;
        }
    }
}
