using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для ProcessWindow.xaml
    /// </summary>
    public partial class ProcessWindow : Window
    {
        private static Mutex? mutex;
        private static String mutexName = "SPNP_MUTEX";
        public ProcessWindow()
        {
            CheckPreviousLauch();
            InitializeComponent();
        }

        private void CheckPreviousLauch()
        {
            /* Синхронізація між процесами.
            * Якщо робота відбувається в одному процесі, то навіть
            * різні потоки можуть мати доступ до спільних ресурсів,
            * у т.ч. ресурсів синхронізації.
            * Коли мова іде про різні процеси, то єдиним способом
            * розподілити ресурс виступає операційна система. Вона
            * дозволяє реєструвати ресурси за іменем, зокерма, Мьютекси.
            */
            try  // Mutex реєструється при першому запуску вікна,
            {    // тому він може вже бути наявним у системі
                mutex = Mutex.OpenExisting(mutexName);  // намагаємось відкрити
            }
            catch { }

            if (mutex != null)  // Mutex зареєстрований у системі - інший процес його зареєстував
            {   // Хоча він зареєстрований, він може бути як відкритим, так і закритим
                if (!mutex.WaitOne(1))  // перевірка на закритість - спроба чекати його невеликий час
                {   // не пізніше ніж 1 мс WaitOne(1) поверне false, якщо Mutex не звільниться
                    // або успішно переведе його у закритий стан (і поверне true)
                    // Подальший код - у випадку роботи іншого вікна
                    String message = "Запущено ішний екземпляр вікна";
                    MessageBox.Show(message);
                    // Зупинити конструктор можна лише виключенням
                    throw new ApplicationException(message);
                    // (не забути try-catch при створенні даного вікна)
                }
            }
            else    // сюди ми потрапляємо якщо Мьютекс не зареєстрований,
            {       // значить це перший запуск вікна взагалі -
                    // створюємо Мьютекс із заданим іменем та у "закритому" стані (true)
                mutex = new Mutex(true, mutexName);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Синхронізація між процесами
            // Якщо робота відбувається в одному процесі, то навіть
            // різні потоки можуть мати доступ до спільних ресурсів,
            // Коли мова іде про різні процеси, то єдиним способом
            // розподілити ресурс виступає операційна система. Вона 
            // дозволяє реєструвати ресурси за іменем, зокерма - мьютекси
            try
            {
                mutex = Mutex.OpenExisting(mutexName);
            }
            catch (Exception ex) { }
            if (mutex != null) //Mutex - зареєстрований у системі - інший процес його зареєстрував
            {
                if (!mutex.WaitOne())
                {
                    MessageBox.Show("Запущено інши екземпляр вікна");
                    mutex = null;
                    Close();
                    return;
                }
                else
                {
                    mutex = new Mutex(true, mutexName);// 1 parameter true - close
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mutex?.ReleaseMutex();
            //mutex?.Dispose();
        }

        private void ShowProcesses_Click(object sender, RoutedEventArgs e)
        {
            Process[] processes = Process.GetProcesses();
            ProcTreeView.Items.Clear();
            //ProcTextBlock.Text = "";
            TreeViewItem? item = null;
            String prevName = "";
            foreach (Process process in processes.OrderBy(p => p.ProcessName))
            {
                //ProcTextBlock.Text += String.Format("{0} {1}\n", process.Id, process.ProcessName);
                if (prevName != process.ProcessName)
                {
                    prevName = process.ProcessName;
                    item = new TreeViewItem() { Header = prevName };
                    ProcTreeView.Items.Add(item);
                }
                var subItem = new TreeViewItem()
                {
                    Header = String.Format("{0} {1}", process.Id, process.ProcessName),
                    Tag = process
                };

                subItem.MouseDoubleClick += TreeViewItem_MouseDoubleClick;
                item?.Items.Add(subItem);
                //ProcTreeView.Items.Add(String.Format("{0} {1}\n", process.Id, process.ProcessName));
            }
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(sender is TreeViewItem item)
            {
                String message = "";
                if(item.Tag is Process process)
                {
                    int countThread = 0;
                    foreach(ProcessThread thread in process.Threads)
                    {
                        countThread++;
                        try
                        {
                            message += thread.Id + "\r\nTotalProcessorTime: " + thread.TotalProcessorTime;
                        }
                        catch (Exception)
                        {

                        }
                    }
                    
                    message += "\nRAM:" + (process.PeakWorkingSet64);
                    message += "\nCountThread: " + countThread;
                }
                else
                {
                    message = "No process in tag";
                }
                MessageBox.Show(message);
            }
        }

        #region starting .exe programm and exit 
        private Process? processMy;
        private void StartNotepad_Click(object sender, RoutedEventArgs e)
        {
            // запуск процесів 
            processMy = Process.Start("notepad.exe");
        }

        private void StopNotepad_Click(object sender, RoutedEventArgs e)
        {
            processMy?.CloseMainWindow();
            processMy?.Kill(true);
            processMy?.WaitForExitAsync();
            processMy?.Dispose();
            processMy = null;
        }
        

        private void StartEditNotepad_Click(object sender, RoutedEventArgs e)
        {
            String dir = AppContext.BaseDirectory;
            int binPosition = dir.LastIndexOf("bin");
            String projectRoot = dir[0..binPosition];
            //MessageBox.Show(projectRoot);
            processMy = Process.Start("notepad.exe",
                $"{projectRoot}ProcessWindow.xaml.cs"
                );
        }

        private void StartBrowserNotepad_Click(object sender, RoutedEventArgs e)
        {
            processMy = Process.Start("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\Microsoft Edge.lnk");
        }
        #endregion

        #region HomeWork
        private void StartProcess(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                processMy ??= Process.Start(button.Tag.ToString());
            }
        }
        private void CloseProcess(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var temp = Process.GetProcessesByName(button.Tag.ToString());
                foreach (var item in temp)
                {
                    item.Kill();
                }
                processMy = null;
            }
        }
        #endregion
    }
}

/* Процес - системний ресурс, контейнер для потоків.
 * Завдяки процесам досягається незалежність різних програм
 * від ресурсів (занятими іншими процесами). Цю задачу бере
 * на себе ОС, процеси у свою чергу працюють наче вони єдині на ПК.
 * 
 * "Диспетчер задач" - 
 * 1) додати відомостей про споживання процесорного часу та 
 *     оперативної пам'яті, а також про загальну кількість потоків
 * 2) забезпечити періодичне оновлення даних, але не повну 
 *     перебудову інтерфейсу, а саме оновлення - наявні змінюють
 *     відомості про рівень споживання, ті, що зникли - видаляються,
 *     ті, що додались - додаються.
 * 3)* реалізувати можливість сортування
 *     - за іменем
 *     - за споживання ресурсів (процесор/пам'ять)
 *     - за кількістю потоків/процесів
 */