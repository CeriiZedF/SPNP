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
using System.Windows.Threading;

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
        #region 1
        private void StartButton1_Click(object sender, RoutedEventArgs e)
        {
            // демонстрація проблеми - зависання інтерфейсу
            // протягом роботи методу-обробника події усі інші події
            // стають у чергу і не обробляються
            // оновлення вікна - це теж одна з подій, тому бігунок
            // відображається відразу заповненим, а не покроково
        }

        private void StopButton1_Click(object sender, RoutedEventArgs e)
        {
            // через зависання інтерфейсу кнопка не натискається протягом
            // роботи "Старт", жодні дії не зможуть зупинити її роботу
        }
        #endregion
        #region 2
        private void StartButton2_Click(object sender, RoutedEventArgs e)
        {
            new Thread(IncrementProgress).Start();
        }

        private void StopButton2_Click(object sender, RoutedEventArgs e)
        {
            new Thread(IncrementProgress).Interrupt();
        }

        private void IncrementProgress()
        {
            /* Проблема - з данного потоку не можна змінювати елементи,
             * які належать іншому потоку. Для доступу до елементів
             * інтерфейсу (вікна) слід делегувати виконання 
             *
             * 
             */
             
            for (int i = 0; i < 10; i++)
            {
                ProgressBar2.Value = i * 10;
                Thread.Sleep(300);
            }
            //ProgressBar2.Value = 100;
        }
#endregion
        #region 3
        private bool isStopped3;
        private void StartButton3_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar3.Value = 0;
            new Thread(IncrementProgress3).Start();
            isStopped3 = false;
        }

        private void StopButton3_Click(object sender, RoutedEventArgs e)
        {
            isStopped3 = true;
        }

        private void IncrementProgress3()
        {
            
            for (int i = 0; i <= 100 && !isStopped3; i++)
            {
                /* Делегування виконання дії (лямбди) до віконного (UI) потоку
                 * 
                 */
                this.Dispatcher.Invoke(
                    () => ProgressBar3.Value += 1
                );
                Thread.Sleep(50);
            }
            
        }
#endregion
        #region 4
        private bool isStopped4;
        private Thread? thread4;
        private void StartButton4_Click(object sender, RoutedEventArgs e)
        {
            if (thread4 == null)
            {
                if(ProgressBar4.Value == 100)
                {
                    ProgressBar4.Value = 0;
                }
                thread4 = new Thread(IncrementProgress4);
                isStopped4 = false;
                thread4.Start();
                StartButton4.IsEnabled = false;
                StopButton4.IsEnabled = true;
            }
            
        }

        private void StopButton4_Click(object sender, RoutedEventArgs e)
        {
            HandleStop();
        }

        private void HandleStop()
        {
            isStopped4 = true;
            StartButton4.IsEnabled = true;
            StopButton4.IsEnabled = false;
            thread4 = null;
        }

        private void IncrementProgress4()
        {
            for (int i = 0; i <= 100 && !isStopped4; i++)
            {
                this.Dispatcher.Invoke(
                    () => ProgressBar4.Value += 1
                );
                Thread.Sleep(50);
            }

            this.Dispatcher.Invoke(HandleStop);
        }
