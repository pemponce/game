using System.Windows;

namespace SpaceBattle
{
    public partial class MainMenuWindow : Window
    {
        public MainMenuWindow()
        {
            InitializeComponent();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow gameWindow = new MainWindow();
            gameWindow.Show();
            Close();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Открыты настройки игры");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
