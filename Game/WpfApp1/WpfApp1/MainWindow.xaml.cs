using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private GameClient gameClient;
        private Stopwatch gameTimer;
        private int gridSize = 5;
        private int[,] numbers;
        private int currentNumber = 1;
        private string playerName;
        private TimeSpan gameTime;
        private bool gameStarted;

        public MainWindow()
        {
            InitializeComponent();
            gameClient = new GameClient();
            gameClient.CellClicked += GameClient_CellClicked;
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!gameStarted)
            {
                // Отримати ім'я гравця з текстового поля
                playerName = PlayerNameTextBox.Text;

                // Вивести ім'я гравця в консолі сервера
                gameClient.SendPlayerNameToServer(playerName);

                // Почати гру на клієнті
                gameClient.StartGame(playerName);

                // Очистити список результатів на клієнті
                ResultsListBox.ItemsSource = null;

                // Почати таймер гри
                StartNewGame();
                gameStarted = true;
            }
            else
            {
                MessageBox.Show("Game is already in progress. Finish the current game before starting a new one.");
            }
        }

        private void StartNewGame()
        {
            gameTimer = new Stopwatch();
            gameTimer.Start();

            numbers = GenerateRandomNumbers();
            currentNumber = 1;

            UpdateGameBoard();
        }

        private int[,] GenerateRandomNumbers()
        {
            int[,] nums = new int[gridSize, gridSize];
            var allNumbers = Enumerable.Range(1, gridSize * gridSize).ToList();
            var rand = new Random();

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    int index = rand.Next(allNumbers.Count);
                    nums[i, j] = allNumbers[index];
                    allNumbers.RemoveAt(index);
                }
            }

            return nums;
        }

        private void UpdateGameBoard()
        {
            GameGrid.Children.Clear();

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    Button button = new Button
                    {
                        Content = numbers[i, j].ToString(),
                        Tag = $"{i}_{j}",
                        Width = 40,
                        Height = 40,
                        Margin = new Thickness(5),
                        Background = System.Windows.Media.Brushes.LightGray,
                    };
                    button.Click += Cell_Click;

                    GameGrid.Children.Add(button);
                }
            }
        }

        private void GameClient_CellClicked(string playerName, int row, int col)
        {
            if (numbers[row, col] == currentNumber)
            {
                currentNumber++;

                if (currentNumber > gridSize * gridSize)
                {
                    gameTimer.Stop();
                    gameTime = gameTimer.Elapsed;
                    UpdatePlayerResults();

                    MessageBox.Show($"Congratulations, {playerName}! You completed the game in {gameTime.TotalSeconds} seconds.");

                    // Надіслати час гравця на сервер у форматі JSON
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(new { PlayerName = playerName, Time = gameTime.TotalSeconds });
                    gameClient.SendPlayerNameToServer(json);

                    gameStarted = false;
                }
                else
                {
                    UpdateGameBoard();
                }
            }
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string[] coordinates = button.Tag.ToString().Split('_');
            int row = int.Parse(coordinates[0]);
            int col = int.Parse(coordinates[1]);

            gameClient.MarkCell(row, col);
        }

        private void UpdatePlayerResults()
        {
            ResultsListBox.ItemsSource = new[] { $"{playerName}: {gameTime.TotalSeconds} seconds" };
        }
    }
}
