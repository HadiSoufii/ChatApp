using System;
using System.Windows.Forms;

namespace ChatpApp.Client
{
    internal static class Program
    {
        // فرم ورود فقط تا زمان احراز اتصال باز است؛ پس از ورود Dispose شده و دیگر در برنامه وجود ندارد.
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var login = new Login();
            if (login.ShowDialog() != DialogResult.OK || login.ConnectedClient == null)
            { login.Dispose(); return; }

            string userName = login.UserName;
            string avatar = login.Avatar;
            string host = login.HostName;
            int port = login.PortNumber;
            var client = login.ConnectedClient;
            var welcome = login.WelcomePacket;
            login.Dispose();

            // فرم گفتگو بعد از نابودی فرم ورود اجرا می‌شود و همان سوکت تأییدشده را تحویل می‌گیرد.
            Application.Run(new ChatGroup(userName, avatar, host, port, client, welcome));
        }
    }
}
