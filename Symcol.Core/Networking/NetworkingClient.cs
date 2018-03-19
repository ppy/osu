using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Symcol.Core.Networking
{
    public class NetworkingClient
    {
        public UdpClient UdpClient;

        public IPEndPoint EndPoint;

        /// <summary>
        /// if false we only receive
        /// </summary>
        public readonly bool Send;

        public readonly int Port;

        public readonly string IP;

        public NetworkingClient(bool send, string ip, int port = 25570)
        {
            Port = port;
            IP = ip;

            if (send)
                initializeSend();
            else
                initializeReceive();
        }

        private void initializeSend()
        {
            UdpClient = new UdpClient(IP, Port);
        }

        private void initializeReceive()
        {
            UdpClient = new UdpClient(Port);
            EndPoint = new IPEndPoint(IPAddress.Any, Port);
        }

        private void sendByte(byte[] data)
        {
            UdpClient.Send(data, data.Length);
        }

        private byte[] receiveByte()
        {
            return UdpClient.Receive(ref EndPoint);
        }

        public static int SENTPACKETCOUNT;

        /// <summary>
        /// Send a Packet somewhere
        /// </summary>
        /// <param name="packet"></param>
        public void SendPacket(Packet packet)
        {
            SENTPACKETCOUNT++;
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, packet);

                stream.Position = 0;

                int i = packet.PacketSize;
                retry:
                byte[] data = new byte[i];

                try
                {
                    stream.Read(data, 0, (int)stream.Length);
                }
                catch
                {
                    i *= 2;
                    goto retry;
                }

                sendByte(data);
            }
        }

        /// <summary>
        /// Receive a Packet from somewhere
        /// </summary>
        /// <returns></returns>
        public Packet ReceivePacket(bool force = false)
        {
            if (UdpClient.Available > 0 || force)
                using (MemoryStream stream = new MemoryStream())
                {
                    byte[] data = receiveByte();
                    stream.Write(data, 0, data.Length);

                    stream.Position = 0;

                    BinaryFormatter formatter = new BinaryFormatter();
                    Packet packet = (Packet)formatter.Deserialize(stream);
                    packet.ClientInfo.IP = EndPoint.Address.ToString();

                    return packet;
                }
            else
                return null;
        }

        public void Clear()
        {
            if (UdpClient != null)
                UdpClient.Dispose();
        }
    }
}
