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
        #region HomeWork
        private static Mutex? mutexThreading;
        private const String mutexThreadingName = "SPNP_MPW_MUTEX";
        private void ThreadingButton_Click(object sender, RoutedEventArgs e)
        {
            try { mutexThreading = Mutex.OpenExisting(mutexThreadingName); }
            catch { }

            if (mutexThreading != null)
            {
                if (!mutexThreading.WaitOne(1))
                {
                    String message = "Запущено ішний екземпляр вікна";
                    MessageBox.Show(message);
                    return;
                }
            }
            else
            {
                mutexThreading = new Mutex(true, mutexThreadingName);
            }
            this.Hide();
            try { new ThreadingWindow().ShowDialog(); } catch { }
            this.Show();
            mutexThreading?.ReleaseMutex();
        }
        #endregion

        private void SynchroButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            try
            {
                new SynchroWindow().ShowDialog();
            }
           catch (Exception ex) { }
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

        private static Mutex? mutex;
        private const String mutexName = "SPNP_MPW_MUTEX";
        private void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            try { mutex = Mutex.OpenExisting(mutexName); }
            catch { }

            if (mutex != null)
            {
                if (!mutex.WaitOne(1))
                {
                    String message = "Запущено ішний екземпляр вікна";
                    MessageBox.Show(message);
                    return;
                }
            }
            else
            {
                mutex = new Mutex(true, mutexName);
            }
            this.Hide();
            try { new ProcessWindow().ShowDialog(); } catch { }
            this.Show();
            mutex?.ReleaseMutex();
        }

        private void ChainingButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new ChainingWindow().ShowDialog();
            this.Show();
        }

        private void DLLButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new DLLWindow().ShowDialog();
            this.Show();
        }
    }
}