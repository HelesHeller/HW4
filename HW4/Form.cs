using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW4
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;

        public Form1()
        {
            InitializeComponent();
        }

        private async void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync("26.24.106.13", 8888);//настроен на мой айпи адрес

                NetworkStream stream = client.GetStream();
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                reader = new StreamReader(stream, Encoding.UTF8);

                await writer.WriteLineAsync(usernameTextBox.Text);

                _ = Task.Run(async () =>
                {
                    string message;
                    while ((message = await reader.ReadLineAsync()) != null)
                    {
                        Invoke(new Action(() =>
                        {
                            chatTextBox.AppendText(message + Environment.NewLine);
                        }));
                    }
                });

                userInfoLabel.Text = $"USER {usernameTextBox.Text} CONNECTED {DateTime.Now.ToLongTimeString()}";
                usersListBox.Items.Add(usernameTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to server: " + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void sendButton_Click(object sender, EventArgs e)
        {
            if (client != null && client.Connected)
            {
                try
                {
                    await writer.WriteLineAsync(sendTextBox.Text);
                    sendTextBox.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error sending message: " + ex.Message, "Send Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Not connected to server.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            if (client != null && client.Connected)
            {
                writer.Close();
                reader.Close();
                client.Close();
            }
            Application.Exit();
        }
    }
}
