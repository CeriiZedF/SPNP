using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SPNP
{
    /// <summary>
    /// Логика взаимодействия для CountDownWindow.xaml
    /// </summary>
    public partial class CountDownWindow : Window
    {
        private Mutex? mutex;

        public CountDownWindow(Mutex? mutex)
        {
            this.mutex = mutex;
           InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(Tick);
        }

        private void Tick()
        {
            if (!mutex.WaitOne(1))
            {
                Dispatcher.Invoke(() =>
                {
                    progressBar1.Value -= 1;
                    if (progressBar1.Value <= 0)
                    {
                        this.DialogResult = false;
                    }
                    else
                    {
                        Tick();
                    }
                });
            }
            else
            {
                Dispatcher.Invoke(() => { this.DialogResult = true; });
                
            }
        }
    }
}
