using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
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
        public ProcessWindow()
        {
            InitializeComponent();
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
    }
}
