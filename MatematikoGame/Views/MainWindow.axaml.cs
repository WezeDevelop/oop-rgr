using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;

namespace MatematikoGame.Views
{
    public partial class MainWindow : Window
    {
        private List<int> deck = new();
        private int[,] playerBoard = new int[5, 5];
        private int[,] computerBoard = new int[5, 5];
        private Button[,] playerButtons = new Button[5, 5];
        private Label[,] computerLabels = new Label[5, 5];
        private string currentTurn = "Player";
        private int? currentNumber;
        private Label? currentLabel;
        private int remainingMoves;
        private TextBox? scoreText;
        private ObservableCollection<string> history = new();
        private Random random = new();
        private ContentControl? mainContent;

        public MainWindow()
        {
            InitializeComponent();
            mainContent = this.FindControl<ContentControl>("MainContent");
            ShowStartScreen();
        }

        private void ShowStartScreen()
        {
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 20,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var title = new TextBlock
            {
                Text = "Математико",
                FontSize = 24,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var rulesText = @"Гравці по черзі тягнуть випадкові числа від 1 до 13 (кожне по 4 рази).
Потрібно розмістити числа у поле 5x5, щоб набрати максимальну кількість балів.

Комбінації (рядок / стовпець / діагональ):
- 1 пара: 10 / 20 балів
- 2 пари: 20 / 30 балів
- 3 однакових: 40 / 50 балів
- 3 однакових і 2 інших однакових: 80 / 90 балів
- 4 однакових: 160 / 170 балів
- 5 послідовних чисел: 50 / 60 балів
- 1,13,12,11,10: 150 / 160 балів
- 4 одиниці: 200 / 210 балів
- 3 рази 1 і 2 рази 13: 100 / 110 балів";

            var rules = new TextBlock
            {
                Text = rulesText,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 600,
                TextAlignment = TextAlignment.Left
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 20,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var startButton = new Button
            {
                Content = "Почати гру",
                FontSize = 14,
                Padding = new Thickness(20, 10)
            };
            startButton.Click += StartButton_Click;

            var historyButton = new Button
            {
                Content = "Історія ігор",
                FontSize = 14,
                Padding = new Thickness(20, 10)
            };
            historyButton.Click += HistoryButton_Click;

            buttonPanel.Children.Add(startButton);
            buttonPanel.Children.Add(historyButton);

            stackPanel.Children.Add(title);
            stackPanel.Children.Add(rules);
            stackPanel.Children.Add(buttonPanel);

            if (mainContent != null)
                mainContent.Content = stackPanel;
        }

        private void StartButton_Click(object? sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void HistoryButton_Click(object? sender, RoutedEventArgs e)
        {
            ShowHistory();
        }

        private void ShowHistory()
        {
            var historyWindow = new Window
            {
                Title = "Історія ігор",
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            var title = new TextBlock
            {
                Text = "Історія ігор",
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var historyText = new TextBox
            {
                Height = 300,
                Width = 550,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true
            };

            if (history.Count > 0)
            {
                historyText.Text = string.Join("\n", history.Select(h => $"- {h}"));
            }
            else
            {
                historyText.Text = "Немає завершених ігор.";
            }

            stackPanel.Children.Add(title);
            stackPanel.Children.Add(historyText);

            historyWindow.Content = stackPanel;
            historyWindow.Show(this);
        }

        private void StartGame()
        {
            deck = new List<int>();
            for (int i = 1; i <= 13; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    deck.Add(i);
                }
            }
            
            // Shuffle deck
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }

            playerBoard = new int[5, 5];
            computerBoard = new int[5, 5];
            playerButtons = new Button[5, 5];
            computerLabels = new Label[5, 5];
            currentTurn = "Player";
            currentNumber = null;
            remainingMoves = 50;

            CreateGameWidgets();
        }

        private void CreateGameWidgets()
        {
            var mainPanel = new DockPanel { Margin = new Thickness(10) };

            // Top panel with boards
            var boardsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 20,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Player board
            var playerFrame = new Border
            {
                BorderBrush = Brushes.Blue,
                BorderThickness = new Thickness(2),
                Background = new SolidColorBrush(Color.FromRgb(230, 247, 255)),
                Padding = new Thickness(10)
            };

            var playerPanel = new StackPanel();
            var playerTitle = new TextBlock
            {
                Text = "Поле гравця",
                FontSize = 14,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            
            var playerGrid = new Grid();
            for (int i = 0; i < 5; i++)
            {
                playerGrid.RowDefinitions.Add(new RowDefinition());
                playerGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    var btn = new Button
                    {
                        Width = 60,
                        Height = 60,
                        FontSize = 20,
                        FontWeight = FontWeight.Bold,
                        Background = Brushes.White,
                        Margin = new Thickness(2)
                    };
                    
                    int row = i, col = j;
                    btn.Click += (s, e) => PlaceNumber(row, col);
                    
                    Grid.SetRow(btn, i);
                    Grid.SetColumn(btn, j);
                    playerGrid.Children.Add(btn);
                    playerButtons[i, j] = btn;
                }
            }

            playerPanel.Children.Add(playerTitle);
            playerPanel.Children.Add(playerGrid);
            playerFrame.Child = playerPanel;

            // Computer board
            var computerFrame = new Border
            {
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(2),
                Background = new SolidColorBrush(Color.FromRgb(255, 230, 240)),
                Padding = new Thickness(10)
            };

            var computerPanel = new StackPanel();
            var computerTitle = new TextBlock
            {
                Text = "Поле комп'ютера",
                FontSize = 14,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var computerGrid = new Grid();
            for (int i = 0; i < 5; i++)
            {
                computerGrid.RowDefinitions.Add(new RowDefinition());
                computerGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    var lbl = new Label
                    {
                        Width = 60,
                        Height = 60,
                        FontSize = 20,
                        FontWeight = FontWeight.Bold,
                        Background = new SolidColorBrush(Color.FromRgb(255, 230, 230)),
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(2)
                    };
                    
                    Grid.SetRow(lbl, i);
                    Grid.SetColumn(lbl, j);
                    computerGrid.Children.Add(lbl);
                    computerLabels[i, j] = lbl;
                }
            }

            computerPanel.Children.Add(computerTitle);
            computerPanel.Children.Add(computerGrid);
            computerFrame.Child = computerPanel;

            boardsPanel.Children.Add(playerFrame);
            boardsPanel.Children.Add(computerFrame);

            DockPanel.SetDock(boardsPanel, Dock.Top);
            mainPanel.Children.Add(boardsPanel);

            // Control panel
            var controlPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 10)
            };

            var nextButton = new Button
            {
                Content = "Наступне число",
                FontSize = 12,
                Padding = new Thickness(10, 5)
            };
            nextButton.Click += (s, e) => NextNumber();

            currentLabel = new Label
            {
                Content = "Натисни 'Наступне число'",
                FontSize = 12,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            var scoreButton = new Button
            {
                Content = "Підрахувати бали",
                FontSize = 12,
                Padding = new Thickness(10, 5)
            };
            scoreButton.Click += (s, e) => CalculateScore();

            controlPanel.Children.Add(nextButton);
            controlPanel.Children.Add(currentLabel);
            controlPanel.Children.Add(scoreButton);

            DockPanel.SetDock(controlPanel, Dock.Top);
            mainPanel.Children.Add(controlPanel);

            // Score text
            scoreText = new TextBox
            {
                Height = 300,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 10,
                Margin = new Thickness(0, 10, 0, 10)
            };

            DockPanel.SetDock(scoreText, Dock.Top);
            mainPanel.Children.Add(scoreText);

            // Return button
            var returnButton = new Button
            {
                Content = "Повернутись до початку",
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(20, 10)
            };
            returnButton.Click += (s, e) => ShowStartScreen();

            DockPanel.SetDock(returnButton, Dock.Bottom);
            mainPanel.Children.Add(returnButton);

            if (mainContent != null)
                mainContent.Content = mainPanel;
        }

        private async void NextNumber()
        {
            if (deck.Count == 0)
            {
                await ShowMessage("Кінець", "Колода закінчилась!");
                return;
            }
            if (currentNumber != null)
                return;

            currentNumber = deck[0];
            deck.RemoveAt(0);
            if (currentLabel != null)
                currentLabel.Content = $"{currentTurn}: число {currentNumber}";

            if (currentTurn == "Computer")
            {
                await Task.Delay(500);
                ComputerMove();
            }
        }

        private void PlaceNumber(int x, int y)
        {
            if (currentNumber == null || currentTurn != "Player")
                return;
            if (playerBoard[x, y] != 0)
                return;

            playerBoard[x, y] = currentNumber.Value;
            playerButtons[x, y].Content = currentNumber.ToString();
            playerButtons[x, y].IsEnabled = false;
            playerButtons[x, y].Background = new SolidColorBrush(Color.FromRgb(153, 230, 255));
            
            EndTurn();
        }

        private void ComputerMove()
        {
            int bestScore = -1;
            var bestPositions = new List<(int, int)>();

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (computerBoard[i, j] == 0)
                    {
                        var tempBoard = (int[,])computerBoard.Clone();
                        tempBoard[i, j] = currentNumber!.Value;
                        int tempScore = ScoreBoard(tempBoard, true);

                        if (tempScore > bestScore)
                        {
                            bestScore = tempScore;
                            bestPositions.Clear();
                            bestPositions.Add((i, j));
                        }
                        else if (tempScore == bestScore)
                        {
                            bestPositions.Add((i, j));
                        }
                    }
                }
            }

            if (bestPositions.Count > 0)
            {
                var (x, y) = bestPositions[random.Next(bestPositions.Count)];
                computerBoard[x, y] = currentNumber!.Value;
                computerLabels[x, y].Content = currentNumber.ToString();
                computerLabels[x, y].Background = new SolidColorBrush(Color.FromRgb(255, 204, 204));
            }

            EndTurn();
        }

