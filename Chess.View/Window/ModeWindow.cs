namespace Chess.View.Window
{
    using Chess.Model.Rule;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    // Dialog that lets the user pick between Traditional and Chess960 before a game starts.
    public class ModeWindow : Window
    {
        private readonly RadioButton radioChess960;

        // The rulebook matching whichever mode the user picked.
        public IRulebook SelectedRulebook { get; private set; }

        public ModeWindow()
        {
            this.Title = "New Game";
            this.Width = 420;
            this.Height = 320;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.NoResize;
            this.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));

            // Default to traditional in case the user closes the window without clicking Start
            this.SelectedRulebook = new StandardRulebook();

            var layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var content = new StackPanel { Margin = new Thickness(28, 24, 28, 16) };

            content.Children.Add(new TextBlock
            {
                Text = "Select Game Mode",
                Foreground = Brushes.White,
                FontSize = 17,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 18)
            });

            content.Children.Add(new RadioButton
            {
                Content = "Traditional Chess",
                Foreground = Brushes.White,
                FontSize = 13,
                IsChecked = true,
                Margin = new Thickness(0, 0, 0, 4)
            });

            content.Children.Add(new TextBlock
            {
                Text = "Standard FIDE starting position.",
                Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170)),
                FontSize = 11,
                Margin = new Thickness(20, 0, 0, 16)
            });

            this.radioChess960 = new RadioButton
            {
                Content = "Chess960 (Fischer Random)",
                Foreground = Brushes.White,
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 4)
            };
            content.Children.Add(this.radioChess960);

            content.Children.Add(new TextBlock
            {
                Text = "Back-rank pieces are randomized each game. Bishops always land on opposite-colored squares and the king is always placed between both rooks.",
                Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170)),
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(20, 0, 0, 0)
            });

            Grid.SetRow(content, 0);
            layout.Children.Add(content);

            // Button bar pinned to the bottom of the window
            var buttonRow = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(35, 35, 35)),
                Padding = new Thickness(28, 12, 28, 12)
            };

            var startButton = new Button
            {
                Content = "Start Game",
                Width = 130,
                Height = 36,
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Color.FromRgb(74, 144, 217)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 13,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            startButton.Click += (s, e) =>
            {
                this.SelectedRulebook = this.radioChess960.IsChecked == true
                    ? (IRulebook)new Chess960Rulebook()
                    : new StandardRulebook();

                this.DialogResult = true;
                this.Close();
            };

            buttonRow.Child = startButton;
            Grid.SetRow(buttonRow, 1);
            layout.Children.Add(buttonRow);

            this.Content = layout;
        }
    }
}