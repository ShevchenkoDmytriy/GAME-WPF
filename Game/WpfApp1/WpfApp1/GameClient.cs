using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

public class GameClient
{
    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];
    private string playerName;

    public event Action<string, int, int> CellClicked;

    public void StartGame(string playerName)
    {
        this.playerName = playerName;
        ConnectToServer();
        SendPlayerName(playerName);
        WaitForData();
    }

    private void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect("127.0.0.1", 8888);
            stream = client.GetStream();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to server: {ex.Message}");
            // Обробка помилки підключення, наприклад, спроба повторного підключення або вивід повідомлення про помилку
        }
    }

    private void WaitForData()
    {
        Thread receiveThread = new Thread(ReceiveData);
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        try
        {
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                ProcessData(data);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving data: {ex.Message}");
            // Обробка помилки при зчитуванні даних, наприклад, закриття з'єднання та вивід повідомлення про помилку
        }
    }

    private void ProcessData(string data)
    {
        string[] parts = data.Split('|');
        string messageType = parts[0];

        switch (messageType)
        {
            case "Board":
                UpdateBoard(parts[1]);
                break;
            case "CellClicked":
                int row = int.Parse(parts[1]);
                int col = int.Parse(parts[2]);
                CellClicked?.Invoke(parts[3], row, col);
                break;
            case "PlayerResults":
                HandlePlayerResults(parts[1]);
                break;
                // Обробка інших типів повідомлень за потреби
        }
    }

    private void UpdateBoard(string boardData)
    {
        // Обробка оновлення графічного інтерфейсу гри
    }

    public void MarkCell(int row, int col)
    {
        string message = $"MarkCell|{row}|{col}|{playerName}";
        SendData(message);
    }

    private void SendPlayerName(string playerName)
    {
        string message = $"PlayerName|{playerName}";
        SendData(message);
    }

    private void SendData(string message)
    {
        try
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending data: {ex.Message}");
            // Обробка помилки при відправленні даних, наприклад, закриття з'єднання та вивід повідомлення про помилку
        }
    }

    private void HandlePlayerResults(string resultsJson)
    {
        // Серіалізуйте результати гравця та обробіть їх, наприклад, виведіть їх у консоль
        var playerResults = JsonConvert.DeserializeObject<List<string>>(resultsJson);
        foreach (var result in playerResults)
        {
            Console.WriteLine(result);
        }
    }
    public void SendPlayerNameToServer(string playerName)
    {
        string message = $"PlayerName|{playerName}";
        SendData(message);
    }
}