        private async void EndTurn()
        {
            remainingMoves--;
            currentNumber = null;
            if (currentLabel != null)
                currentLabel.Content = "Натисни 'Наступне число'";

            if (remainingMoves == 0)
            {
                CalculateScore();
                return;
            }

            currentTurn = currentTurn == "Player" ? "Computer" : "Player";
            if (currentTurn == "Computer")
            {
                await Task.Delay(100);
                NextNumber();
            }
        }

        private void CalculateScore()
        {
            if (scoreText == null) return;

            scoreText.Text = "Оцінка поля гравця:\n";
            int playerScore = ScoreBoard(playerBoard, false);
            scoreText.Text += "\nОцінка поля комп'ютера:\n";
            int computerScore = ScoreBoard(computerBoard, false);

            string result = $"\nПідсумок: Гравець - {playerScore} балів, Комп'ютер - {computerScore} балів.\n";
            if (playerScore > computerScore)
                result += "Переміг гравець!";
            else if (playerScore < computerScore)
                result += "Переміг комп'ютер!";
            else
                result += "Нічия!";

            scoreText.Text += result;
            history.Add(result);

            scoreText.Text += "\n\nІсторія ігор:\n";
            foreach (var entry in history)
            {
                scoreText.Text += $"- {entry}\n";
            }
        }

