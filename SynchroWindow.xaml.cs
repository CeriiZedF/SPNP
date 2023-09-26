using System;
using System.Collections.Generic;
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
        private double sum = 0;
        private int threadCount;  //кількість активних потоків

        public SynchroWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            sum = 100;
            LogTextBlock.Text = "";
            threadCount = 30;
            for (int i = 0; i < threadCount; i++)
            {
                new Thread(AddPercentSemaphore).Start(new MonthData() { Month = i + 1 });
            }
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
            Dispatcher.Invoke(() =>  
            {
                LogTextBlock.Text += $"{monthData?.Month}) {localSum}\n";  
            });
        }

        // об'єкт для синхронізації
        private object sumLocker = new();
        private void AddPercent(object? data)
        {
            // Метод що імітує звернення до мережі з одержанням 
            // даних про інфляцію за місяць та додає її до загальної суми
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

            // из-за того, что порядок не гарантируется, номер месяца не годится для определения
            // последнего потока, используем уменьшение счётчика потоков
            threadCount--;
            Thread.Sleep(1);  // если добавить тут паузу, то все потоки попадут под это условие, и каждый поток выведет итоговую запись
            if (threadCount == 0)
            {
                // добавляем итоговую запись
                Dispatcher.Invoke(() =>
                {
                    LogTextBlock.Text += $"------------------\nresult = {sum}\n";
                });
            }
        }

        class MonthData
        {
            public int Month { get; set; }
            public float Percent { get; set; }
        }

        private void AddPercent4()
        {
            // Метод що імітує звернення до мережі з одержанням 
            // даних про інфляцію за місяць та додає її до загальної суми
            lock (sumLocker)                            //переводить sumLocker у "закритий" стан
            {                                           //поки sumLocker "закритий"            
                                                                //lock не починають работу, чекаючи на "відкриття" об'єкту синхронізації
                Thread.Sleep(200);
                lock (sumLocker)
                {
                    sum = sum *= 1.1;
                    Dispatcher.Invoke(() =>
                    {
                        LogTextBlock.Text += sum + "\n";
                    });
                }
            }
        }

        private void AddPercent3()
        {
            // Метод що імітує звернення до мережі з одержанням 
            // даних про інфляцію за місяць та додає її до загальної суми
            lock (sumLocker)                            //переводить sumLocker у "закритий" стан
            {                                           //поки sumLocker "закритий"
                double localSum = sum;                  //lock не починають работу, чекаючи на "відкриття" об'єкту синхронізації
                Thread.Sleep(200);
                localSum *= 1.1; // 10%
                sum = localSum;

                Dispatcher.Invoke(() =>
                {
                    LogTextBlock.Text += sum + "\n";
                });
            }
        }
    }
}
