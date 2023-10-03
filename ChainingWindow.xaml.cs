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
    /// Логика взаимодействия для ChainingWindow.xaml
    /// </summary>
    public partial class ChainingWindow : Window
    {
        private CancellationTokenSource _CTS;
        public ChainingWindow()
        {
            _CTS = new CancellationTokenSource();
            InitializeComponent();
        }
        #region HomeWork
        private void StartBtn1_Click(object sender, RoutedEventArgs e)
        {
            var task10 =
                ShowProgress(ProgressBar1, _CTS.Token)        // showProgress - повертає Task
                .ContinueWith(task10 =>            // до нього задаємо продовження
                    ShowProgress(ProgressBar1_1, _CTS.Token)    // task - результат Task (showProgress)
                    .ContinueWith(task11 =>
                        ShowProgress(ProgressBar1_2, _CTS.Token)
                    ));

            ShowProgress(ProgressBar2, _CTS.Token)
                .ContinueWith(task =>
                    ShowProgress(ProgressBar2_1, _CTS.Token)
                    .ContinueWith(task =>
                        ShowProgress(ProgressBar2_2, _CTS.Token)
                    ));
            _CTS = new CancellationTokenSource();
        }

        private void StopBtn6_Click(object sender, RoutedEventArgs e)
        {
            _CTS?.Cancel();
            
        }

        private async Task ShowProgress(ProgressBar progressBar, CancellationToken cancellationToken)
        {
            int delay = 0;
            if (progressBar == ProgressBar1) delay = 300;
            if (progressBar == ProgressBar1_1) delay = 200;
            if (progressBar == ProgressBar1_2) delay = 100;

            if (progressBar == ProgressBar2) delay = 100;
            if (progressBar == ProgressBar2_1) delay = 200;
            if (progressBar == ProgressBar2_2) delay = 300;


            for (int i = 0; i <= 10; i++)
            {
                await Task.Delay(delay);
                Dispatcher.Invoke(new Action(() =>
                {
                    progressBar.Value = i * 10;
                }));
                if(cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                //progressBar.Value = i * 10;   // Треба Dispatcher.Invoke
            }
        }
        
        private async void StartBtn2_Click(object sender, RoutedEventArgs e)
        {
            Task task1 = ShowProgress(ProgressBar1, _CTS.Token);
            Task task2 = ShowProgress(ProgressBar2, _CTS.Token);
            await task1;
            await task2; 

            Task task1_1 = ShowProgress(ProgressBar1_1, _CTS.Token);
            Task task2_1 = ShowProgress(ProgressBar2_1, _CTS.Token);
            await task1_1;
            await task2_1;

            Task task1_2 = ShowProgress(ProgressBar1_2, _CTS.Token);
            Task task2_2 = ShowProgress(ProgressBar2_2, _CTS.Token);
            await task1_2;
            await task2_2;
        }

        private void StopBtn2_Click(object sender, RoutedEventArgs e)
        {
            _CTS?.Cancel();
        }

        private void StartBtn3_Click(object sender, RoutedEventArgs e)
        {
            String str = "";
            AddHello(str)
                .ContinueWith(task =>
                {
                    String res = task.Result;
                    Dispatcher.Invoke(() => LogTextBlock.Text = res);
                    return AddWorld(res);
                })
                .Unwrap() //зняти одну "обгортку" Task<>, без неї task2 - Task<taskW> = Task<Task<String>>
                .ContinueWith(task2 =>  // а з нею - task2 - Task<String> (без однієї)
                {
                    String res = task2.Result;
                    Dispatcher.Invoke(() => LogTextBlock.Text = res);
                    return AddExclamation(res);
                })
                .Unwrap()
                .ContinueWith(task3 =>
                {
                    Dispatcher.Invoke(() => LogTextBlock.Text = task3.Result);
                });
            _CTS = new CancellationTokenSource();   
        }
        #endregion

        async Task<String> AddHello(String str)
        {
            await Task.Delay(1000);
            return str + " Hello";
        }

        async Task<String> AddWorld(String str)
        {
            await Task.Delay(1000);
            return str + " World";
        }

        async Task<String> AddExclamation(String str)
        {
            await Task.Delay(1000);
            return str + " Exclamation";
        }
    }
}

/* Task chaining. Continuations.
 * Нитки коду. Продовження
 * Задачі, як об'єкта, дозволяють зазначити інші задачі, які 
 * будуть виконуватись після того, як буде завершена дана.
 * Друга задача називається Продовженням (Continuation) першої задачі.
 * Утворенна ланцюгів з кілької продовжень складають "нитку" (chain)
 * - це є окремим стилем в асинхронному програмуванні.
 */