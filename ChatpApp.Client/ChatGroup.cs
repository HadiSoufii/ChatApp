using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using ChatpApp.Shared;
using WinTimer = System.Windows.Forms.Timer;

namespace ChatpApp.Client
{
    /// <summary>
    /// فرم اصلی چت (سمت کلاینت)
    /// یک ترید پس‌زمینه بسته‌های سرور را می‌خواند و روی ترید اصلی فرم اعمال می‌کند
    /// </summary>
    public partial class ChatGroup : Form
    {
        // ---------- شبکه و وضعیت ----------
        private readonly ChatConnection conn;
        private readonly string host;
        private readonly int port;
        private readonly int myAvatar;
        private string myName = "";
        private bool connected = true;
        private long tempCounter;                 // شمارنده شناسه موقت پیام‌های در حال ارسال
        private string welcomeText = "";
        private int welcomeOnline;
        private bool historyDivider;
        private bool soundOn = true;
        private int unread;

        /// <summary>
        /// اگر true باشد، بعد از بسته شدن این فرم، فرم ورود دوباره نمایش داده می‌شود (وگرنه برنامه بسته می‌شود)
        /// </summary>
        public bool ReturnToLogin;

        // کنترل‌های فرم در فایل ChatGroup.Designer.cs تعریف شده‌اند و در طراح فرم دیده می‌شوند
        private readonly WinTimer readTimer = new WinTimer();

        public ChatGroup(ChatConnection conn, Packet welcome, string host, int port, int avatar)
        {
            this.conn = conn;
            this.host = host;
            this.port = port;
            this.myAvatar = avatar;

            InitializeComponent();
            SetupBehavior();

            Theme.Changed += OnThemeChanged;
            ApplyTheme();

            // بسته خوشامدگویی (WELCOME) که هنگام ورود گرفته شده را همین‌جا پردازش می‌کنیم
            HandlePacket(welcome);

            // هر ۷۰۰ میلی‌ثانیه پیام‌های دیده شده را به سرور گزارش می‌دهیم (برای تیک سوم «خوانده شد»)
            readTimer.Interval = 700;
            readTimer.Tick += delegate { ReportRead(); };
            readTimer.Start();
        }

        // ==================================================================================
        // ساخت رابط کاربری
        // ==================================================================================

        // تنظیماتی که در طراح فرم ذخیره نمی‌شود: متن‌های وابسته به اتصال، آیکون دکمه‌ها و رویدادها
        private void SetupBehavior()
        {
            DoubleBuffered = true;

            lblSub.Text = "متصل به " + host + " : " + port;
            pill.SetState("متصل", Color.FromArgb(74, 222, 128), true);

            // دکمه صدا: روشن / خاموش کردن اعلان صوتی
            btnSound.IconPainter = Icons.SpeakerOn;
            btnSound.Click += delegate
            {
                soundOn = !soundOn;
                btnSound.IconPainter = soundOn ? (Action<Graphics, Rectangle, Color>)Icons.SpeakerOn : Icons.SpeakerOff;
                btnSound.Invalidate();
            };

            // دکمه خروج: بسته شدن چت و بازگشت به فرم ورود
            btnReturn.IconPainter = Icons.Back;
            btnReturn.Click += delegate { ReturnToLogin = true; Close(); };

            // رویدادهای پنل چت
            chat.SendRequested += OnSend;
            chat.EditSubmitted += delegate(long id, string text) { SendPacket(new Packet("EDIT", id.ToString(CultureInfo.InvariantCulture), text)); };
            chat.DeleteRequested += delegate(ChatMessageInfo m) { SendPacket(new Packet("DEL", m.Id.ToString(CultureInfo.InvariantCulture))); };
            chat.ReactRequested += delegate(ChatMessageInfo m, string emoji) { SendPacket(new Packet("REACT", m.Id.ToString(CultureInfo.InvariantCulture), emoji)); };
            chat.TypingNotify += delegate { SendPacket(new Packet("TYPING")); };
        }

