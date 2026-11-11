using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatpApp.Shared;
using WinTimer = System.Windows.Forms.Timer;

namespace ChatpApp.Client
{
    /// <summary>
    /// فرم ورود: انتخاب آواتار، نام کاربری، آدرس و پورت سرور
    /// بعد از ورود موفق، این فرم مخفی می‌شود و فرم چت باز می‌شود
    /// </summary>
    public partial class Login : Form
    {
        // کنترل‌های فرم در فایل Login.Designer.cs تعریف شده‌اند و در طراح فرم دیده می‌شوند
        private bool connecting;

        public Login()
        {
            InitializeComponent();
            SetupBehavior();
            Theme.Changed += OnThemeChanged;
            ApplyTheme();
        }

        // تنظیمات رفتاری که در طراح فرم قابل ذخیره نیست: آیکون دکمه، رویدادها و محدودیت‌ها
        private void SetupBehavior()
        {
            DoubleBuffered = true;

            btnLogin.IconPainter = Icons.Send;
            btnLogin.Click += BtnLogin_Click;
            txtName.Inner.MaxLength = 20;

            // با زدن Enter در هر کادر، ورود انجام می‌شود
            txtName.Inner.KeyDown += InputKeyDown;
            txtIp.Inner.KeyDown += InputKeyDown;
            txtPort.Inner.KeyDown += InputKeyDown;
        }

        private void ApplyTheme()
        {
            BackColor = Theme.Bg;
            formArea.BackColor = Theme.Bg;
            Invalidate(true);
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                BtnLogin_Click(this, EventArgs.Empty);
            }
        }

        private void ShowError(string text)
        {
            lblError.Text = text;
        }

        // بعد از بازگشت از چت، فرم دوباره آماده ورود می‌شود
        private void ResetAfterReturn()
        {
            connecting = false;
            btnLogin.Busy = false;
            btnLogin.Enabled = true;
            btnLogin.Text = "ورود به چت";
            ShowError("");
        }

        /// <summary>
        /// دکمه ورود: اعتبارسنجی ورودی‌ها، اتصال به سرور در پس‌زمینه (بدون قفل شدن فرم) و باز کردن چت
        /// </summary>
        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            if (connecting) return;

            string host = txtIp.Text.Trim();
            string name = ChatText.CleanName(txtName.Text);
            int port;

            if (txtName.Text.Trim().Length == 0) { ShowError("لطفا یک نام کاربری وارد کن."); txtName.FocusInner(); return; }
            if (host.Length == 0) { ShowError("آدرس سرور را وارد کن."); txtIp.FocusInner(); return; }
            if (!int.TryParse(txtPort.Text.Trim(), out port) || port < 1 || port > 65535) { ShowError("پورت باید عددی بین ۱ تا ۶۵۵۳۵ باشد."); txtPort.FocusInner(); return; }

            int avatar = picker.Selected;

            connecting = true;
            ShowError("");
            btnLogin.Busy = true;
            btnLogin.Enabled = false;
            btnLogin.Text = "در حال اتصال...";

            ConnectResult res = await Task.Run(() => TryConnect(host, port, name, avatar));

            connecting = false;
            btnLogin.Busy = false;
            btnLogin.Enabled = true;
            btnLogin.Text = "ورود به چت";

            if (res.Error != null)
            {
                ShowError(res.Error);
                return;
            }

