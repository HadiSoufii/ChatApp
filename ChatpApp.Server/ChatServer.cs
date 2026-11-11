using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using ChatpApp.Shared;
using WinTimer = System.Windows.Forms.Timer;

namespace ChatpApp.Server
{
    /// <summary>
    /// فرم اصلی سرور چت
    /// سرور برای هر کلاینت یک ترید جدا می‌سازد (پس چند کاربر همزمان پشتیبانی می‌شوند)
    /// تاریخچه پیام‌ها، واکنش‌ها، پیام‌های سنجاق شده و وضعیت خوانده شدن پیام‌ها در سرور نگهداری می‌شود
    /// </summary>
    public partial class ChatServer : Form
    {
        // ==================================================================================
        // مدل‌ها و وضعیت سرور
        // ==================================================================================

        /// <summary>
        /// اطلاعات یک کاربر متصل
        /// </summary>
        private class ClientSession
        {
            public string Name = "";
            public int Avatar;
            public ChatConnection Conn;
            public string Ip = "";
            public int Port;
            public DateTime ConnectedAt;
            public int Messages;
            public volatile bool Ready;      // بعد از ارسال خوشامد و تاریخچه true می‌شود (قبل از آن بسته‌های همگانی برایش ارسال نمی‌شود)
            public volatile bool Kicked;     // توسط سرور قطع شده؟
        }

        private Socket listener;
        private volatile bool running;
        private readonly object stateLock = new object();            // قفل همه داده‌های مشترک زیر
        private readonly List<ClientSession> sessions = new List<ClientSession>();
        private readonly List<ChatMessageInfo> history = new List<ChatMessageInfo>();
        private readonly Dictionary<long, ChatMessageInfo> byId = new Dictionary<long, ChatMessageInfo>();
        private readonly List<long> pins = new List<long>();        // شناسه پیام‌های سنجاق شده (جدیدترین اول)
        private long nextId;
        private int totalMessages;
        private DateTime startedAt;
        private volatile string welcomeText = "";

        // ==================================================================================
        // کنترل‌ها: در فایل ChatServer.Designer.cs تعریف شده‌اند و در طراح فرم دیده می‌شوند
        // ==================================================================================

        private readonly WinTimer statsTimer = new WinTimer();

        public ChatServer()
        {
            InitializeComponent();
            SetupBehavior();

            Theme.Changed += OnThemeChanged;
            ApplyTheme();

            SetRunningUi(false);
            ShowTab(true);
            ShowNetworkInfo();

            // هر ثانیه آمار هدر به‌روز می‌شود
            statsTimer.Interval = 1000;
            statsTimer.Tick += delegate { UpdateStats(); };
            statsTimer.Start();
            UpdateStats();
        }

        // ==================================================================================
        // تنظیمات رفتاری فرم (آنچه در طراح فرم ذخیره نمی‌شود)
        // ==================================================================================

        private void SetupBehavior()
        {
            DoubleBuffered = true;

            // حالت مدیر: سرور می‌تواند هر پیامی را حذف و سنجاق کند
            users.Detailed = true;
            users.KickClicked += KickUser;
            chat.MyName = ChatConst.ServerName;
            chat.AdminMode = true;

            // متن خوشامدگویی: هر تغییر در کادر، بلافاصله برای کاربران تازه‌وارد اعمال می‌شود
            txtWelcome.Inner.TextChanged += delegate { welcomeText = txtWelcome.Text.Trim(); };
            welcomeText = txtWelcome.Text.Trim();

            // دکمه‌ها
            btnStart.Click += delegate { StartServer(); };
            btnStop.Click += delegate { StopServer("سرور توسط مدیر متوقف شد."); };
            btnTabChat.Click += delegate { ShowTab(true); };
            btnTabLog.Click += delegate { ShowTab(false); };

            // رویدادهای پنل چت
            chat.SendRequested += ServerSend;
            chat.EditSubmitted += ServerEdit;
            chat.DeleteRequested += delegate(ChatMessageInfo m) { DeleteMessage(m.Id, ChatConst.ServerName, true); };
            chat.ReactRequested += delegate(ChatMessageInfo m, string emoji) { ToggleReaction(ChatConst.ServerName, m.Id, emoji); };
            chat.PinToggleRequested += delegate(ChatMessageInfo m) { TogglePin(m.Id); };
            chat.UnpinRequested += delegate(long id) { TogglePin(id); };
        }