#endregion
        #region 5
        private bool isStopped5;
        private Thread? thread5;
        // Зупинка потоків - сучасний підхід
        CancellationTokenSource cts; //джерело токенів скасування
        private void StartButton5_Click(object sender, RoutedEventArgs e)
        {
            if (thread5 == null)
            {
                if (ProgressBar5.Value == 100)
                {
                    ProgressBar5.Value = 0;
                }
                int workTime = Convert.ToInt32(WorkTimeTextBox.Text);
                cts = new();
                thread5 = new Thread(IncrementProgress5);
                isStopped5 = false;
                thread5.Start(
                    new ThreadData5 { WorkTime = workTime, CancelToken = cts.Token});   // get - одержання токенів
                StartButton5.IsEnabled = false;
                StopButton5.IsEnabled = true;
            }

        }

        private void StopButton5_Click(object sender, RoutedEventArgs e)
        {
            //скасування потоків здійснюється черех джерело токенів
            cts.Cancel();
            //після цієї команди усі токени даного джерела переходять у скасования стан, але безпосередньо
            //це на потоки не вплине, перевіка стану токенів
            //має окремо здіснюватись у кожному стані

            //HandleStop5();
        }

        private void HandleStop5()
        {
            isStopped5 = true;
            StartButton5.IsEnabled = true;
            StopButton5.IsEnabled = false;
            thread5 = null;
        }

        private void IncrementProgress5(object? parameter)
        {
            /*
             * Аргументом може бути довільний об'єкт, але параметр 
             * прийме його як object. Відповідно, першими командами
             * є перетворення ...
             */
            if (parameter is ThreadData5 data)
            {
                for (int i = 0; i <= 100 && !isStopped5; i++)
                {
                    this.Dispatcher.Invoke(
                        () => ProgressBar5.Value += 1
                    );
                    Thread.Sleep(Convert.ToInt32(data.WorkTime));
                    //задача перевірки токена на скасованість - частина роботи потоку (скасуваня не впливає на потік, якщо ми це будемо ігнорувати)
                    if(data.CancelToken.IsCancellationRequested) 
                    {
                        break;
                    }// <- м'яка зупинка
                    // або за допомогою викидання виключення:
                    // data.CancelToken.ThrowCancellationRequested();   екстрена зупинка
                }
            }
            else
            {

            }

           this.Dispatcher.Invoke(HandleStop5);
        }

        class ThreadData5
        { 
            public int WorkTime { get; set; }
            public CancellationToken CancelToken { get; set; }
            // токен створений джерелом (CTS)  передається серед даних у потік
        }
        #endregion 5

        #region HomeWork_14.09.23
        private bool isStopped6;
        private List<Thread> thread6 = new List<Thread>();
        // Зупинка потоків - сучасний підхід
        CancellationTokenSource cts6 = new(); //джерело токенів скасування
        static int Counter = 0;
        private void StartButton6_Click(object sender, RoutedEventArgs e)
        {
            if (Counter > 2)
            {
                Counter = 0;
            }

            if (ProgressBar6.Value >= 100)
            {
                ProgressBar6.Value = 0;
                ProgressBar6_1.Value = 0;
                ProgressBar6_2.Value = 0;
            }


            thread6.Add(new Thread(IncrementProgress6));
            var temp = new ThreadData6 { WorkTime = 100, CancelToken = cts6.Token, Index = Counter };
            temp.bars.Add(ProgressBar6);
            temp.bars.Add(ProgressBar6_1);
            temp.bars.Add(ProgressBar6_2);
            thread6[Counter].Start(temp);   // get - одержання токенів
            Counter++;

            if (thread6.Count() >= 3)
            {
                StartButton6.IsEnabled = false;
                return;
            }


        }

        

        private void StopButton6_Click(object sender, RoutedEventArgs e)
        {
            //скасування потоків здійснюється черех джерело токенів
            cts6.Cancel();
            cts6 = new();
            //після цієї команди усі токени даного джерела переходять у скасования стан, але безпосередньо
            //це на потоки не вплине, перевіка стану токенів
            //має окремо здіснюватись у кожному стані
            thread6.Clear();
            Counter = 0;
            StartButton6.IsEnabled = true;

        }

        private void IncrementProgress6(object? parameter)
        {
            if (parameter is ThreadData6 data)
            {
                for (int i = 0; i <= 100; i++)
                {
                    this.Dispatcher.Invoke(
                        () => data.bars[data.Index].Value += 1

                    );
                    Thread.Sleep(data.WorkTime);

                    if (data.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }


            //this.Dispatcher.Invoke(HandleStop6);
        }

        class ThreadData6
        {
            public int WorkTime { get; set; }
            public CancellationToken CancelToken { get; set; }
            // токен створений джерелом (CTS)  передається серед даних у потік

            public List<ProgressBar> bars = new List<ProgressBar>();
            public int Index = 0;
            
        }
        #endregion
    }
}
