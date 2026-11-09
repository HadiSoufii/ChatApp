using ChatpApp.Protocol;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace ChatpApp.Client
{
    public sealed partial class Login : Form
    {
        private readonly List<Button> _avatarButtons = new List<Button>();
        private string _avatar = "👩🏻‍💻";

        // سوکت و پیام خوش‌آمد بعد از ورود به فرم گفتگو تحویل داده می‌شوند؛ فرم ورود بسته خواهد شد.
        public TcpClient ConnectedClient { get; private set; }
        public ChatPacket WelcomePacket { get; private set; }
        public string UserName { get { return _name.Text.Trim(); } }
        public string Avatar { get { return _avatar; } }
        public string HostName { get { return _host.Text.Trim(); } }
        public int PortNumber { get { int value; return int.TryParse(_port.Text, out value) ? value : 5050; } }

        public Login()
        {
            // کنترل‌ها در فایل Designer تعریف شده‌اند تا در زمان توسعه نیز قابل مشاهده و ویرایش باشند.
            InitializeComponent();
            _avatarButtons.AddRange(new[] { _avatar1, _avatar2, _avatar3, _avatar4, _avatar5, _avatar6 });
            foreach (Button button in _avatarButtons) button.Click += Avatar_Click;
            _connect.Click += Connect_Click;
            _theme.Click += delegate { ThemeSettings.Dark = !ThemeSettings.Dark; ApplyTheme(); };
            ApplyTheme();
            AcceptButton = _connect;
            SelectAvatar(_avatar);
            FormClosing += delegate { if (DialogResult != DialogResult.OK && ConnectedClient != null) ConnectedClient.Close(); };
        }

        #region رویدادهای فرم ورود
        private void Avatar_Click(object sender, EventArgs e)
        { var button = sender as Button; if (button != null) SelectAvatar(Convert.ToString(button.Tag)); }

        private void SelectAvatar(string icon)
        {
            _avatar = icon;
            foreach (Button b in _avatarButtons)
            {
                bool selected = Convert.ToString(b.Tag) == icon;
                b.FlatAppearance.BorderSize = selected ? 2 : 1;
                b.FlatAppearance.BorderColor = selected ? Color.FromArgb(59, 130, 246) : Color.FromArgb(90, 105, 125);
                b.BackColor = selected ? Color.FromArgb(45, 65, 95) : Color.Transparent;
            }
        }
        #endregion

        #region اتصال و ورود
        private void Connect_Click(object sender, EventArgs e)
        {
            string name = _name.Text.Trim(); string host = _host.Text.Trim(); int port;
            if (name.Length < 2 || name.Length > 24)
            { MessageBox.Show("نام نمایشی باید بین ۲ تا ۲۴ نویسه باشد.", "نام نامعتبر", MessageBoxButtons.OK, MessageBoxIcon.Warning); _name.Focus(); return; }
            if (host.Length == 0 || !int.TryParse(_port.Text.Trim(), out port) || port < 1 || port > 65535)
            { MessageBox.Show("آدرس سرور یا شماره پورت معتبر نیست.", "مشخصات اتصال", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            _connect.Enabled = false; _status.Text = "در حال اتصال به سرور..."; Cursor = Cursors.WaitCursor;
            TcpClient client = null;
            try
            {
                // ورود فقط پس از دریافت پاسخ Welcome از سرور موفق اعلام می‌شود.
                client = new TcpClient(); client.ReceiveTimeout = 7000; client.SendTimeout = 7000;
                client.Connect(host, port); NetworkStream stream = client.GetStream(); stream.ReadTimeout = 7000; stream.WriteTimeout = 7000;
                FrameProtocol.Send(stream, new ChatPacket { Type = "Join", UserName = name, Avatar = _avatar }, new object());
                ChatPacket response = FrameProtocol.Receive(stream);
                if (response == null) throw new InvalidOperationException("سرور اتصال را بست.");
                if (response.Type == "Error") throw new InvalidOperationException(response.Text);
                if (response.Type != "Welcome") throw new InvalidOperationException("پاسخ سرور قابل شناسایی نیست.");
                stream.ReadTimeout = Timeout.Infinite; stream.WriteTimeout = Timeout.Infinite; client.ReceiveTimeout = 0; client.SendTimeout = 0;
                ConnectedClient = client; WelcomePacket = response; DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex)
            {
                if (client != null) try { client.Close(); } catch { }
                _status.Text = "اتصال برقرار نشد؛ آدرس، پورت و وضعیت سرور را بررسی کن.";
                MessageBox.Show("اتصال به سرور برقرار نشد:\n" + ex.Message, "خطای اتصال", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { Cursor = Cursors.Default; _connect.Enabled = true; }
        }
        #endregion

        private void ApplyTheme()
        {
            bool dark = ThemeSettings.Dark;
            Color back = dark ? Color.FromArgb(10, 16, 29) : Color.FromArgb(239, 244, 250);
            Color card = dark ? Color.FromArgb(20, 30, 48) : Color.White;
            Color fore = dark ? Color.FromArgb(235, 241, 250) : Color.FromArgb(30, 41, 59);
            BackColor = back; ForeColor = fore;
            _rootPanel.BackColor = back;
            _topBar.BackColor = back;
            if (_card != null) { _card.BackColor = card; _card.ForeColor = fore; }
            // فقط کارت اصلی به‌صورت بازگشتی رنگ می‌شود تا حاشیه بیرونی تم جداگانه خود را حفظ کند.
            ThemeChild(_card, card, fore, dark);
            _smallBrandLabel.ForeColor = dark ? Color.FromArgb(147, 197, 253) : Color.FromArgb(37, 99, 235);
            if (_theme != null) { _theme.BackColor = Color.FromArgb(37, 99, 235); _theme.ForeColor = Color.White; }
            SelectAvatar(_avatar);
        }
        private void ThemeChild(Control parent, Color card, Color fore, bool dark)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBox) { c.BackColor = dark ? Color.FromArgb(12, 21, 36) : Color.FromArgb(248, 250, 252); c.ForeColor = fore; }
                else if (c is Button) { c.BackColor = Color.FromArgb(37, 99, 235); c.ForeColor = Color.White; }
                else { c.BackColor = card; c.ForeColor = fore; }
                ThemeChild(c, card, fore, dark);
            }
        }
    }
}
