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
        IPEndPoint sourceSocket;
        string ipaddress;
        int port;
        public MainWindow() => InitializeComponent();

        private void btnInviaMSG_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SocketSend(IPAddress.Parse(ipaddress), port, txtMessage.Text);
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
                if (txtIP.Text != string.Empty && txtPort.Text != string.Empty)
                {
                    ipaddress = txtIP.Text;
                    port = int.Parse(txtPort.Text);
                    sourceSocket = new IPEndPoint(IPAddress.Parse("10.73.0.22"), 56000);
                    btnInviaMSG.IsEnabled = true;
                    Thread ricezione = new Thread(new ParameterizedThreadStart(SocketReceive));
                    ricezione.Start(sourceSocket);
                }
                else MessageBox.Show("Inserisci correttamente l' indirizzo IP e la porta", "Errore");
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message, "Errore");
            }
        }
        public async void SocketReceive(object socketsource)
        {
            IPEndPoint ipendpoint = (IPEndPoint)socketsource;
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
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            txtChat.Text += $"[{ipendpoint.Address}] " + message + "\n";
                        }));
                    }
                }
            });
        }
        public void SocketSend(IPAddress dest, int destPort, string message)
        {
            byte[] byteSent = Encoding.ASCII.GetBytes(message);
            Socket s = new Socket(dest.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint remote_endpoint = new IPEndPoint(dest, destPort);
            s.SendTo(byteSent, remote_endpoint);
            txtChat.Text += $"[Tu] " + message + "\n";
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Button_Click(object sender, RoutedEventArgs e) => Environment.Exit(1);
    }
}