        private void ApplyTheme()
        {
            BackColor = Theme.Bg;
            Invalidate(true);
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        // ==================================================================================
        // شبکه
        // ==================================================================================

        private void RunOnUi(Action action)
        {
            try
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(action);
            }
            catch { }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // شروع ترید دریافت بسته‌ها از سرور
            Thread t = new Thread(ReceiveLoop);
            t.IsBackground = true;
            t.Start();
            chat.FocusInput();
        }

        // حلقه دریافت: هر بسته کامل را روی ترید اصلی فرم پردازش می‌کند
        private void ReceiveLoop()
        {
            try
            {
                while (true)
                {
                    Packet p = conn.ReadPacket();
                    if (p == null) break;
                    Packet pp = p;
                    RunOnUi(() => HandlePacket(pp));
                }
            }
            catch { }

            RunOnUi(() => Disconnected("ارتباط با سرور قطع شد."));
        }

        // ارسال یک بسته؛ اگر ارسال نشد یعنی اتصال قطع شده است
        private void SendPacket(Packet p)
        {
            if (!connected) return;
            if (!conn.Send(p)) Disconnected("ارتباط با سرور قطع شد.");
        }

        // ارسال پیام جدید: اول با یک تیک و شناسه موقت نمایش داده می‌شود و با رسیدن ACK سرور دو تیک می‌شود
        private void OnSend(string text, long replyId)
        {
            if (!connected) return;

            ChatMessageInfo m = new ChatMessageInfo();
            m.Id = -(++tempCounter);
            m.Sender = myName;
            m.Avatar = myAvatar;
            m.Time = DateTime.Now;
            m.Text = text;
            m.ReplyId = replyId;
            m.Status = 0;

            if (replyId != 0)
            {
                ChatMessageInfo orig = chat.View.Find(replyId);
                if (orig != null)
                {
                    m.ReplyName = orig.Sender;
                    m.ReplyText = ChatText.Snippet(orig.Text, 80);
                }
            }

            chat.View.AddMessage(m, true);
            SendPacket(new Packet("MSG", m.Id.ToString(CultureInfo.InvariantCulture), text, replyId.ToString(CultureInfo.InvariantCulture)));
        }

        // گزارش «خوانده شد» برای پیام‌های دیده شده (فقط وقتی پنجره فعال است)
        private void ReportRead()
        {
            if (!connected || Form.ActiveForm != this) return;
            foreach (ChatMessageInfo m in chat.View.TakeVisibleUnread())
            {
                conn.Send(new Packet("READ", m.Id.ToString(CultureInfo.InvariantCulture)));
            }
        }

        /// <summary>
        /// پردازش یک بسته دریافتی از سرور (روی ترید اصلی فرم)
        /// </summary>
        private void HandlePacket(Packet p)
        {
            switch (p.Type)
            {
                case "WELCOME":
                    myName = p.Arg(0);
                    welcomeText = p.Arg(1);
                    welcomeOnline = p.ArgInt(2);
                    chat.MyName = myName;
                    users.MyName = myName;
                    UpdateTitle();
                    break;

                case "USERS":
                    UpdateUsers(p);
                    break;

                case "MSG":
                    OnMessage(p);
                    break;

                case "HISTEND":
                    // بعد از پیام‌های قبلی، کارت خوشامدگویی نشان داده می‌شود
                    chat.View.AddWelcome(myName, myAvatar, welcomeText, welcomeOnline);
                    break;

                case "ACK":
                    chat.View.ReplaceId(p.ArgLong(0), p.ArgLong(1));
                    chat.View.SetStatus(p.ArgLong(1), 1);
                    break;

                case "EDIT":
                    chat.View.SetEdited(p.ArgLong(0), p.Arg(1));
                    break;

                case "DEL":
                    chat.View.MarkDeleted(p.ArgLong(0));
                    break;

                case "REACT":
                    chat.View.SetReactions(p.ArgLong(0), ChatText.DecodeReactions(p.Arg(1)));
                    break;

                case "READ":
                    if (p.ArgInt(1) > 0) chat.View.SetStatus(p.ArgLong(0), 2);
                    break;

                case "TYPING":
                    if (p.Arg(0) != myName) chat.AddTyper(p.Arg(0));
                    break;

                case "PINS":
                    UpdatePins(p);
                    break;

                case "SYS":
                    chat.View.AddSystem(p.Arg(0), ChatText.TicksToLocal(p.ArgLong(1)), p.ArgInt(2));
                    break;

                case "KICK":
                    Disconnected("شما توسط سرور از چت خارج شدید.");
                    break;

                case "BYE":
                    Disconnected("سرور متوقف شد و ارتباط قطع گردید.");
                    break;

                case "ERROR":
                    chat.View.AddSystem(p.Arg(0), DateTime.Now, 2);
                    break;
            }
        }