            // اتصال موفق: فرم لاگین مخفی می‌شود و فرم چت باز می‌شود
            ChatGroup chat = new ChatGroup(res.Conn, res.Welcome, host, port, avatar);
            chat.FormClosed += delegate
            {
                if (chat.ReturnToLogin)
                {
                    ResetAfterReturn();
                    Show();
                    Activate();
                }
                else
                {
                    Close();
                }
            };
            Hide();
            chat.Show();
        }

        /// <summary>
        /// اتصال به سرور و ارسال بسته HELLO و انتظار برای پاسخ (در یک ترید پس‌زمینه اجرا می‌شود)
        /// </summary>
        private ConnectResult TryConnect(string host, int port, string name, int avatar)
        {
            ConnectResult res = new ConnectResult();
            Socket s = null;
            try
            {
                IPAddress addr;
                if (!IPAddress.TryParse(host, out addr))
                {
                    // اگر نام میزبان (مثلا localhost) وارد شده باشد آدرس آن پیدا می‌شود
                    addr = null;
                    foreach (IPAddress a in Dns.GetHostAddresses(host))
                    {
                        if (a.AddressFamily == AddressFamily.InterNetwork) { addr = a; break; }
                    }
                    if (addr == null)
                    {
                        res.Error = "آدرس سرور معتبر نیست.";
                        return res;
                    }
                }

                s = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // اتصال با مهلت ۴ ثانیه
                IAsyncResult ar = s.BeginConnect(addr, port, null, null);
                if (!ar.AsyncWaitHandle.WaitOne(4000))
                {
                    s.Close();
                    res.Error = "زمان اتصال به سرور تمام شد. آدرس و پورت را بررسی کن.";
                    return res;
                }
                s.EndConnect(ar);
                s.NoDelay = true;

                ChatConnection conn = new ChatConnection(s);
                conn.Send(new Packet("HELLO", name, avatar.ToString()));

                // انتظار برای پاسخ سرور (حداکثر ۶ ثانیه)
                s.ReceiveTimeout = 6000;
                Packet first = conn.ReadPacket();
                s.ReceiveTimeout = 0;

                if (first == null)
                {
                    res.Error = "سرور اتصال را بست.";
                    conn.Close();
                    return res;
                }
                if (first.Type == "ERROR")
                {
                    res.Error = first.Arg(0);
                    conn.Close();
                    return res;
                }
                if (first.Type != "WELCOME")
                {
                    res.Error = "پاسخ نامعتبر از سرور دریافت شد.";
                    conn.Close();
                    return res;
                }

                res.Conn = conn;
                res.Welcome = first;
                return res;
            }
            catch (Exception)
            {
                try { if (s != null) s.Close(); } catch { }
                res.Error = "اتصال به سرور برقرار نشد. مطمئن شو سرور روشن است و آدرس و پورت درست است.";
                return res;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Theme.Changed -= OnThemeChanged;
            base.OnFormClosed(e);
        }
    }

    // نکته مهم: کلاس Login باید اولین کلاس این فایل باشد، چون MSBuild نام فایل ریسورس (Login.resx) را از اولین کلاس فایل می‌سازد
    // اگر کلاس دیگری قبل از Login بیاید، ارور MissingManifestResourceException پیش می‌آید
    /// <summary>
    /// نتیجه تلاش برای اتصال به سرور
    /// </summary>
    internal class ConnectResult
    {
        public string Error;            // اگر null نباشد یعنی اتصال ناموفق بوده
        public ChatConnection Conn;     // اتصال برقرار شده
        public Packet Welcome;          // بسته خوشامدگویی سرور
    }

    /// <summary>
    /// سمت چپ فرم ورود: پس‌زمینه گرادیان با حباب‌های شناور، لوگوی متحرک چت و معرفی امکانات
    /// </summary>
    public class LoginArt : Control
    {
        private class Bubble
        {
            public float X, Y, R, Speed, Phase;
            public int Alpha;
        }

        private readonly List<Bubble> bubbles = new List<Bubble>();
        private readonly WinTimer timer = new WinTimer();
        private float time;

        public LoginArt()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            // ساخت حباب‌های تصادفی
            Random rnd = new Random();
            for (int i = 0; i < 16; i++)
            {
                Bubble b = new Bubble();
                b.X = (float)rnd.NextDouble() * 420;
                b.Y = (float)rnd.NextDouble() * 640;
                b.R = 8 + (float)rnd.NextDouble() * 38;
                b.Speed = 0.25f + (float)rnd.NextDouble() * 0.8f;
                b.Phase = (float)rnd.NextDouble() * 6.28f;
                b.Alpha = 14 + rnd.Next(26);
                bubbles.Add(b);
            }

            timer.Interval = 30;
            timer.Tick += OnTick;
            timer.Start();
            Theme.Changed += OnThemeChanged;
        }

        private void OnThemeChanged(object s, EventArgs e) { Invalidate(); }

        private void OnTick(object s, EventArgs e)
        {
            time += 0.05f;
            foreach (Bubble b in bubbles)
            {
                b.Y -= b.Speed;
                b.X += (float)Math.Sin(time + b.Phase) * 0.35f;
                if (b.Y + b.R < 0)
                {
                    b.Y = Height + b.R;
                    b.X = (float)new Random(Environment.TickCount + (int)b.R).NextDouble() * Width;
                }
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (Width < 10 || Height < 10) return;

            // پس‌زمینه گرادیان سه‌رنگ
            Color c1 = Color.FromArgb(67, 56, 202), c2 = Color.FromArgb(124, 58, 237), c3 = Color.FromArgb(219, 39, 119);
            if (Theme.IsDark)
            {
                c1 = Theme.Mix(c1, Color.Black, 0.35f);
                c2 = Theme.Mix(c2, Color.Black, 0.35f);
                c3 = Theme.Mix(c3, Color.Black, 0.35f);
            }
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            using (LinearGradientBrush bg = new LinearGradientBrush(rect, c1, c3, 65f))
            {
                ColorBlend cb = new ColorBlend(3);
                cb.Colors = new Color[] { c1, c2, c3 };
                cb.Positions = new float[] { 0f, 0.55f, 1f };
                bg.InterpolationColors = cb;
                g.FillRectangle(bg, rect);
            }

            // حباب‌های شناور
            foreach (Bubble b in bubbles)
            {
                using (Brush br = new SolidBrush(Color.FromArgb(b.Alpha, 255, 255, 255)))
                {
                    g.FillEllipse(br, b.X - b.R, b.Y - b.R, b.R * 2, b.R * 2);
                }
            }

            DrawLogo(g);

            // عنوان و شعار
            Rectangle title = new Rectangle(20, 318, Width - 40, 50);
            TextRenderer.DrawText(g, "چت‌روم", Theme.Font(30, FontStyle.Bold), title, Color.White, UiUtil.Flags(true, false, true, true, false));
            Rectangle tag = new Rectangle(20, 370, Width - 40, 28);
            TextRenderer.DrawText(g, "ارتباط لحظه‌ای، ساده و زیبا", Theme.Font(12), tag, Color.FromArgb(225, 255, 255, 255), UiUtil.Flags(true, false, true, true, false));

            // امکانات
            string[] feats = { "گفتگوی همزمان با چند کاربر", "پاسخ، ویرایش و واکنش به پیام‌ها", "تم روشن و تیره، آواتار دلخواه" };
            int y = 450;
            foreach (string f in feats)
            {
                Rectangle row = new Rectangle(50, y, Width - 100, 30);
                Rectangle circle = new Rectangle(row.Right - 24, y + 3, 24, 24);
                using (Brush cb = new SolidBrush(Color.FromArgb(50, 255, 255, 255))) g.FillEllipse(cb, circle);
                Icons.Check(g, new Rectangle(circle.X + 3, circle.Y + 3, 18, 18), Color.White);
                TextRenderer.DrawText(g, f, Theme.Font(10.5f), new Rectangle(row.X, y, row.Width - 34, 30), Color.White, UiUtil.Flags(true, false, false, true, true));
                y += 40;
            }
        }

        // لوگوی متحرک: دو حباب گفتگو و سه نقطه‌ای که بالا و پایین می‌پرند
        private void DrawLogo(Graphics g)
        {
            float cx = Width / 2f, cy = 190f;
            float bob = (float)Math.Sin(time * 0.9f) * 5f;

            // حباب پشتی
            RectangleF back = new RectangleF(cx + 10, cy + 6 + bob * 0.5f, 104, 76);
            using (GraphicsPath p = UiUtil.RoundRect(back, 26))
            using (Brush b = new SolidBrush(Color.FromArgb(110, 255, 255, 255)))
            {
                g.FillPath(b, p);
                g.FillPolygon(b, new PointF[] { new PointF(back.Right - 24, back.Bottom - 4), new PointF(back.Right - 8, back.Bottom + 22), new PointF(back.Right - 44, back.Bottom - 4) });
            }

            // حباب اصلی
            RectangleF main = new RectangleF(cx - 90, cy - 62 + bob, 160, 114);
            using (GraphicsPath p = UiUtil.RoundRect(main, 32))
            using (Brush b = new SolidBrush(Color.FromArgb(244, 255, 255, 255)))
            {
                g.FillPath(b, p);
                g.FillPolygon(b, new PointF[] { new PointF(main.X + 30, main.Bottom - 6), new PointF(main.X + 10, main.Bottom + 28), new PointF(main.X + 66, main.Bottom - 6) });
            }

            // سه نقطه
            for (int i = 0; i < 3; i++)
            {
                float dy = (float)Math.Sin(time * 2.2f + i * 0.8f) * 7f;
                using (Brush b = new SolidBrush(Color.FromArgb(79, 70, 229)))
                {
                    g.FillEllipse(b, main.X + 80 - 9 + (i - 1) * 38, main.Y + 57 - 9 + dy, 18, 18);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Theme.Changed -= OnThemeChanged;
                timer.Stop();
                timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
