using PacketData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RocketClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static TcpClient socket;
        public static IPAddress ipAdress;
        public static string ID;
        public static string login;
        public static Thread thread;
        public static bool isConnected = false;
        SoundPlayer MsgNotifyPlayer = new SoundPlayer();
        //       MediaPlayer mp = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        void Init()
        {
            MsgNotifyPlayer.SoundLocation = Environment.CurrentDirectory + "\\Resources\\DingDong.wav";
            MsgNotifyPlayer.Load();

            //  mp.Open(new Uri(".\\DingDong.wav",UriKind.Relative));
            //  mp.Play();
            this.Topmost = false;

        }
        /// <summary>
        /// on window close event update server for close connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (isConnected)
            {
                Packet p = new Packet(PacketType.CloseConnection, ID, login, "Exit from Chat");

        

                socket.Client.Send(Helper.ForePlay(p));

                socket.Close();
                isConnected = false;
                thread.Abort();
            }
        }


        /// <summary>
        /// connect Button click event
        /// validate login and ip adress
        /// connect if valid and start new thread for receiving data from server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Login.Text))//login validation
            {
                LoginRequireShowMsg();
            }
            else if (!IPAddress.TryParse(serverIP.Text, out ipAdress))//ip validation
            {
                ValidIPRequireShowMsg();
            }
            else//connection to server
            {
                ClearRequireMsg();

                socket = new TcpClient(AddressFamily.InterNetwork);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAdress, 4242);
                socket.ReceiveBufferSize = 20971520;
                socket.SendBufferSize = 20971520;
                try
                {
                    socket.Connect(ipEndPoint);
                    login = Login.Text;

                    isConnected = true;
                    ConnectBtn.IsEnabled = false;
                    SendBtn.IsEnabled = true;
                    btnSendFile.IsEnabled = true;
                    thread = new Thread(Data_IN);
                    thread.IsBackground = true;

                    thread.Start();
                }
                catch (SocketException ex)
                {
                    AddMsgToBoard("Error during connecting to server", "System");
                }
            }
        }


        /// <summary>
        /// send message to server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = Msg.Text;
            Msg.Text = string.Empty;

            Packet p = new Packet(PacketType.Chat, ID, login, msg);

            socket.Client.Send(Helper.ForePlay(p));
        }



        /// <summary>
        /// receive data from socket
        /// then data received call to data manager
        /// </summary>
        private void Data_IN()
        {
            List<byte> BufferList = new List<byte>();

            int bufferSize = 4096;
            while (true)
            {
                if (socket.Available > 0)
                {
                    try
                    {


                        if (socket.Available > 0)
                        {
                            NetworkStream clientStream = socket.GetStream();

                            byte[] buffer = new byte[4];
                            clientStream.Read(buffer, 0, 4);//后声明
                            int PacketSize = BitConverter.ToInt32(buffer, 0);
                            do
                            {
                                System.Threading.Thread.Sleep(20);
                            } while (socket.Available < PacketSize);

                            byte[] PacketBuffer = new byte[PacketSize];//后声明
                            clientStream.Read(PacketBuffer, 0, PacketSize);
                            Packet p = new Packet(PacketBuffer);
                            DataManager(p);
                            //   clientStream.Close();
                            // DataManager(p);
                        }

                    }
                    catch (SocketException ex)
                    {
                        ConnectionToServerLost();
                    }

                }
            }

            //  while (true)
            //  {
            //      try
            //      {
            //          buffer = new byte[socket.SendBufferSize];
            //          readBytes = socket.Receive(buffer);
            //
            //          if (readBytes > 0)
            //          {
            //              Thread.Sleep(2000);
            //              DataManager(new Packet(buffer));
            //          }
            //      }
            //      catch (SocketException ex)
            //      {
            //          ConnectionToServerLost();
            //      }
            //
            //  }
        }


        /// <summary>
        /// manage all received packages by PacketType
        /// </summary>
        /// <param name="p"></param>
        private void DataManager(Packet p)
        {
            switch (p._PacketType)
            {
              //  case PacketType.Registration:
             //       ID = p.SenderName;
                    //  Packet packet = new Packet(PacketType.Chat, ID);
                    //  packet.data.Add(login);
                //    p.data.Add("Enter to Chat");
                    //  socket.Send(packet.ToBytes());
                //    AddMsgToBoard(p.data[1], p.data[0]);

             //       break;
                case PacketType.File:
                    ID = p.SenderName;
                    Packet FilePacket = new Packet(p.ToBytes());
                    if (Directory.Exists(".\\FileRecieved\\"))
                        File.WriteAllBytes(".\\FileRecieved\\" + (new Random().Next(10000, 200000).ToString()) + FilePacket.FileName, FilePacket.FileByte);
                    else
                    {
                        Directory.CreateDirectory(".\\FileRecieved\\");
                        File.WriteAllBytes(".\\FileRecieved\\" + (new Random().Next(10000, 200000).ToString()) + FilePacket.FileName, FilePacket.FileByte);

                    }
                    AddMsgToBoard(FilePacket.FileName, "接收成功");

                    break;
                case PacketType.NextPacketInfo:
                case PacketType.Chat:
                case PacketType.CloseConnection:
                    AddMsgToBoard(p.Message, p.SenderName);
                    break;
            }
        }


        /// <summary>
        /// Server connection lost
        /// add message to user, close socket and thread
        /// enable to user try to connect to server
        /// </summary>
        private void ConnectionToServerLost()
        {
            AddMsgToBoard("Server disconnected", "Server");

            this.Dispatcher.Invoke(new Action(() =>
            {
                ConnectBtn.IsEnabled = true;
                SendBtn.IsEnabled = false;
            }));

            socket.Close();
            thread.Abort();
        }

        /// <summary>
        /// add new message to chat board
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        private void AddMsgToBoard(string msg, string user)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                MsgBoard.Text += Environment.NewLine +
                                 DateTime.Now.ToString() +
                                 " " +
                                 user +
                                 " say: " +
                                 msg;
                MsgBoardScroll.ScrollToEnd();
                MsgBoard.UpdateLayout();
                if (!Application.Current.MainWindow.IsActive)
                {
                    FlashWindowHelper helper = new FlashWindowHelper();
                    helper.Flash(10, 300, new WindowInteropHelper(this).Handle);


                    MsgNotifyPlayer.Play();
                    // SystemSounds.Asterisk.Play();
                }

            }));

        }


        /// <summary>
        /// show validation message for login
        /// </summary>
        private void LoginRequireShowMsg()
        {
            IPRequire.Visibility = System.Windows.Visibility.Hidden;
            ValidIPRequire.Visibility = System.Windows.Visibility.Hidden;

            LoginRequire.Visibility = System.Windows.Visibility.Visible;
        }
        /// <summary>
        /// show validation message for IP
        /// </summary>
        private void ValidIPRequireShowMsg()
        {
            LoginRequire.Visibility = System.Windows.Visibility.Hidden;
            IPRequire.Visibility = System.Windows.Visibility.Hidden;

            ValidIPRequire.Visibility = System.Windows.Visibility.Visible;

        }
        /// <summary>
        /// hidde all validation messages
        /// </summary>
        private void ClearRequireMsg()
        {
            LoginRequire.Visibility = System.Windows.Visibility.Hidden;
            IPRequire.Visibility = System.Windows.Visibility.Hidden;
            ValidIPRequire.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Msg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SendBtn.IsEnabled)
                    SendBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                // MessageBox.Show("XX");
            }
        }

        private void cbxAlwaysFront_Click(object sender, RoutedEventArgs e)
        {
            if (cbxAlwaysFront.IsChecked == true)
            {
                this.Topmost = true;
            }
            else
            {
                this.Topmost = false;
            }
        }

        private void btnSendFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Script Files (*.*)|*.*"
            };
            // var result = ;
            if (openFileDialog.ShowDialog() == true)
            {

                Packet p = new Packet(PacketType.File, ID,login,"", openFileDialog.FileName);
                p.SenderName = login;
                p.Message="发送文件" + new FileInfo(openFileDialog.FileName).Name + " 大小：" + p.FileByte.Length;


                socket.Client.Send(Helper.ForePlay(p));

            }
        }
    }
}