        private void ApplyTheme()
        {
            BackColor = Theme.Bg;
            rtbLog.BackColor = Theme.Surface;
            rtbLog.ForeColor = Theme.Text;
            ShowTab(chat.Visible);
            Invalidate(true);
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        // نمایش تب چت یا لاگ؛ دکمه تب فعال رنگی و دیگری ساده است
        private void ShowTab(bool chatTab)
        {
            chat.Visible = chatTab;
            logCard.Visible = !chatTab;
            btnTabChat.Kind = chatTab ? BtnKind.Primary : BtnKind.Secondary;
            btnTabLog.Kind = chatTab ? BtnKind.Secondary : BtnKind.Primary;
            btnTabChat.Invalidate();
            btnTabLog.Invalidate();
        }

        // اطلاعات شبکه این دستگاه (آی‌پی‌های IPv4)
        private void ShowNetworkInfo()
        {
            try
            {
                List<string> ips = new List<string>();
                foreach (IPAddress a in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (a.AddressFamily == AddressFamily.InterNetwork) ips.Add(a.ToString());
                }
                string list = ips.Count > 0 ? string.Join("  ،  ", ips) : "یافت نشد";
                SetNotice("آی‌پی‌های شبکه این دستگاه:  " + list + "   —   برای اتصال از دستگاه‌های دیگر، آی‌پی سرور را 0.0.0.0 بگذارید.", false);
            }
            catch
            {
                SetNotice("اطلاعات شبکه این دستگاه در دسترس نیست.", false);
            }
        }

        // پیام راهنما / خطا زیر کارت تنظیمات
        private void SetNotice(string text, bool error)
        {
            lblNet.Role = error ? LabelRole.Danger : LabelRole.Muted;
            lblNet.Text = text;
        }

        // ==================================================================================
        // ابزارهای کمکی
        // ==================================================================================

        // اجرای کد روی ترید اصلی فرم (از هر ترید دلخواه امن است)
        private void Ui(Action action)
        {
            try
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(action);
            }
            catch { }
        }

        // نوشتن یک خط در لاگ شبکه (با ساعت و رنگ)
        private void Log(string text, Color color)
        {
            Ui(delegate
            {
                rtbLog.SelectionStart = rtbLog.TextLength;
                rtbLog.SelectionLength = 0;
                rtbLog.SelectionColor = Theme.Muted;
                rtbLog.AppendText("[" + Fa.Digits(DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)) + "]  ");
                rtbLog.SelectionColor = color;
                rtbLog.AppendText(text + "\n");
                rtbLog.SelectionStart = rtbLog.TextLength;
                rtbLog.ScrollToCaret();
            });
        }

        private static readonly Color LogGreen = Color.FromArgb(22, 163, 74);
        private static readonly Color LogRed = Color.FromArgb(220, 38, 38);
        private static readonly Color LogAmber = Color.FromArgb(217, 119, 6);
        private static readonly Color LogBlue = Color.FromArgb(37, 99, 235);

        // حالت ظاهری روشن / خاموش بودن سرور
        private void SetRunningUi(bool on)
        {
            btnStart.Enabled = !on;
            btnStop.Enabled = on;
            txtIp.Enabled = !on;
            txtPort.Enabled = !on;
            pill.SetState(on ? "آنلاین" : "آفلاین", on ? Color.FromArgb(74, 222, 128) : Color.FromArgb(248, 113, 113), on);
            chat.SetInputEnabled(on, "برای ارسال پیام ابتدا سرور را روشن کنید");
        }

