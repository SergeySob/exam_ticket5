using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace exam_ticket5
{
    public partial class MainWindow : Window
    {
        private const int Port = 12345;
        private readonly UdpClient udpClient;
        private bool isRunning;

        public MainWindow()
        {
            InitializeComponent();
            udpClient = new UdpClient(Port);
            udpClient.Client.ReceiveTimeout = 500;
            isRunning = true;

            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            string targetIp = txtIP.Text;
            string message = txtMessage.Text;

            if (string.IsNullOrWhiteSpace(targetIp) || string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show("IP or message cannot be empty.");
                return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                udpClient.Send(data, data.Length, targetIp, Port);

                AppendLog($"Me: {message}");
                txtMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}");
            }
        }

        private void ReceiveMessages()
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, Port);

            while (isRunning)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref remoteEP);
                    string message = Encoding.UTF8.GetString(data);

                    Dispatcher.Invoke(() => AppendLog($"{remoteEP.Address}: {message}"));
                }
                catch (SocketException) { /* Timeout expected */ }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => AppendLog($"Error receiving message: {ex.Message}"));
                }
            }
        }

        private void AppendLog(string message)
        {
            txtLog.AppendText(message + Environment.NewLine);
            txtLog.ScrollToEnd();
        }

        protected override void OnClosed(EventArgs e)
        {
            isRunning = false;
            udpClient.Close();
            base.OnClosed(e);
        }
    }
}
