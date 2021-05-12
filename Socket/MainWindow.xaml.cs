using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
//Aggiunta delle seguenti librerie:
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EsercizioSocket
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Global Variables
        ChatState chatState;
        IPEndPoint ipendpoint;
        IPEndPoint sourceSocket;
        IPAddress ipaddress;
        int port;
        int numeroDado;
        bool connesso;
        #endregion

        public MainWindow() => InitializeComponent();

        #region Connection Methods
        public async void SocketReceive(object socketsource)
        {
            try
            {
                ipendpoint = (IPEndPoint)socketsource;
                Socket t = new Socket(ipendpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                t.Bind(ipendpoint);
                byte[] bytesRicevuti = new byte[256];
                string message;
                int bytes = 0;
                await Task.Run(() =>
                {
                    while (true)
                    {
                        if (t.Available > 0)
                        {
                            message = "";
                            bytes = t.Receive(bytesRicevuti, bytesRicevuti.Length, 0);
                            message = message + Encoding.ASCII.GetString(bytesRicevuti, 0, bytes);
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                switch(message)
                                {
                                    case "MSG":
                                        chatState = ChatState.DiceGame;
                                        LanciaDadi();
                                        break;
                                    case "CNS":
                                        Connetti();
                                        break;
                                    default:
                                        if(int.TryParse(message, out int n) && chatState == ChatState.DiceGame)
                                        {
                                            if (numeroDado > n)
                                                txtChat.Text += "Hai vinto! + \n";
                                            else if (numeroDado < n)
                                                txtChat.Text += "Hai perso! + \n";
                                            else
                                                txtChat.Text += "Avete Pareggiato! + \n";
                                            chatState = ChatState.Chat;
                                        }
                                        else txtChat.Text += $"[{ipendpoint.Address}] " + message + "\n";
                                        break;
                                }
                            }));
                        }
                    }
                });
            }
            catch(Exception exe)
            {
                MessageBox.Show(exe.Message, "Errore");
            }
        }
        public void SocketSend(IPAddress dest, int destPort, string message)
        {
            byte[] byteSent = Encoding.ASCII.GetBytes(message);
            Socket s = new Socket(dest.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint remote_endpoint = new IPEndPoint(dest, destPort);
            s.SendTo(byteSent, remote_endpoint);
            txtChat.Text += $"[Tu] " + message + "\n";
        }
        #endregion

        #region WPF Methods
        private void btnInviaMSG_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SocketSend(ipaddress, port, txtMessage.Text);
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message, "Errore");
            }
        }

        private void btnCreaSocket_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckIfIPandPortAreValid(txtIP.Text, txtPort.Text))
                {
                    port = int.Parse(txtPort.Text);
                    sourceSocket = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), 56000);
                    btnInviaMSG.IsEnabled = true;
                    Thread ricezione = new Thread(new ParameterizedThreadStart(SocketReceive));
                    ricezione.Start(sourceSocket);
                    SocketSend(ipaddress, port, "CNS");
                    chatState = ChatState.Chat;
                }
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message, "Errore");
            }
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Button_Click(object sender, RoutedEventArgs e) => Environment.Exit(1);

        private void btnDice_Click(object sender, RoutedEventArgs e)
        {
            SocketSend(ipaddress, port, "MSG");
            LanciaDadi();
        }

        private void txtMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SocketSend(ipaddress, port, txtMessage.Text);
                ResetChatTextbox();
            }
        }
        #endregion

        #region Application Methods

        bool CheckIfIPandPortAreValid(string IP, string Port)
        {
            if (IPAddress.TryParse(IP, out ipaddress) && int.Parse(Port) > 0 && int.Parse(Port) < 65536)
                return true;
            else return false;
        }
        private void ResetChatTextbox()
        {
            txtMessage.Clear();
        }

        private void Connetti()
        {
            if (!connesso)
            {
                btnDice.IsEnabled = true;
                SocketSend(ipaddress, port, "CNS");
                connesso = true;
            }
        }

        private void LanciaDadi()
        {
            numeroDado = new Random().Next(0, 11);
            SocketSend(ipaddress, port, numeroDado.ToString());
        }

        public static string GetLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        #endregion

        enum ChatState
        {
            DiceGame,
            Chat
        }
    }
}
