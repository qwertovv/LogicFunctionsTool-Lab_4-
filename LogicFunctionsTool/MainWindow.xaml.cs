using System.Windows;
using LogicFunctionsTool.ViewModels;

namespace LogicFunctionsTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Owner = this; // Делаем основное окно владельцем
            helpWindow.ShowDialog(); // Показываем как диалоговое окно
        }
    }
}