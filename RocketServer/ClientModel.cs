using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PacketData;
namespace RocketServer
{
    class ClientModel
    {
        public TcpClient ClientSocket;
        public Thread ClientThread;
        public string id;
        public string ClientIp;
        public delegate void ClientStatusChangedHandler(Object Sender, ClientStatusChangedArgs StatusChanged);
        public delegate void ClientMessageRecievedHandler(Object Sendr, ClientPacketRecievedArgs  MessageRecieved);
        public static ClientMessageRecievedHandler MessageRecieved;
        public static ClientStatusChangedHandler ClientStatusChanged;
        public ClientModel(TcpClient InClient)
        {
            ClientSocket = InClient;
            ClientSocket.Client.ReceiveBufferSize = 20971520;
            ClientSocket.Client.SendBufferSize = 20971520;

            id = Guid.NewGuid().ToString();
            ClientIp = InClient.Client.AddressFamily.ToString();

            ClientStatusChanged(this, new ClientStatusChangedArgs(this,ClientStatus.在线));
            ClientThread = new Thread(Data_IN);
            ClientThread.Start();
        }
        public void Data_IN(object cSocket)
        {
            List<byte> BufferList = new List<byte>();

            int bufferSize = 4096;
            while (true)
            {
                try
                {
                    if (ClientSocket.Available > 0)
                    {
                        NetworkStream clientStream = ClientSocket.GetStream();
                        
                        byte[] buffer = new byte[4];
                        clientStream.Read(buffer, 0, 4);//后声明
                        int PacketSize = BitConverter.ToInt32(buffer, 0);
                        do
                        {
                            System.Threading.Thread.Sleep(20);
                        } while (ClientSocket.Available < PacketSize );
                        
                               byte[] PacketBuffer = new byte[PacketSize];//后声明
                                clientStream.Read(PacketBuffer, 0, PacketSize);
                                Packet p = new Packet(PacketBuffer);
                                MessageRecieved(this, new ClientPacketRecievedArgs(p, this.id));
                     //   clientStream.Close();
                        // DataManager(p);
                    }


                }
                catch (SocketException ex)
                {
                    ClientStatusChanged(this, new ClientStatusChangedArgs(this, ClientStatus.离线));
                }
            }
        }



        public class ClientPacketRecievedArgs : EventArgs
        {
            public ClientPacketRecievedArgs(Packet InPacket, string InMessageContent)
            {
                this.thePacket = InPacket;
                this.MessageContent = InMessageContent;
            }

            public Packet thePacket;
            public string MessageContent;
        }

        public class ClientStatusChangedArgs : EventArgs
        {
            public ClientStatusChangedArgs(ClientModel InClient, ClientStatus InClientStatus)
            {
                this.theClient = InClient;
                this._ClientStatus = InClientStatus;
            }
            public ClientModel theClient;
            public ClientStatus _ClientStatus;
        }
        public enum ClientStatus
        {
            在线,
            离线
        }


    }
}
