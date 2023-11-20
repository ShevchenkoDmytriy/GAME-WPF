// Player.cs
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Player
{
    public string Name { get; private set; }
    public int NextNumber { get; set; }
    public double TotalTime { get; private set; }

    private DateTime startTime;
    private TcpClient client;
    private NetworkStream stream;

    public Player(string name, TcpClient client)
    {
        Name = name;
        NextNumber = 1;
        TotalTime = 0;
        startTime = DateTime.Now;
        this.client = client;
        stream = client.GetStream();

        // Start a new thread to listen for player results
        Thread resultThread = new Thread(ListenForResult);
        resultThread.Start();
    }

    public void UpdateTime()
    {
        TotalTime = (DateTime.Now - startTime).TotalSeconds;
    }

    public void SendResults(string resultsJson)
    {
        byte[] data = Encoding.ASCII.GetBytes($"PlayerResults|{resultsJson}");
        stream.Write(data, 0, data.Length);
    }

    private void ListenForResult()
    {
        try
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                ProcessResultData(data);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listening for results: {ex.Message}");
        }
    }

    private void ProcessResultData(string data)
    {
        string[] parts = data.Split('|');
        string messageType = parts[0];

        switch (messageType)
        {
            case "PlayerResults":
                string resultsJson = parts[1];
                Console.WriteLine($"Results from other players:\n{resultsJson}");
                // Process results as needed
                break;
                // Handle other message types if necessary
        }
    }
}
