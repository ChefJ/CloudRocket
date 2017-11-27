using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PacketData;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RocketServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<ClientModel> ClientList = new List<ClientModel>();
        static TcpListener listenerSocket;
        Thread listenThread ;

        public MainWindow()
        {
            InitializeComponent();

            tbxConsole.Text += "Starting Server...\r\n";
            listenerSocket = new TcpListener(new IPEndPoint(IPAddress.Any, 4242));
            listenThread = new Thread(ListenThread);
            listenThread.IsBackground = true;
            //   IPEndPoint ip = ;
            //     listenerSocket.Bind(ip);
            //     listenerSocket.b = 20971520;
            //     listenerSocket.SendBufferSize = 20971520;
            //

            listenThread.Start();
            tbxConsole.Text+=("Success... Listening IP: " + GetIP4Adress() + ":4242\r\n");

            ClientModel.MessageRecieved += Client_MessageRecieved ;
            ClientModel.ClientStatusChanged += Client_StatusChanged;
        }

        private void Client_StatusChanged(object Sender, ClientModel.ClientStatusChangedArgs StatusChanged)
        {
            ClientModel ChangedModel = Sender as ClientModel;
            switch (StatusChanged._ClientStatus)
            {
                case ClientModel.ClientStatus.在线:
                    SendMessageToCLients(new Packet(PacketType.Chat, "", ChangedModel.id+ "Server", "上线了。"));

                    break;
                case ClientModel.ClientStatus.离线:
                    SendMessageToCLients(new Packet(PacketType.Chat, "", "Server", "下线了。"));

                    break;

            }
        }

        private void Client_MessageRecieved(object Sendr, ClientModel.ClientPacketRecievedArgs MessageRecieved)
        {
            App.Current.Dispatcher.Invoke(() => {
        //        tbxConsole.Text += "接收到：" + MessageRecieved.thePacket.SenderName + "发送的" + MessageRecieved.thePacket.Message + "\r\n\r\n";

            });
            switch (MessageRecieved.thePacket._PacketType)
            {
                case PacketType.Chat:
                    SendMessageToCLients(MessageRecieved.thePacket);

                    break;
                case PacketType.File:
                    SendFlieToCLients(MessageRecieved);

                    break;
                case PacketType.CloseConnection:
                    SendMessageToCLients(new Packet(PacketType.Chat,"","Server","下线了。" ));

                    RemoveClient(Sendr as ClientModel);
                    break;
            }
        }

        private void SendMessageToCLients(Packet thePacket)
        {
            for(int Index = 0; Index < ClientList.Count; Index++)
            {
                try
                {
                    ClientList[Index].ClientSocket.Client.Send(Helper.ForePlay(thePacket));
                }
                catch (Exception exx)
                {
                    Console.WriteLine(ClientList[Index].ClientIp + "Client Disconnected.");

                    RemoveClient(ClientList[Index]);
                    continue;
                }

            }
            
        }

        private void SendFlieToCLients(ClientModel.ClientPacketRecievedArgs messageRecieved)
        {
            for (int Index = 0; Index < ClientList.Count; Index++)
            {
                try
                {
                    ClientList[Index].ClientSocket.Client.Send(Helper.ForePlay(messageRecieved.thePacket));

                }
                catch (Exception exx)
                {
                    Console.WriteLine(ClientList[Index].ClientIp + "Client Disconnected.");

                    RemoveClient(ClientList[Index]);
                    continue;
                }

            }
        }

        void ListenThread()
        {
            listenerSocket.Start(0);

            try
            {
                while (true)
                {

                    ClientList.Add(new ClientModel(listenerSocket.AcceptTcpClient()));

                }
            }
            catch (Exception exx)
            {
                MessageBox.Show(exx.Message);
            }

        }

        void RemoveClient(ClientModel p)
        {
            p.ClientSocket.Close();
            ClientList.Remove(p);

            p.ClientThread.Abort();

        }

        public static string GetIP4Adress()
        {
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());

            return (from ip in ips
                    where ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                    select ip)
                   .FirstOrDefault()
                   .ToString();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SendMessageToCLients(new Packet(PacketType.Chat, "", "Server", "已经下线了。"));

          //  listenThread.Abort();
        }
    }
}
