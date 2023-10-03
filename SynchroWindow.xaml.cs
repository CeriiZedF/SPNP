using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Data;
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
    /// Логика взаимодействия для SynchroWindow.xaml
    /// </summary>
    public partial class SynchroWindow : Window
    {
        private const int Months = 12;
        private static Random random = new Random();
        private double sum = 0;
        private int threadCount;  //кількість активних потоків

        private static Mutex? mutex;
        private const String mutexName = "SPNP_SW_MUTEX";
        public SynchroWindow()
        {
            WaitOtherInstance();
            InitializeComponent();
        }

        private void WaitOtherInstance()
        {
            try{mutex = Mutex.OpenExisting(mutexName);}
            catch { }
            if(mutex == null)
            {
                mutex = new Mutex(true, mutexName);
            }
            else
            {
                if(!mutex.WaitOne(1))
                {
                    if(new CountDownWindow(mutex).ShowDialog() == false)
                    {
                        throw new ApplicationException();
                    }
                }
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            sum = 100;
            LogTextBlock.Text = String.Empty;
            threadCount = Months;
            float randPercent, avgPercent = 0;
            for (int i = 0; i < threadCount; i++)
            {
                randPercent = (float)Math.Round(random.NextDouble() * 20, 1); 
                avgPercent += randPercent;
                new Thread(AddPercentHomeWork).Start(new MonthData { Month = i + 1, Percent = randPercent });
            }
            LogTextBlock.Text += $"Average percent: {avgPercent / Months}\n";
        }
        private Semaphore semaphore = new Semaphore(3, 3);
        //Семафоры позволяют ограничить количество потоков, которые имеют доступ к определенным ресурсам.
        //В .NET семафоры представлены классом Semaphore.
        private void AddPercentSemaphore(object? data)
        { 
            semaphore.WaitOne();        //зменшуємо вільні місця, якщо немає чекаємо
            var monthData = data as MonthData; 
            Thread.Sleep(2000);  
            double localSum;
            localSum = sum = sum * 1.1;
            semaphore.Release();        //звільняємо одну чергу
            //Dispatcher.Invoke(() =>  
            //{
            //    LogTextBlock.Text += $"{monthData?.Month}) {localSum}\n";  
            //});
        }

        // об'єкт для синхронізації
        private object sumLocker = new();
        private void AddPercent1()
        {
            // метод, который имитирует обращение к сети с получением данных про инфляцию за месяц и добавляет её до общей суммы
            double localSum = sum;
            Thread.Sleep(200);  // ~запрос
            localSum *= 1.1;  // 10%
            sum = localSum;
            Dispatcher.Invoke(() =>
            {
                LogTextBlock.Text += $"{sum}\n";
            });

            // Проблема: все потоки выводят отдно и то же число - 110
            // Задержка выделяет проблему гарантируя, что все потоки начинаются со 100
            // Это илюстрирует общую проблему асинхронных задач - при работе с общим ресурсом, необходима синхронизация.
        }

        private void AddPercent2()
        {
            // метод, который имитирует обращение к сети с получением данных про инфляцию за месяц и добавляет её до общей суммы
            Thread.Sleep(200);  // ~запрос
            double localSum = sum;
            localSum *= 1.1;  // 10%
            sum = localSum;
            Dispatcher.Invoke(() =>
            {
                LogTextBlock.Text += $"{sum}\n";
            });

            // Перенос операции, уменьшает эффект, но не избавляется от него.
            // Числа выводятся разные, но с дублированием, вместо пошагового увеличения
        }


        private void AddPercent3()
        {
            // метод, который имитирует обращение к сети с получением данных про инфляцию за месяц и добавляет её до общей суммы
            lock (sumLocker)  // блок синхронизации (lock), переводит sunLocker в "закрытое" состояние
            {
                double localSum = sum;
                Thread.Sleep(200);  // ~запрос
                localSum *= 1.1;  // 10%
                sum = localSum;
                Dispatcher.Invoke(() =>
                {
                    LogTextBlock.Text += $"{sum}\n";
                });
            }  // завершение блока "открывает" sunLocker

            // пока sunLocker "закрытый", другие инструкции с блоком lock не начинают работу,
            // ожидая на "открытия" объекта синхронизации (sumLocker)

            // но вмещение всего тела метода у синхроблок производит к полной сериализации работы - теряется асинхронность
        }

        private void AddPercent4()
        {
            // метод, который имитирует обращение к сети с получением данных про инфляцию за месяц и добавляет её до общей суммы
            Thread.Sleep(200);  // ~запрос

            lock (sumLocker)  // транзакция - смена общего ресурса
            {
                sum *= 1.1;
            }

            Dispatcher.Invoke(() =>  // вывод - за транзакцией, поэтому результаты непредсказуемые, но последний результат будет правильный
            {
                LogTextBlock.Text += $"{sum}\n";
            });
        }

        private void AddPercent5()
        {
            // метод, который имитирует обращение к сети с получением данных про инфляцию за месяц и добавляет её до общей суммы
            Thread.Sleep(200);  // ~запрос

            double localSum;
            lock (sumLocker)  // транзакция - смена общего ресурса
            {
                localSum = sum *= 1.1;  // копия вычисленной суммы в локальную переменную
            }

            Dispatcher.Invoke(() =>  // вывод - за транзакцией, но с локальной переменной, которая не разделяется с другими потоками
            {
                LogTextBlock.Text += $"{localSum}\n";  // порядок этих операций тоже свободный, но все будут выведены
            });
        }

        private void AddPercent6(object? data)
        {
            // метод, который имитирует обращение к сети с получением данных про инфляцию за месяц и добавляет её до общей суммы
            // и выводит месяц(который посчитался уже), с помощью переданного параметра

            var monthData = data as MonthData;  // преобразование

            Thread.Sleep(200);  // ~запрос

            double localSum;
            lock (sumLocker)  // транзакция - смена общего ресурса
            {
                localSum = sum *= 1.1;  // копия вычисленной суммы в локальную переменную
            }

            Dispatcher.Invoke(() =>  // вывод - за транзакцией, но с локальной переменной, которая не разделяется с другими потоками
            {
                LogTextBlock.Text += $"{monthData?.Month}) {localSum}\n";  // порядок этих операций тоже свободный, но все будут выведены
            });
        }

        private void AddPercent7(object? data)
        {
            // метод, который имитирует обращение к сети с получением данных про инфляцию за месяц и добавляет её до общей суммы
            // и выводит месяц(который посчитался уже), с помощью переданного параметра

            var monthData = data as MonthData;  // преобразование

            Thread.Sleep(200);  // ~запрос

            double localSum;
            lock (sumLocker)  // транзакция - смена общего ресурса
            {
                localSum = sum *= 1.1;  // копия вычисленной суммы в локальную переменную
            }

            Dispatcher.Invoke(() =>  // вывод - за транзакцией, но с локальной переменной, которая не разделяется с другими потоками
            {
                LogTextBlock.Text += $"{monthData?.Month}) {localSum}\n";  // порядок этих операций тоже свободный,
                                                                           // но все будут выведены
            });
        }

        #region HW
        private object mainLocker = new();
        private void AddPercentHomeWork(object? data)
        {
            var months = data as MonthData;  
            Thread.Sleep(200); 

            double localSum;
            lock (mainLocker)  
            {
                localSum = sum += (sum * months!.Percent / 100); 
            }
            Dispatcher.Invoke(() =>
            {
                LogTextBlock.Text += $"{months?.Month}) {localSum} (+{months?.Percent}%)\n";
            });

            bool isLast = false;  
            lock (mainLocker) 
            {
                threadCount--;
                Thread.Sleep(1);
                if (threadCount == 0)  
                {
                    isLast = true;
                }
            }
            if (isLast)
            {
                Dispatcher.Invoke(() =>
                {
                    LogTextBlock.Text += $"------------------\nresult = {sum}\n";  // вывод результата
                });
            }
        }
        #endregion

        class MonthData
        {
            public int Month { get; set; }
            public float Percent { get; set; }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mutex?.ReleaseMutex();
        }
    }
}
