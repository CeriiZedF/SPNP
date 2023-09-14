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
    /// Логика взаимодействия для ThreadingWindow.xaml
    /// </summary>
    public partial class ThreadingWindow : Window
    {
        public ThreadingWindow()
        {
            InitializeComponent();
        }

        private void StartButton1_Click(object sender, RoutedEventArgs e)
        {
            // демонстрація проблеми - зависання інтерфейсу
            // протягом роботи методу-обробника події усі інші події
            // стають у чергу і не обробляються
            for (int i = 0; i < 10; i++)
            {
                ProgressBar1.Value = i * 10;
                Thread.Sleep(300);
            }
            ProgressBar1.Value = 100;
            // оновлення вікна - це теж одна з подій, тому бігунок
            // відображається відразу заповненим, а не покроково
        }

        private void StopButton1_Click(object sender, RoutedEventArgs e)
        {
            // через зависання інтерфейсу кнопка не натискається протягом
            // роботи "Старт", жодні дії не зможуть зупинити її роботу
        }
    }
}
