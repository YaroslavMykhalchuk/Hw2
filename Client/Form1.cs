using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using StreetLibrary;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Client
{
    public partial class Form1 : Form
    {

        private readonly string connectionIP;
        private readonly string[] addressparts;
        public Form1()
        {
            InitializeComponent();

            connectionIP = ConfigurationManager.AppSettings["ServerIP"];
            addressparts = connectionIP.Split(":");
        }

        private async void button_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBoxInput.Text, out int zipCode))
            {
                MessageBox.Show("Invalid zip code.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await ConnectServerAsync();
        }

        private async Task ConnectServerAsync()
        {
            IPAddress address = IPAddress.Parse(addressparts[0]);
            IPEndPoint endPoint = new IPEndPoint(address, Convert.ToInt32(addressparts[1]));

            using (Socket client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
            {
                try
                {
                    await client_socket.ConnectAsync(endPoint);

                    if (client_socket.Connected)
                    {
                        byte[] buff = new byte[1024];
                        byte[] buff1 = new byte[1024];
                        int receiveBytes;
                        string convertedFromBytes;
                        List<Street> streets = new List<Street>();
                        textBoxOuput.Text = string.Empty;

                        do
                        {
                            buff = Encoding.Default.GetBytes(textBoxInput.Text);
                            await client_socket.SendAsync(buff);

                            await Task.Delay(1000);

                            receiveBytes = await client_socket.ReceiveAsync(buff1);
                            convertedFromBytes = Encoding.Default.GetString(buff1, 0, receiveBytes);
                            streets = JsonSerializer.Deserialize<List<Street>>(convertedFromBytes);

                            if (streets.Count == 0)
                            {
                                textBoxOuput.Text = "The streets with such zip code non-available";
                            }
                            else
                            {
                                foreach(var street in streets)
                                {
                                    textBoxOuput.Text += $"{street.Name} : {street.ZipCode}{Environment.NewLine}";
                                }
                            }

                        } while (client_socket.Available > 0);


                    }
                }
                catch (SocketException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally { client_socket.Close(); }
            }
        }
    }
}