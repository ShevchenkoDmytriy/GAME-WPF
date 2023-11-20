using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

public class Game
{
    private int[,] board;
    private Dictionary<string, Player> players;
    private int size;
    private int currentPlayerIndex;
    private Timer timer;
    private DateTime startTime;

    public event Action<string, int, int> CellClicked;

    public Game(int size)
    {
        this.size = size;
        board = new int[size, size];
        players = new Dictionary<string, Player>();
        currentPlayerIndex = 0;
        timer = new Timer(UpdateTimer, null, Timeout.Infinite, Timeout.Infinite);
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        List<int> numbers = Enumerable.Range(1, size * size).ToList();
        Random random = new Random();

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                int index = random.Next(numbers.Count);
                board[row, col] = numbers[index];
                numbers.RemoveAt(index);
            }
        }
    }

    public void AddPlayer(string playerName, TcpClient client)
    {
        Player player = new Player(playerName, client);
        players.Add(playerName, player);
    }


    public void StartGame()
    {
        startTime = DateTime.Now;
        timer.Change(0, 1000);
    }

    public int[,] GetBoard()
    {
        return board;
    }

    public void MarkCell(string playerName, int row, int col)
    {
        if (row < 0 || row >= size || col < 0 || col >= size)
            return;

        if (board[row, col] == players[playerName].NextNumber)
        {
            board[row, col] = 0;
            players[playerName].NextNumber++;

            CellClicked?.Invoke(playerName, row, col);

            if (players[playerName].NextNumber == size * size + 1)
            {
                // Player won
                EndGame(playerName);
            }
        }
    }

    private void UpdateTimer(object state)
    {
        foreach (var player in players.Values)
        {
            player.UpdateTime();
        }
    }

    private void EndGame(string winner)
    {
        timer.Change(Timeout.Infinite, Timeout.Infinite);

        // Calculate player rankings based on time
        var rankings = players.Values.OrderBy(p => p.TotalTime).ToList();

        Console.WriteLine("Game Over!");
        Console.WriteLine($"Winner: {winner}");
        Console.WriteLine("Rankings:");

        for (int i = 0; i < rankings.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {rankings[i].Name} - {rankings[i].TotalTime} seconds");
        }
    }
}