        private int ScoreBoard(int[,] board, bool simulate)
        {
            int score = 0;
            var lines = new List<int[]>();
            var lineTypes = new List<string>();

            // Rows
            for (int i = 0; i < 5; i++)
            {
                var row = new int[5];
                for (int j = 0; j < 5; j++)
                    row[j] = board[i, j];
                lines.Add(row);
                lineTypes.Add("Рядок");
            }

            // Columns
            for (int j = 0; j < 5; j++)
            {
                var col = new int[5];
                for (int i = 0; i < 5; i++)
                    col[i] = board[i, j];
                lines.Add(col);
                lineTypes.Add("Стовпець");
            }

            // Diagonals
            var diag1 = new int[5];
            var diag2 = new int[5];
            for (int i = 0; i < 5; i++)
            {
                diag1[i] = board[i, i];
                diag2[i] = board[i, 4 - i];
            }
            lines.Add(diag1);
            lines.Add(diag2);
            lineTypes.Add("Діагональ");
            lineTypes.Add("Діагональ");

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains(0))
                    continue;

                var (s, desc) = EvaluateLine(lines[i], lineTypes[i]);
                score += s;
                if (!simulate && scoreText != null)
                {
                    scoreText.Text += $"{lineTypes[i]}: [{string.Join(", ", lines[i])}] -> {desc}: {s} балів\n";
                }
            }

            if (!simulate && scoreText != null)
            {
                scoreText.Text += $"\nПідсумок: {score} балів\n";
            }

            return score;
        }

        private (int score, string desc) EvaluateLine(int[] line, string lineType)
        {
            int score = 0;
            var descriptions = new List<string>();
            var counter = line.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var values = counter.Values.ToList();

            bool isDiagonal = lineType == "Діагональ";

            if (values.Contains(4))
            {
                score += isDiagonal ? 170 : 160;
                descriptions.Add("4 однакових");
            }
            else if (values.Contains(3) && values.Contains(2))
            {
                score += isDiagonal ? 90 : 80;
                descriptions.Add("3+2 однакових");
            }
            else if (values.Count(v => v == 2) == 2)
            {
                score += isDiagonal ? 30 : 20;
                descriptions.Add("2 пари");
            }
            else if (values.Contains(3))
            {
                score += isDiagonal ? 50 : 40;
                descriptions.Add("3 однакових");
            }
            else if (values.Count(v => v == 2) == 1)
            {
                score += isDiagonal ? 20 : 10;
                descriptions.Add("1 пара");
            }

            var sortedLine = line.OrderBy(x => x).ToArray();
            bool isSequence = true;
            for (int i = 1; i < sortedLine.Length; i++)
            {
                if (sortedLine[i] - sortedLine[i - 1] != 1)
                {
                    isSequence = false;
                    break;
                }
            }

            if (isSequence)
            {
                score += isDiagonal ? 60 : 50;
                descriptions.Add("Послідовність");
            }

            var lineSet = new HashSet<int>(line);
            if (lineSet.SetEquals(new HashSet<int> { 1, 13, 12, 11, 10 }))
            {
                score += isDiagonal ? 160 : 150;
                descriptions.Add("1,13,12,11,10");
            }

            if (line.Count(x => x == 1) == 4)
            {
                score += isDiagonal ? 210 : 200;
                descriptions.Add("4 одиниці");
            }

            if (line.Count(x => x == 1) == 3 && line.Count(x => x == 13) == 2)
            {
                score += isDiagonal ? 110 : 100;
                descriptions.Add("3 рази 1, 2 рази 13");
            }

            return (score, descriptions.Count > 0 ? string.Join(", ", descriptions) : "нема комбінацій");
        }

        private async Task ShowMessage(string title, string message)
        {
            var messageBox = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var panel = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 20
            };

            panel.Children.Add(new TextBlock { Text = message });
            
            var okButton = new Button
            {
                Content = "OK",
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(20, 5)
            };
            okButton.Click += (s, e) => messageBox.Close();

            panel.Children.Add(okButton);
            messageBox.Content = panel;
            
            await messageBox.ShowDialog(this);
        }
    }
}