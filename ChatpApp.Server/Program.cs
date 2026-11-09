using System;
using System.Windows.Forms;

namespace ChatpApp.Server
{
    internal static class Program
    {
        // اجرای پنل مدیریت؛ توقف سوکت از داخل فرم انجام می‌شود و برنامه بسته نمی‌شود.
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ChatServer());
        }
    }
}