        // آمار هدر
        private void UpdateStats()
        {
            int online;
            lock (stateLock) { online = sessions.Count; }
            chipOnline.Value = Fa.Digits(online);
            chipMessages.Value = Fa.Digits(totalMessages);
            chipUptime.Value = running ? Fa.Duration(DateTime.Now - startedAt) : "—";
        }

        // ==================================================================================
        // روشن و خاموش کردن سرور
        // ==================================================================================

        private void StartServer()
        {
            if (running) return;

            IPAddress ip;
            int portNumber;
            if (!IPAddress.TryParse(txtIp.Text.Trim(), out ip)) { SetNotice("آی‌پی وارد شده معتبر نیست.", true); return; }
            if (!int.TryParse(txtPort.Text.Trim(), out portNumber) || portNumber < 1 || portNumber > 65535) { SetNotice("پورت باید عددی بین ۱ تا ۶۵۵۳۵ باشد.", true); return; }

            Socket l = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                l.Bind(new IPEndPoint(ip, portNumber));
                l.Listen(100);
            }
            catch (Exception ex)
            {
                try { l.Close(); } catch { }
                SetNotice("راه‌اندازی سرور ناموفق بود: " + ex.Message, true);
                Log("راه‌اندازی سرور ناموفق بود: " + ex.Message, LogRed);
                return;
            }

            listener = l;
            running = true;
            startedAt = DateTime.Now;

            // ترید قبول کردن اتصال کلاینت‌ها
            Thread t = new Thread(() => AcceptLoop(l));
            t.IsBackground = true;
            t.Start();

