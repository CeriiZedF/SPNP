using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
    /// Логика взаимодействия для DLLWindow.xaml
    /// </summary>
    public partial class DLLWindow : Window
    {
        [DllImport("User32.dll")]
        // Робота з DLL виглядає наступним чином: у С# оголошується
        // метод, який буде відповідати за виклик процедури з DLL
        public
            static   //статичне з'єднання - захист від GC (сборщика мусора)
            extern   // посилання на зовнішній модуль (некерований код)
            int MessageBoxA(
                IntPtr hWnd,    //IntPtr <-> HWND
                String lpText,   //String <-> LPCSTR
                String lpCaption,
                uint uType
            );

        public DLLWindow()
        {
            InitializeComponent();

        }

        private void AlertButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxA(
                IntPtr.Zero,
                "Повідомлення",
                "Заголовок",
                0x47);
        }
        //    HANDLE CreateThread(
        //  [in, optional] LPSECURITY_ATTRIBUTES       lpThreadAttributes,
        //  [in] SIZE_T                                 dwStackSize,
        //  [in] LPTHREAD_START_ROUTINE                lpStartAddress,
        //  [in, optional] __drv_aliasesMem LPVOID         lpParameter,
        //  [in] DWORD                                     dwCreationFlags,
        //  [out, optional] LPDWORD                        lpThreadId
        //);

        public delegate void ThreadMethod();
        // зазначення EntryPoint дозволяє використати іншу назву для методу
        [DllImport("Kernel32.dll", EntryPoint = "CreateThread")]
        public
            static
            extern
        IntPtr NewThread(
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            ThreadMethod lpStartAddress,    //замість IntPtr - делегат
            IntPtr lpParameter,
            uint dwCreationFlags,
            IntPtr lpThreadId);

        public void ErrorMessage()
        {
            MessageBoxA(
                IntPtr.Zero,
                "Повідомлення про помилку",
                "Місце виникнення",
                0x14);
            methodHandle.Free();
        }

        // GC може змінювати адреси об'єктів (та їх методів), а
        // У DLL передаються саме адреси (а не посилання), тому слід
        // "зафіксувати" об'єкт, на який буде посилатись DLL
        
        GCHandle methodHandle; // "фіксатор"
        private void ThreadButton_Click(object sender, RoutedEventArgs e)
        {
            // створюємо новий об'єкт з методом ErrorMessage
            var method = new ThreadMethod(ErrorMessage);
            // фіксуємо його у пам'яті та зберігаємо дескриптор (handle) 
            methodHandle = GCHandle.Alloc(method);
            // передаємо посилання на цейй об'єкт у некерований код
            NewThread(IntPtr.Zero, 0, method, IntPtr.Zero, 0, IntPtr.Zero);
        }

        [DllImport("Kernel32", EntryPoint = "Beep")]
        public
            static
            extern
            bool
        Sound(uint frequency, uint duration);

        private async void SoundButton_Click(object sender, RoutedEventArgs e)
        {
            
            await SoundAsync(1000, 1000, "Done1");

        }

        private async void SoundButton1_Click(object sender, RoutedEventArgs e)
        {
            await SoundAsync(10300, 3000, "Done2");
        }

        private async void SoundButton2_Click(object sender, RoutedEventArgs e)
        {
            await SoundAsync(14000, 1000, "Done3");
        }

        private async void SoundButton3_Click(object sender, RoutedEventArgs e)
        {
            await SoundAsync(14500, 1000, "Done4");
        }

        private async void SoundButton4_Click(object sender, RoutedEventArgs e)
        {
            await SoundAsync(11200, 200, "Done5");
        }


        private async Task<bool> SoundAsync(uint a, uint b, String message)
        {
            await Task.Run(() => Sound(a, b));
            
            return true;
        }
    }

    public class TwoNumber
    {
        public uint a, b;
    }

}