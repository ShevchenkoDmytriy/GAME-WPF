using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

class Program
{
    static void Main()
    {
        Server server = new Server(8888);
        server.Start();
    }
}

public class Server
{
    private TcpListener listener;
    private List<PlayerData> playerDataList;

    public Server(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
        playerDataList = new List<PlayerData>();
    }

    public void Start()
    {
        listener.Start();
        Console.WriteLine($"Server started. Waiting for connections on port {((IPEndPoint)listener.LocalEndpoint).Port}...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private void HandleClient(TcpClient tcpClient)
    {
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        while (true)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {data}");

                // Обробка отриманих від клієнта даних (впровадьте свою логіку тут)

                if (data.StartsWith("GameResult"))
                {
                    string json = data.Substring("GameResult|".Length);
                    var playerData = JsonConvert.DeserializeObject<PlayerData>(json);
                    playerDataList.Add(playerData);
                    Console.WriteLine($"Received game result from {playerData.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
                break;
            }
        }

        tcpClient.Close();
    }
}

public class PlayerData
{
    public string Name { get; set; }
    public double TotalTime { get; set; }
}
