//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs">
//     Copyright (c) Michael Szvetits. All rights reserved.
// </copyright>
// <author>Michael Szvetits</author>
//-----------------------------------------------------------------------
namespace Chess.View.Window
{
    using Chess.Model.Game;
    using Chess.View.Selector;
    using Chess.ViewModel.Game;
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for the <see cref="MainWindow"/> window.
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChessGameVM game;

        private readonly PromotionSelector promotionSelector;

        public MainWindow()
        {
            this.InitializeComponent();
            this.promotionSelector = new PromotionSelector();
            this.StartNewGame();
        }

        // Shows the mode selection dialog then starts a new game with the chosen rulebook.
        private void StartNewGame()
        {
            var modeWindow = new ModeWindow();
            modeWindow.ShowDialog();

            this.game = new ChessGameVM(modeWindow.SelectedRulebook, this.Choose);
            this.DataContext = this.game;
        }

        private void BoardMouseDown(object sender, MouseButtonEventArgs e)
        {
            var point = Mouse.GetPosition(sender as Canvas);
            var row = 7 - (int)point.Y;
            var column = (int)point.X;
            var validRow = Math.Max(0, Math.Min(7, row));
            var validColumn = Math.Max(0, Math.Min(7, column));

            this.game.Select(validRow, validColumn);
        }

        private void ExitClick(object sender, EventArgs e)
        {
            this.Close();
        }

        // Re-opens the mode dialog when the user clicks New Game.
        private void NewGameClick(object sender, RoutedEventArgs e)
        {
            this.StartNewGame();
        }

        private void RemoveCompleted(object sender, EventArgs e)
        {
            this.game.Board.CleanUp();
        }

        private Update Choose(IList<Update> updates)
        {
            if (updates.Count == 0)
            {
                return null;
            }

            if (updates.Count == 1)
            {
                return updates[0];
            }

            // Multiple choices means a pawn promotion is available.
            var promotions = this.promotionSelector.Find(updates);
            var pieceWindow = new PieceWindow() { Owner = this };
            var selectedPiece = pieceWindow.Show(promotions.Keys);

            return
                selectedPiece != null
                    ? promotions[selectedPiece]
                    : null;
        }
    }
}