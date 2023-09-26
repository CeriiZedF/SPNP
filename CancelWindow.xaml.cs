using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Логика взаимодействия для CancelWindow.xaml
    /// </summary>
    public partial class CancelWindow : Window
    {
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private int TaskCount = 0;
        private readonly object countLocker = new();

        public CancelWindow()
        {
            InitializeComponent();
        }
        //HomeWork
        #region RunProgressCancallable
        private async void StartBtn6_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();                // Параллельно
            RunProgressCancallable(ProgressBar10, cancellationTokenSource.Token, 2);                        
            RunProgressCancallable(ProgressBar11, cancellationTokenSource.Token, 3);
            RunProgressCancallable(ProgressBar12, cancellationTokenSource.Token, 4);
        }

        private void StopBtn6_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
        }

        private List<bool> bools = new List<bool>();
        private async void RunProgressCancallable(ProgressBar progressBar, CancellationToken cancellationToken, int time = 3)
        {
            lock(countLocker)
            {
                TaskCount++;
            }
            progressBar.Value = 0;
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    progressBar.Value += 10;
                    await Task.Delay(1000 * time / 10);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                bools.Add(true);
                return;
            }
            catch (Exception ex)
            {
                if(progressBar.Value < 100)
                {
                    progressBar.Foreground = Brushes.Tomato;
                    while (progressBar.Value > 0)
                    {
                        progressBar.Value--;
                        await Task.Delay(5);
                    }
                    bools.Add(false);
                }
            }
            finally
            {
                progressBar.Foreground = Brushes.Green;
                bool isLast;
                lock (countLocker)
                {
                    TaskCount--;
                    isLast = TaskCount == 0;
                }
                if (isLast)
                {
                    MessageBox.Show("All finished");
                    int tr = 0, fl = 0;
                    for(int i = 0; i < bools.Count; i++)
                    {
                        if (bools[i] == true)
                        {
                            tr++;
                        }
                        else
                        {
                            fl++;
                        }
                    }
                    MessageBox.Show($"{tr} - {fl}");
                    bools.Clear();
                }
            }
           
        }
        #endregion
        #region RunProgress
        private void StartRP_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();
            RunProgress(ProgressBarRP10, cancellationTokenSource.Token);                         // Параллельно
            RunProgress(ProgressBarRP11, cancellationTokenSource.Token, 4);
            RunProgress(ProgressBarRP12, cancellationTokenSource.Token, 2);
        }

        private void StopRP_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
        }

        private async void RunProgress(ProgressBar progressBar, CancellationToken cancellationToken, int time = 3)
        {

            for (int i = 0; progressBar.Value < 100; i++)
            {
                progressBar.Value += 10;
                await Task.Delay(1000 * time / 10);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
        #endregion
        #region RunProgressWaitable
        private async void StartRPW_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();
            //Треба Invoke 
            //await Task.Run(() => RunProgressWaitable(ProgressBar10));        
            //await Task.Run(() => RunProgressWaitable(ProgressBar11, 4));
            //await Task.Run(() => RunProgressWaitable(ProgressBar12, 2));

            await RunProgressWaitable(ProgressBarRPW10, cancellationTokenSource.Token);           // Послідовно
            await RunProgressWaitable(ProgressBarRPW11, cancellationTokenSource.Token, 4);
            await RunProgressWaitable(ProgressBarRPW12, cancellationTokenSource.Token, 2);
        }

        private void StopRPW_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
        }

        private async Task RunProgressWaitable(ProgressBar progressBar, CancellationToken cancellation, int time = 3)
        {
            progressBar.Value = 0;
            for (int i = 0; i < 10; i++)
            {
                if (cancellation.IsCancellationRequested)
                {
                    return;
                }
                progressBar.Value += 10;
                await Task.Delay(1000 * time / 10);
            }
        }
        #endregion
    }
}