            SetRunningUi(true);
            ShowNetworkInfo();
            Log("سرور روی " + ip + " : " + portNumber + " روشن شد و منتظر اتصال کاربران است.", LogGreen);
            chat.View.AddSystem("سرور روشن شد", DateTime.Now, 1);
        }

        /// <summary>
        /// توقف سرور: به همه کاربران اطلاع داده می‌شود و سوکت‌ها بسته می‌شوند
        /// (برنامه بسته نمی‌شود و می‌توان دوباره سرور را روشن کرد)
        /// </summary>
        private void StopServer(string reason)
        {
            if (!running) return;
            running = false;

            Socket l = listener;
            listener = null;
            try { if (l != null) l.Close(); } catch { }

            List<ClientSession> snapshot;
            lock (stateLock)
            {
                snapshot = new List<ClientSession>(sessions);
                sessions.Clear();
            }

            foreach (ClientSession s in snapshot)
            {
                s.Conn.Send(new Packet("BYE", "سرور متوقف شد."));
                s.Conn.Close();
            }

            users.SetUsers(new List<UserRow>());
            lblUsers.Text = "کاربران آنلاین (۰)";
            SetRunningUi(false);
            Log(reason + " (" + Fa.Digits(snapshot.Count) + " اتصال بسته شد)", LogAmber);
            chat.View.AddSystem("سرور متوقف شد", DateTime.Now, 2);
            chat.SetPins(new List<PinInfo>());
            ShowNetworkInfo();
        }

        // حلقه قبول کردن اتصال‌ها (در یک ترید پس‌زمینه)
        private void AcceptLoop(Socket l)
        {
            try
            {
                while (running)
                {
                    Socket c = l.Accept();
                    if (!running || l != listener)
                    {
                        try { c.Close(); } catch { }
                        break;
                    }

                    c.NoDelay = true;
                    c.SendTimeout = 4000;

                    // برای هر کلاینت یک ترید جدا
                    Thread t = new Thread(() => ClientLoop(c));
                    t.IsBackground = true;
                    t.Start();
                }
            }
            catch
            {
                // با بسته شدن سوکت شنونده، Accept خطا می‌دهد و ترید تمام می‌شود
            }
        }

        // ==================================================================================
        // مدیریت یک کلاینت
        // ==================================================================================

        private void ClientLoop(Socket c)
        {
            ChatConnection conn = new ChatConnection(c);
            ClientSession me = null;

            try
            {
                // اولین بسته باید HELLO باشد (حداکثر ۸ ثانیه صبر می‌کنیم)
                c.ReceiveTimeout = 8000;
                Packet hello = conn.ReadPacket();
                c.ReceiveTimeout = 0;
                if (hello == null || hello.Type != "HELLO")
                {
                    conn.Close();
                    return;
                }

                string name = ChatText.CleanName(hello.Arg(0));
                int avatar = hello.ArgInt(1);
                if (avatar < 0 || avatar >= ChatConst.AvatarCount) avatar = 0;
                IPEndPoint ep = (IPEndPoint)c.RemoteEndPoint;

                int onlineCount;
                lock (stateLock)
                {
                    if (!running)
                    {
                        conn.Send(new Packet("ERROR", "سرور در حال حاضر خاموش است."));
                        conn.Close();
                        return;
                    }

                    me = new ClientSession();
                    me.Name = UniqueName(name);
                    me.Avatar = avatar;
                    me.Conn = conn;
                    me.Ip = ep.Address.ToString();
                    me.Port = ep.Port;
                    me.ConnectedAt = DateTime.Now;
                    sessions.Add(me);
                    onlineCount = sessions.Count;
                }

                // 1) پیام خوشامدگویی
                conn.Send(new Packet("WELCOME", me.Name, welcomeText, onlineCount.ToString(CultureInfo.InvariantCulture), ChatConst.ServerName));

                // 2) پیام‌های سنجاق شده و تاریخچه (۴۰ پیام آخر)
                conn.Send(BuildPinsPacket());
                List<ChatMessageInfo> recent = new List<ChatMessageInfo>();
                lock (stateLock)
                {
                    int start = Math.Max(0, history.Count - 40);
                    for (int i = start; i < history.Count; i++)
                    {
                        if (!history[i].Deleted) recent.Add(history[i].Clone());
                    }
                }
                foreach (ChatMessageInfo m in recent) conn.Send(ChatText.ToPacket(m, true));
                conn.Send(new Packet("HISTEND"));

                // 3) از این لحظه کاربر آماده دریافت بسته‌های همگانی است
                me.Ready = true;
                Log(me.Name + " متصل شد (آی‌پی " + me.Ip + " ، پورت " + me.Port + ")", LogGreen);
                BroadcastSystem(me.Name + " وارد چت شد", 1, me);
                BroadcastUsers();
                RefreshUsersUi();

                // 4) حلقه دریافت بسته‌های کاربر
                while (running)
                {
                    Packet p = conn.ReadPacket();
                    if (p == null || p.Type == "BYE") break;
                    HandleClientPacket(me, p);
                }
            }
            catch
            {
                // قطع ناگهانی کاربر (بستن برنامه، قطع شبکه یا kick)
            }

            RemoveSession(me, conn);
        }

        // حذف کاربر بعد از قطع اتصال
        private void RemoveSession(ClientSession me, ChatConnection conn)
        {
            conn.Close();
            if (me == null) return;

            bool removed;
            lock (stateLock) { removed = sessions.Remove(me); }
            if (!removed) return;      // قبلا هنگام توقف سرور حذف شده

            Log(me.Name + (me.Kicked ? " توسط مدیر قطع شد." : " قطع اتصال کرد."), me.Kicked ? LogAmber : LogRed);
            if (me.Ready)
            {
                BroadcastSystem(me.Name + (me.Kicked ? " توسط سرور از چت خارج شد" : " از چت خارج شد"), 2, null);
                BroadcastUsers();
            }
            Ui(() => chat.RemoveTyper(me.Name));
            RefreshUsersUi();
        }

        // نام یکتا (در صورت تکراری بودن یک عدد اضافه می‌شود). فقط داخل lock صدا زده شود
        private string UniqueName(string name)
        {
            string result = name;
            int n = 2;
            while (NameExists(result))
            {
                result = name + "(" + n + ")";
                n++;
            }
            return result;
        }

        private bool NameExists(string name)
        {
            if (string.Equals(name, ChatConst.ServerName, StringComparison.OrdinalIgnoreCase)) return true;
            foreach (ClientSession s in sessions)
            {
                if (string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        // پردازش بسته‌های کاربر
        private void HandleClientPacket(ClientSession me, Packet p)
        {
            switch (p.Type)
            {
                case "MSG":
                    OnClientMessage(me, p);
                    break;

                case "EDIT":
                    EditMessage(p.ArgLong(0), me.Name, ChatText.LimitText(p.Arg(1)));
                    break;

                case "DEL":
                    DeleteMessage(p.ArgLong(0), me.Name, false);
                    break;

                case "REACT":
                    ToggleReaction(me.Name, p.ArgLong(0), p.Arg(1));
                    break;

                case "READ":
                    OnRead(me, p.ArgLong(0));
                    break;

                case "TYPING":
                    SendToAll(new Packet("TYPING", me.Name), me);
                    Ui(() => chat.AddTyper(me.Name));
                    break;
            }
        }

        // ==================================================================================
        // پیام‌ها
        // ==================================================================================

        // ساخت پیام جدید و ثبت در تاریخچه. فقط داخل lock صدا زده شود
        private ChatMessageInfo NewMessage(string sender, int avatar, string kind, string text, long replyId)
        {
            ChatMessageInfo m = new ChatMessageInfo();
            m.Id = ++nextId;
            m.Sender = sender;
            m.Avatar = avatar;
            m.Kind = kind;
            m.Time = DateTime.Now;
            m.Text = text;

            ChatMessageInfo orig;
            if (replyId != 0 && byId.TryGetValue(replyId, out orig) && !orig.Deleted)
            {
                m.ReplyId = replyId;
                m.ReplyName = orig.Sender;
                m.ReplyText = ChatText.Snippet(orig.Text, 80);
            }

            history.Add(m);
            byId[m.Id] = m;
            totalMessages++;

            // تاریخچه را در ۵۰۰ پیام نگه می‌داریم
            if (history.Count > 500)
            {
                for (int i = 0; i < 50; i++) byId.Remove(history[i].Id);
                history.RemoveRange(0, 50);
            }
            return m;
        }

        // پیام جدید از کلاینت: به فرستنده ACK (۲ تیک) می‌دهیم و برای بقیه پخش می‌کنیم
        private void OnClientMessage(ClientSession me, Packet p)
        {
            string text = ChatText.LimitText(p.Arg(1));
            if (text.Length == 0) return;

            ChatMessageInfo m;
            lock (stateLock)
            {
                m = NewMessage(me.Name, me.Avatar, "user", text, p.ArgLong(2));
                me.Messages++;
            }

            // تعداد پیام‌های این کاربر در لیست آنلاین سرور بلافاصله به‌روزرسانی می‌شود.
            RefreshUsersUi();

            me.Conn.Send(new Packet("ACK", p.Arg(0), m.Id.ToString(CultureInfo.InvariantCulture)));
            SendToAll(ChatText.ToPacket(m, false), me);

            ChatMessageInfo copy = m.Clone();
            Ui(() => { chat.RemoveTyper(copy.Sender); chat.View.AddMessage(copy, true); });
        }

        // پیام از طرف سرور (مدیر)
        private void ServerSend(string text, long replyId)
        {
            if (!running) return;

            ChatMessageInfo m;
            lock (stateLock)
            {
                m = NewMessage(ChatConst.ServerName, -1, "server", text, replyId);
            }
            SendToAll(ChatText.ToPacket(m, false), null);
            chat.View.AddMessage(m.Clone(), true);
        }

        private void ServerEdit(long id, string text)
        {
            if (!running) return;
            EditMessage(id, ChatConst.ServerName, ChatText.LimitText(text));
        }

        // ویرایش پیام (فقط فرستنده خود پیام)
        private void EditMessage(long id, string requester, string text)
        {
            if (text.Length == 0) return;

            bool pinned;
            lock (stateLock)
            {
                ChatMessageInfo m;
                if (!byId.TryGetValue(id, out m) || m.Deleted || m.Sender != requester) return;
                m.Text = text;
                m.Edited = true;
                pinned = pins.Contains(id);
            }

            SendToAll(new Packet("EDIT", id.ToString(CultureInfo.InvariantCulture), text), null);
            Ui(() => chat.View.SetEdited(id, text));
            if (pinned) BroadcastPins();
        }

        // حذف پیام: فرستنده یا مدیر (admin) می‌تواند حذف کند
        private void DeleteMessage(long id, string requester, bool admin)
        {
            bool wasPinned = false;
            lock (stateLock)
            {
                ChatMessageInfo m;
                if (!byId.TryGetValue(id, out m) || m.Deleted) return;
                if (!admin && m.Sender != requester) return;
                m.Deleted = true;
                m.Reactions.Clear();
                wasPinned = pins.Remove(id);
            }

            SendToAll(new Packet("DEL", id.ToString(CultureInfo.InvariantCulture)), null);
            Ui(() => chat.View.MarkDeleted(id));
            if (wasPinned) BroadcastPins();
        }

        // زدن یا برداشتن یک واکنش (ایموجی) روی پیام
        private void ToggleReaction(string user, long id, string emoji)
        {
            bool allowed = false;
            foreach (string e in ChatConst.Reactions) if (e == emoji) allowed = true;
            if (!allowed) return;

            string encoded;
            lock (stateLock)
            {
                ChatMessageInfo m;
                if (!byId.TryGetValue(id, out m) || m.Deleted) return;

                ReactionInfo ri = null;
                foreach (ReactionInfo r in m.Reactions) if (r.Emoji == emoji) { ri = r; break; }
                if (ri == null)
                {
                    ri = new ReactionInfo();
                    ri.Emoji = emoji;
                    m.Reactions.Add(ri);
                }

                if (!ri.Users.Remove(user)) ri.Users.Add(user);
                if (ri.Users.Count == 0) m.Reactions.Remove(ri);
                encoded = ChatText.EncodeReactions(m.Reactions);
            }

            SendToAll(new Packet("REACT", id.ToString(CultureInfo.InvariantCulture), encoded), null);
            List<ReactionInfo> decoded = ChatText.DecodeReactions(encoded);
            Ui(() => chat.View.SetReactions(id, decoded));
        }

        // «خوانده شد» توسط یک کاربر: به فرستنده پیام اطلاع می‌دهیم (۳ تیک)
        private void OnRead(ClientSession me, long id)
        {
            string sender;
            int count;
            lock (stateLock)
            {
                ChatMessageInfo m;
                if (!byId.TryGetValue(id, out m) || m.Deleted || m.Sender == me.Name) return;
                if (!m.Readers.Add(me.Name)) return;
                sender = m.Sender;
                count = m.Readers.Count;
            }

            ClientSession target = null;
            lock (stateLock)
            {
                foreach (ClientSession s in sessions) if (s.Name == sender) { target = s; break; }
            }
            if (target != null)
            {
                target.Conn.Send(new Packet("READ", id.ToString(CultureInfo.InvariantCulture), count.ToString(CultureInfo.InvariantCulture)));
            }
        }

        // ==================================================================================
        // سنجاق کردن پیام
        // ==================================================================================

        // سنجاق کردن یا برداشتن سنجاق (حداکثر ۵ پیام)
        private void TogglePin(long id)
        {
            lock (stateLock)
            {
                if (!pins.Remove(id))
                {
                    ChatMessageInfo m;
                    if (!byId.TryGetValue(id, out m) || m.Deleted) return;
                    pins.Insert(0, id);
                    if (pins.Count > 5) pins.RemoveAt(pins.Count - 1);
                }
            }
            BroadcastPins();
        }

        private List<PinInfo> BuildPinList()
        {
            List<PinInfo> list = new List<PinInfo>();
            lock (stateLock)
            {
                foreach (long id in pins)
                {
                    ChatMessageInfo m;
                    if (byId.TryGetValue(id, out m) && !m.Deleted)
                    {
                        PinInfo pi = new PinInfo();
                        pi.Id = id;
                        pi.Sender = m.Sender;
                        pi.Text = ChatText.Snippet(m.Text, 120);
                        list.Add(pi);
                    }
                }
            }
            return list;
        }

        private Packet BuildPinsPacket()
        {
            List<PinInfo> list = BuildPinList();
            List<string> args = new List<string>();
            args.Add(list.Count.ToString(CultureInfo.InvariantCulture));
            foreach (PinInfo pi in list)
            {
                args.Add(pi.Id.ToString(CultureInfo.InvariantCulture));
                args.Add(pi.Sender);
                args.Add(pi.Text);
            }
            return new Packet("PINS", args.ToArray());
        }

        private void BroadcastPins()
        {
            SendToAll(BuildPinsPacket(), null);
            List<PinInfo> list = BuildPinList();
            Ui(() => chat.SetPins(list));
        }

        // ==================================================================================
        // ارسال همگانی
        // ==================================================================================

        // ارسال یک بسته برای همه کاربران آماده (به جز except)
        private void SendToAll(Packet p, ClientSession except)
        {
            List<ClientSession> snapshot;
            lock (stateLock) { snapshot = new List<ClientSession>(sessions); }
            foreach (ClientSession s in snapshot)
            {
                if (s == except || !s.Ready) continue;
                s.Conn.Send(p);
            }
        }

        // پیام سیستمی (ورود / خروج) برای همه و نمایش در چت سرور
        private void BroadcastSystem(string text, int kind, ClientSession except)
        {
            DateTime now = DateTime.Now;
            SendToAll(new Packet("SYS", text, now.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture), kind.ToString(CultureInfo.InvariantCulture)), except);
            Ui(() => chat.View.AddSystem(text, now, kind));
        }

        // فرستادن لیست کاربران آنلاین (نام و آواتار) برای همه
        private void BroadcastUsers()
        {
            List<string> args = new List<string>();
            lock (stateLock)
            {
                foreach (ClientSession s in sessions)
                {
                    if (!s.Ready) continue;
                    args.Add(s.Name);
                    args.Add(s.Avatar.ToString(CultureInfo.InvariantCulture));
                }
            }
            SendToAll(new Packet("USERS", args.ToArray()), null);
        }

        // به‌روزرسانی لیست کاربران در فرم سرور (با آی‌پی، زمان اتصال و ...)
        private void RefreshUsersUi()
        {
            List<UserRow> rows = new List<UserRow>();
            lock (stateLock)
            {
                foreach (ClientSession s in sessions)
                {
                    if (!s.Ready) continue;
                    UserRow u = new UserRow();
                    u.Name = s.Name;
                    u.Avatar = s.Avatar;
                    u.Ip = s.Ip;
                    u.Port = s.Port;
                    u.ConnectedAt = s.ConnectedAt;
                    u.Messages = s.Messages;
                    rows.Add(u);
                }
            }
            Ui(() =>
            {
                users.SetUsers(rows);
                lblUsers.Text = "کاربران آنلاین (" + Fa.Digits(rows.Count) + ")";
            });
        }

        // قطع اتصال یک کاربر توسط مدیر (دکمه «قطع اتصال» در لیست کاربران)
        private void KickUser(string name)
        {
            ClientSession target = null;
            lock (stateLock)
            {
                foreach (ClientSession s in sessions) if (s.Name == name) { target = s; break; }
            }
            if (target == null) return;

            target.Kicked = true;
            target.Conn.Send(new Packet("KICK", "شما توسط سرور از چت خارج شدید."));
            target.Conn.Close();      // ترید کاربر با خطا تمام می‌شود و RemoveSession اجرا می‌شود
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // با بستن پنجره، اگر سرور روشن است اتصال همه قطع می‌شود
            if (running) StopServer("برنامه سرور بسته شد.");
            statsTimer.Stop();
            Theme.Changed -= OnThemeChanged;
            base.OnFormClosing(e);
        }
    }
}
