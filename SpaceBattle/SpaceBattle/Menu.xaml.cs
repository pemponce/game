using System.Windows;
using System.Windows.Controls;

namespace SpaceBattle
{
    public partial class Menu
    {
        public Menu()
        {
            InitializeComponent();
        }
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            Close();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Добавьте код для закрытия приложения или выполнения действий при нажатии кнопки "Выход"
            Application.Current.Shutdown();
        }
    }
}