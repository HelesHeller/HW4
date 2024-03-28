using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp_TcpServer_Console
{
    class Program
    {
        static List<StreamWriter> clientStreams = new List<StreamWriter>();
        static List<string> usernames = new List<string>();
        static async Task Main(string[] args)
        {
            TcpListener server = null;
            try
            {
                IPAddress localAddr = IPAddress.Parse("26.129.29.176");
                server = new TcpListener(localAddr, 8888);
                server.Start();

                while (true)
                {
                    Console.WriteLine("Waiting for connections... ");
                    TcpClient client = await server.AcceptTcpClientAsync();
                    Console.WriteLine("Connected!");
                    _ = HandleClientAsync(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server.Stop();
            }
            Console.WriteLine("\nServer stopped. Press any key to exit...");
            Console.ReadKey();
        }

        static async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string username = await reader.ReadLineAsync();

            lock (clientStreams)
            {
                clientStreams.Add(writer);
                usernames.Add(username);
            }
            await BroadcastMessageAsync($"{username} has joined the chat ({DateTime.Now})");
            Console.WriteLine($"{username} has joined the chat.");

            string message;
            while (true)
            {
                try
                {
                    message = await reader.ReadLineAsync();
                    await BroadcastMessageAsync($"{username}: {message}");
                }
                catch (IOException)
                {
                    break;
                }
            }

            lock (clientStreams)
            {
                int index = clientStreams.IndexOf(writer);
                clientStreams.RemoveAt(index);
                usernames.RemoveAt(index);
            }

            await BroadcastMessageAsync($"{username} has left the chat ({DateTime.Now})");
            Console.WriteLine($"{username} has left the chat.");

            writer.Close();
            reader.Close();
            client.Close();
        }

        static async Task BroadcastMessageAsync(string message)
        {
            List<StreamWriter> streams;
            lock (clientStreams)
            {
                streams = new List<StreamWriter>(clientStreams);
            }

            foreach (StreamWriter writer in streams)
            {
                await writer.WriteLineAsync(message);
            }
        }
    }
}