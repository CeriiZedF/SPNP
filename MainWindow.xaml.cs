using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SPNP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ThreadingButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new ThreadingWindow().ShowDialog();
            this.Show();
        }

        private void SynchroButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new SynchroWindow().ShowDialog();
            this.Show();
        }

        private void TaskWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new TaskWindow().ShowDialog();
            this.Show();
        }

        private void Canceling_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new CancelWindow().ShowDialog();
            this.Show();
        }

        private void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new ProcessWindow().ShowDialog();
            this.Show();
        }
    }
}