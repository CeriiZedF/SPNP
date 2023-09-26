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
    /// Логика взаимодействия для TaskWindow.xaml
    /// </summary>
    public partial class TaskWindow : Window
    {
        public TaskWindow()
        {
            InitializeComponent();
        }

        private void DemoButton1_Click(object sender, RoutedEventArgs e)
        {
            Task task = new Task(Demos); //об'єкт
            task.Start();                //та запуск

            Task task2 = Task.Run(Demos); // запуск і повернення запущенної
        }

        private void Demos()
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBlock.Text += "Demo1 start\n";
                Thread.Sleep(2000);
                LogTextBlock.Text += "Demo1 finished\n";
            });
        }

        // WPF дозволяє стоворювати async обробники подій
        private async void DemoButton2_Click(object sender, RoutedEventArgs e)
        {
            //var res = Demos2();
            //LogTextBlock.Text += $"Demo2 result: {res.Result} \n";
            Task<String> task = Demos2();
            String str = await task;
            LogTextBlock.Text += $"Demo2 result: {str} \n";
        }

        //Новий стиль
        private async Task<String> Demos2()
        {
            LogTextBlock.Text += "Demo2 starts\n";
            await Task.Delay(1000);
            return "Done";

        }
        #region HomeWork
        //   За допомогою багатозадачності реалізувати наступну схему Є два-три ProgressBar 
        //       і дві кнопки Одна стартує послідовну роботу РВ(один заповнюється, за ним інший) Друга - 
        //       паралельну(усі заповнюються одночасно) Закласти різний час заповнення РВ для наочності

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private async void StartButton1_Click(object sender, RoutedEventArgs e)
        {
            SetValueDefault();
            Task<ProgressBar> task = ProgressBarLogicAsync(ProgressBar1, 400, cancellationTokenSource.Token);
            Task<ProgressBar> task1 = ProgressBarLogicAsync(ProgressBar1_1, 500, cancellationTokenSource.Token);
            Task<ProgressBar> task2 = ProgressBarLogicAsync(ProgressBar1_2, 900, cancellationTokenSource.Token);
            var temp = await task;
            var temp1 = await task1;
            var temp2 = await task2;
        }

        private async void StartSequentialButton1_Click(object sender, RoutedEventArgs e)
        {
            SetValueDefault();
            await ProgressBarLogicAsync(ProgressBar1, 100, cancellationTokenSource.Token);
            await ProgressBarLogicAsync(ProgressBar1_1, 50, cancellationTokenSource.Token);
            await ProgressBarLogicAsync(ProgressBar1_2, 80, cancellationTokenSource.Token);
        }

        private void SetValueDefault()
        {
            ProgressBar1.Value = 0;
            ProgressBar1_1.Value = 0;
            ProgressBar1_2.Value = 0;
        }

        private void StopButton1_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new();
        }

        private async Task<ProgressBar> ProgressBarLogicAsync(ProgressBar progress, int delay, CancellationToken cancellationToken)
        {
            for(int i = 0; i < 100; i++)
            {
                progress.Value += 5;
                await Task.Delay(delay);
                if(cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
            return progress;
        }
        #endregion
    }
}
/* "Старий" тиль - використання класу Task
 * 
 *
*/ 