        // دریافت پیام: پیام‌های قبلی (History) بی‌صدا اضافه می‌شوند و پیام‌های تازه صدا و چشمک دارند
        private void OnMessage(Packet p)
        {
            ChatMessageInfo m = ChatText.FromPacket(p);
            bool history = p.Arg(11) == "1";

            if (chat.View.Find(m.Id) != null) return;

            if (history)
            {
                if (!historyDivider)
                {
                    chat.View.AddDivider("پیام‌های قبلی");
                    historyDivider = true;
                }
                m.ReadReported = true;
                m.Status = 1;
                chat.View.AddMessage(m, false);
                return;
            }

            chat.View.AddMessage(m, true);
            chat.RemoveTyper(m.Sender);
            if (m.Sender != myName) Notify();
        }

        // اعلان پیام جدید: صدا + (اگر پنجره فعال نیست) چشمک آیکون نوار وظیفه و شمارنده در عنوان
        private void Notify()
        {
            if (soundOn)
            {
                try { SystemSounds.Asterisk.Play(); } catch { }
            }

            if (Form.ActiveForm != this)
            {
                unread++;
                UpdateTitle();
                NativeHelper.FlashTaskbar(this);
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            unread = 0;
            UpdateTitle();
            NativeHelper.StopFlash(this);
            ReportRead();
        }

        private void UpdateTitle()
        {
            string prefix = unread > 0 ? "(" + Fa.Digits(unread) + ") " : "";
            Text = prefix + "چت‌روم - " + myName;
        }

        // به‌روزرسانی لیست کاربران آنلاین
        private void UpdateUsers(Packet p)
        {
            List<UserRow> rows = new List<UserRow>();
            for (int i = 0; i + 1 < p.Args.Length; i += 2)
            {
                UserRow u = new UserRow();
                u.Name = p.Args[i];
                int av;
                int.TryParse(p.Args[i + 1], out av);
                u.Avatar = av;
                rows.Add(u);
            }
            users.SetUsers(rows);
            lblUsers.Text = "کاربران آنلاین (" + Fa.Digits(rows.Count) + ")";
            welcomeOnline = rows.Count;
        }

        // به‌روزرسانی پیام‌های سنجاق شده: PINS|تعداد|(شناسه|فرستنده|متن)...
        private void UpdatePins(Packet p)
        {
            List<PinInfo> pins = new List<PinInfo>();
            int n = p.ArgInt(0);
            for (int i = 0; i < n; i++)
            {
                PinInfo pi = new PinInfo();
                pi.Id = p.ArgLong(1 + i * 3);
                pi.Sender = p.Arg(2 + i * 3);
                pi.Text = p.Arg(3 + i * 3);
                pins.Add(pi);
            }
            chat.SetPins(pins);
        }

        // قطع شدن ارتباط (توسط سرور، خطای شبکه یا kick)
        private void Disconnected(string reason)
        {
            if (!connected) return;
            connected = false;
            readTimer.Stop();

            pill.SetState("قطع شده", Color.FromArgb(248, 113, 113), false);
            chat.View.AddSystem(reason, DateTime.Now, 2);
            chat.SetInputEnabled(false, "اتصال قطع شده است");
            users.SetUsers(new List<UserRow>());
            lblUsers.Text = "کاربران آنلاین (۰)";
            btnReturn.Text = "ورود مجدد";
            conn.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (connected)
            {
                // اعلام خروج مرتب به سرور
                conn.Send(new Packet("BYE"));
                connected = false;
                conn.Close();
            }
            readTimer.Stop();
            Theme.Changed -= OnThemeChanged;
            base.OnFormClosing(e);
        }
    }
}
