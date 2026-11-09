using ChatpApp.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace ChatpApp.Server
{
    // هر اتصال یک سوکت، جریان شبکه و قفل ارسال مستقل دارد تا ارسال هم‌زمان بسته‌ها با هم تداخل نکند.
    internal sealed class ClientSession
    {
        public Guid Id; public TcpClient Client; public NetworkStream Stream; public readonly object WriteLock = new object();
        public string UserName; public string Avatar; public string IpAddress; public DateTime ConnectedAt;
    }

    // شناسه پیام در آیتم فهرست نگهداری می‌شود تا مدیر بتواند همان پیام را سنجاق کند.
    internal sealed class ServerMessageItem
    {
        public string Id; public string User; public string Text;
        public override string ToString() { return User + "  |  " + (Text ?? "").Replace("\r", " ").Replace("\n", " "); }
    }

    public partial class ChatServer : Form
    {
        private readonly ConcurrentDictionary<Guid, ClientSession> _clients = new ConcurrentDictionary<Guid, ClientSession>();
        private readonly List<ChatMessageInfo> _history = new List<ChatMessageInfo>();
        private readonly Dictionary<string, Dictionary<string, HashSet<string>>> _reactions = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        private readonly HashSet<string> _readMessages = new HashSet<string>();
        private readonly object _sync = new object();
        private TcpListener _listener; private Thread _acceptThread; private volatile bool _running;
        private string _pinnedId; private bool _dark = true;
        private System.Windows.Forms.Timer _timer;

        public ChatServer()
        {
            // تمام کنترل‌ها در فایل Designer ساخته می‌شوند و در حالت طراحی هم قابل مشاهده هستند.
            InitializeComponent();
            WireUiEvents();
            // در زمان طراحی، داده‌های نمونه برای دیدن چیدمان نگه داشته می‌شوند و تایمر شبکه فعال نمی‌شود.
            bool designTime = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            if (!designTime)
            {
                _logs.Items.Clear();
                _messages.Items.Clear();
                Shown += delegate
                {
                    SetState(false);
                    RefreshUsers();
                    // پس از تکمیل چیدمان واقعی فرم، فاصله جداکننده را با اندازه نهایی تنظیم می‌کنیم.
                    SetSafeSplitterDistance();
                };
                FormClosing += OnClosing;
                // این تایمر مدت آنلاین بودن و تعداد اتصال‌ها را در پنل سرور تازه‌سازی می‌کند.
                _timer = new System.Windows.Forms.Timer { Interval = 1000 };
                _timer.Tick += delegate { RefreshUsers(); };
                _timer.Start();
            }
            ApplyTheme();
        }

        #region اتصال رویدادهای رابط کاربری
        // در این بخش فقط رفتار دکمه‌ها و رویدادهای فرم تعریف شده است؛ چیدمان در Designer قرار دارد.
        private void WireUiEvents()
        {
            _theme.Click += delegate { _dark = !_dark; ApplyTheme(); };
            // یک دکمه، وضعیت واقعی Listener را کنترل می‌کند.
            _start.Click += ToggleServer;
            _disconnect.Click += DisconnectUser;
            _send.Click += SendAnnouncement;
            _pin.Click += PinSelected;
            _unpin.Click += delegate { SetPinned(null); };
            // اندازه جدول و دکمه قطع اتصال با اندازه کارت کاربران هماهنگ می‌شود.
            _usersCard.Resize += UsersCard_Resize;
            UsersCard_Resize(_usersCard, EventArgs.Empty);
        }

        private void UsersCard_Resize(object sender, EventArgs e)
        {
            if (_usersCard == null || _users == null || _disconnect == null) return;
            _users.Width = Math.Max(100, _usersCard.ClientSize.Width - 24);
            _users.Height = Math.Max(80, _usersCard.ClientSize.Height - 98);
            _disconnect.Top = Math.Max(0, _usersCard.ClientSize.Height - 47);
            _disconnect.Width = Math.Max(100, _usersCard.ClientSize.Width - 24);
        }

        // این متد پس از تکمیل Layout، اندازه و فاصله SplitContainer را متناسب با فضای واقعی تنظیم می‌کند.
        private void SetSafeSplitterDistance()
        {
            // تا قبل از ساخته‌شدن Handle، اندازه نهایی کنترل قابل اتکا نیست.
            if (_mainSplit == null || !_mainSplit.IsHandleCreated)
                return;

            int availableWidth = _mainSplit.ClientSize.Width;
            int splitterWidth = _mainSplit.SplitterWidth;

            // در عرض‌های بسیار کوچک، تنظیم فاصله را به زمان مناسب‌تری موکول می‌کنیم.
            if (availableWidth <= splitterWidth)
                return;

            // حداقل عرض مطلوب دو پنل مدیریت سرور.
            int panel1Minimum = 350;
            int panel2Minimum = 520;

            // اگر فضای واقعی کافی نبود، حداقل‌ها را متناسب با عرض موجود کوچک می‌کنیم.
            int availableForPanels = availableWidth - splitterWidth;
            int requiredWidth = panel1Minimum + panel2Minimum;
            if (requiredWidth > availableForPanels)
            {
                double scale = availableForPanels / (double)requiredWidth;
                panel1Minimum = (int)Math.Floor(panel1Minimum * scale);
                panel2Minimum = availableForPanels - panel1Minimum;
            }

            // ابتدا محدودیت‌های قبلی را موقتاً آزاد می‌کنیم تا مقدار جدید بدون تعارض اعمال شود.
            _mainSplit.Panel1MinSize = 0;
            _mainSplit.Panel2MinSize = 0;

            // فاصله پیشنهادی پنل اول را داخل بازه مجاز قرار می‌دهیم.
            int maximumDistance = availableWidth - panel2Minimum - splitterWidth;
            int safeDistance = Math.Max(panel1Minimum, Math.Min(400, maximumDistance));
            _mainSplit.SplitterDistance = safeDistance;

            // پس از تنظیم فاصله معتبر، حداقل عرض نهایی پنل‌ها را اعمال می‌کنیم.
            _mainSplit.Panel1MinSize = panel1Minimum;
            _mainSplit.Panel2MinSize = panel2Minimum;
        }

        #endregion

        #region تنظیم ظاهر و رنگ‌بندی فرم
        // رنگ‌ها به‌صورت مرکزی اعمال می‌شوند تا کل پنل سرور بین دو تم هماهنگ تغییر کند.
        private void ApplyTheme()
        {
            // پالت Emerald Dark با پس‌زمینه عمیق و تاکید زمردی.
            Color back = _dark ? Color.FromArgb(7, 17, 17) : Color.FromArgb(240, 248, 245);
            Color card = _dark ? Color.FromArgb(13, 31, 30) : Color.White;
            Color fore = _dark ? Color.FromArgb(226, 245, 239) : Color.FromArgb(24, 48, 43);
            BackColor = back; ForeColor = fore; ThemeControls(this, card, fore);
            _rootPanel.BackColor = back;
            _headerPanel.BackColor = card;
            _networkCard.BackColor = card;
            _usersCard.BackColor = card;
            _trafficHeader.BackColor = card;
            _composeCard.BackColor = card;
            _titleLabel.ForeColor = _dark ? Color.FromArgb(110, 231, 183) : Color.FromArgb(5, 150, 105);
            _theme.BackColor = _dark ? Color.FromArgb(20, 83, 69) : Color.FromArgb(209, 250, 229);
            _theme.ForeColor = _dark ? Color.White : Color.FromArgb(6, 78, 59);
            // دکمه اصلی براساس وضعیت سرور رنگ می‌گیرد.
            _start.BackColor = _running ? Color.FromArgb(5, 150, 105) : Color.FromArgb(37, 99, 235);
            _start.ForeColor = Color.White;
            _start.FlatAppearance.MouseOverBackColor = Color.FromArgb(4, 120, 87);
            _unpin.BackColor = _dark ? Color.FromArgb(71, 85, 105) : Color.FromArgb(203, 213, 225);
            _unpin.ForeColor = _dark ? Color.White : Color.FromArgb(30, 41, 59);
            _count.ForeColor = Color.FromArgb(52, 211, 153);
            _traffic.ForeColor = _dark ? Color.FromArgb(110, 231, 183) : Color.FromArgb(5, 150, 105);
            _host.BackColor = _dark ? Color.FromArgb(8, 23, 22) : Color.FromArgb(248, 252, 250);
            _port.BackColor = _dark ? Color.FromArgb(8, 23, 22) : Color.FromArgb(248, 252, 250);
            _host.ForeColor = fore; _port.ForeColor = fore;
            _hostLabel.ForeColor = fore; _portLabel.ForeColor = fore;
            if (_users != null) { _users.BackgroundColor = card; _users.DefaultCellStyle.BackColor = card; _users.DefaultCellStyle.ForeColor = fore;
                _users.ColumnHeadersDefaultCellStyle.BackColor = _dark ? Color.FromArgb(31, 45, 67) : Color.FromArgb(226, 232, 240); _users.ColumnHeadersDefaultCellStyle.ForeColor = fore; _users.EnableHeadersVisualStyles = false; }
        }
        private void ThemeControls(Control parent, Color card, Color fore)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Button) { c.BackColor = _dark ? Color.FromArgb(5, 120, 87) : Color.FromArgb(16, 185, 129); c.ForeColor = Color.White; }
                else if (c is TextBox || c is ListBox || c is DataGridView) { c.BackColor = _dark ? Color.FromArgb(8, 23, 22) : Color.FromArgb(248, 252, 250); c.ForeColor = fore; }
                else { c.BackColor = card; c.ForeColor = fore; }
                ThemeControls(c, card, fore);
            }
            if (_logs != null) _logs.ForeColor = fore; if (_messages != null) _messages.ForeColor = fore;
        }
        #endregion

        #region سرور چنداتصالی و چرخه عمر سوکت‌ها
        // دکمه واحد براساس وضعیت فعلی سرور، شروع یا توقف سوکت را انجام می‌دهد.
        private void ToggleServer(object sender, EventArgs e)
        {
            if (_running) StopServer(false);
            else StartServer();
        }

        private void StartServer()
        {
            int port;
            if (!int.TryParse(_port.Text.Trim(), out port) || port < 1 || port > 65535)
            { MessageBox.Show("شماره پورت باید بین ۱ تا ۶۵۵۳۵ باشد.", "پورت نامعتبر", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            // آدرس واردشده باید IP معتبر باشد؛ 0.0.0.0 یعنی تمام رابط‌های IPv4.
            IPAddress bindAddress;
            if (!IPAddress.TryParse(_host.Text.Trim(), out bindAddress))
            { MessageBox.Show("آدرس IP واردشده معتبر نیست.", "IP نامعتبر", MessageBoxButtons.OK, MessageBoxIcon.Warning); _host.Focus(); return; }
            try
            {
                // Listener دقیقاً روی IP و پورتی که مدیر تعیین کرده Bind می‌شود.
                _listener = new TcpListener(bindAddress, port); _listener.Start(100); _running = true;
                TcpListener currentListener = _listener;
                _acceptThread = new Thread(new ThreadStart(delegate { AcceptLoop(currentListener); })) { IsBackground = true, Name = "ChatServer-Accept" }; _acceptThread.Start();
                SetState(true); Log("سرور روی " + bindAddress + ":" + port + " فعال شد و آماده پذیرش چندین کلاینت است.");
            }
            catch (Exception ex) { _running = false; Log("خطا در شروع سرور: " + ex.Message); MessageBox.Show(ex.Message, "خطای شبکه"); }
        }

        // حلقه پذیرش فقط اتصال جدید را می‌گیرد؛ مدیریت هر کاربر به Thread جداگانه سپرده می‌شود.
        private void AcceptLoop(TcpListener currentListener)
        {
            while (_running && object.ReferenceEquals(_listener, currentListener))
            {
                try
                {
                    TcpClient tcp = currentListener.AcceptTcpClient(); tcp.NoDelay = true;
                    var session = new ClientSession { Id = Guid.NewGuid(), Client = tcp, Stream = tcp.GetStream(),
                        IpAddress = ((IPEndPoint)tcp.Client.RemoteEndPoint).Address.ToString(), ConnectedAt = DateTime.Now };
                    var worker = new Thread(new ThreadStart(delegate { HandleClient(session); })); worker.IsBackground = true; worker.Start();
                }
                catch (ObjectDisposedException) { break; }
                catch (SocketException ex) { if (_running && object.ReferenceEquals(_listener, currentListener)) Log("خطای پذیرش اتصال: " + ex.Message); else break; }
                catch (Exception ex) { if (_running && object.ReferenceEquals(_listener, currentListener)) Log("خطای سرور: " + ex.Message); else break; }
            }
        }

        private void HandleClient(ClientSession s)
        {
            try
            {
                ChatPacket join = FrameProtocol.Receive(s.Stream);
                if (join == null || join.Type != "Join") throw new InvalidOperationException("درخواست ورود معتبر نیست.");
                string name = (join.UserName ?? "").Trim(); if (name.Length > 24) name = name.Substring(0, 24);
                name = name.Replace("\r", "").Replace("\n", "");
                if (name.Length == 0) throw new InvalidOperationException("نام کاربری خالی است.");

                // بررسی نام و ثبت اتصال زیر یک قفل انجام می‌شود تا دو ورود هم‌نام هم‌زمان پذیرفته نشوند.
                lock (_sync)
                {
                    if (_clients.Values.Any(x => string.Equals(x.UserName, name, StringComparison.OrdinalIgnoreCase)))
                    { FrameProtocol.Send(s.Stream, new ChatPacket { Type = "Error", Text = "این نام کاربری در حال حاضر آنلاین است." }, s.WriteLock); return; }
                    s.UserName = name; s.Avatar = string.IsNullOrWhiteSpace(join.Avatar) ? "🙂" : join.Avatar;
                    _clients.TryAdd(s.Id, s);
                }

                // پیام خوش‌آمدگویی همراه با تاریخچه و کاربران آنلاین برای کلاینت تازه‌وارد ارسال می‌شود.
                Send(s, new ChatPacket { Type = "Welcome", UserName = s.UserName, Avatar = s.Avatar,
                    Text = "خوش اومدی " + s.UserName + "! به جمع دوستان پیام‌رسان خوش اومدی 🌟",
                    Users = UserSnapshot(), History = HistorySnapshot(), IsPinned = !string.IsNullOrEmpty(_pinnedId),
                    MessageId = _pinnedId, ReplyToText = PinnedText(), ReplyToUser = PinnedUser() });
                Log(s.Avatar + " " + s.UserName + " از IP " + s.IpAddress + " متصل شد.");
                Broadcast(new ChatPacket { Type = "System", Text = s.UserName + " به گفتگو پیوست.", UserName = "سیستم", Avatar = "⚙️", TimestampTicks = DateTime.Now.Ticks }, s.Id);
                BroadcastUsers();

                // هر Thread فقط جریان همان کاربر را می‌خواند و قطع یک کلاینت روی بقیه اثر نمی‌گذارد.
                while (_running && s.Client.Connected)
                {
                    ChatPacket packet = FrameProtocol.Receive(s.Stream); if (packet == null) break;
                    ProcessPacket(s, packet);
                }
            }
            catch (Exception ex)
            {
                if (_running && _clients.ContainsKey(s.Id) && !string.IsNullOrEmpty(s.UserName) && !(ex is System.IO.IOException) && !(ex is SocketException))
                    Log("ارتباط " + s.UserName + " با خطا روبه‌رو شد: " + ex.Message);
            }
            finally { RemoveClient(s, true); }
        }

        private void ProcessPacket(ClientSession sender, ChatPacket p)
        {
            switch (p.Type)
            {
                case "Chat": NewMessage(sender, p); break;
                case "Typing": p.UserName = sender.UserName; Broadcast(p, sender.Id); break;
                case "ReadReceipt": ReadReceipt(sender, p.MessageId); break;
                case "Reaction": ToggleReaction(sender, p); break;
                case "EditMessage": EditMessage(sender, p); break;
                case "DeleteMessage": DeleteMessage(sender, p); break;
            }
        }

        private void NewMessage(ClientSession sender, ChatPacket p)
        {
            string text = (p.Text ?? "").Trim(); if (text.Length == 0 || text.Length > 4000) return;
            var m = new ChatMessageInfo { MessageId = string.IsNullOrEmpty(p.MessageId) ? Guid.NewGuid().ToString("N") : p.MessageId,
                SenderName = sender.UserName, SenderAvatar = sender.Avatar, Text = text, TimestampTicks = DateTime.Now.Ticks,
                ReplyToId = p.ReplyToId, ReplyToText = p.ReplyToText, ReplyToUser = p.ReplyToUser };
            Store(m); AddServerMessage(m); Log("پیام جدید از " + sender.UserName + " دریافت شد.");
            // پیام برای تمام کاربران به‌جز فرستنده پخش می‌شود؛ تعداد ارسال‌های موفق وضعیت دو تیک را تعیین می‌کند.
            Broadcast(MessagePacket("Message", m), sender.Id);
            // دریافت تایید از سرور یعنی پیام ثبت شده است؛ حتی اگر فعلاً گیرنده دیگری آنلاین نباشد دو تیک نمایش داده می‌شود.
            Send(sender, new ChatPacket { Type = "MessageConfirmed", MessageId = m.MessageId, TimestampTicks = m.TimestampTicks, DeliveredCount = 1 });
            RefreshTraffic();
        }

        private void EditMessage(ClientSession sender, ChatPacket p)
        {
            ChatMessageInfo m;
            lock (_sync)
            {
                m = _history.FirstOrDefault(x => x.MessageId == p.MessageId);
                if (m == null || m.IsDeleted || m.IsServer || m.SenderName != sender.UserName) return;
                string text = (p.Text ?? "").Trim(); if (text.Length == 0 || text.Length > 4000) return;
                m.Text = text; m.IsEdited = true; m.TimestampTicks = DateTime.Now.Ticks;
            }
            Broadcast(MessagePacket("MessageUpdated", m)); Log(sender.UserName + " پیام خودش را ویرایش کرد.");
        }

        private void DeleteMessage(ClientSession sender, ChatPacket p)
        {
            ChatMessageInfo m;
            lock (_sync)
            {
                m = _history.FirstOrDefault(x => x.MessageId == p.MessageId);
                if (m == null || m.IsDeleted || m.IsServer || m.SenderName != sender.UserName) return;
                m.IsDeleted = true; m.Text = "این پیام حذف شده است."; m.IsEdited = false;
            }
            Broadcast(MessagePacket("MessageDeleted", m)); Log(sender.UserName + " یکی از پیام‌های خودش را حذف کرد.");
        }

        private void ToggleReaction(ClientSession sender, ChatPacket p)
        {
            if (string.IsNullOrEmpty(p.Reaction)) return;
            ReactionCounter[] result;
            lock (_sync)
            {
                if (!_history.Any(x => x.MessageId == p.MessageId)) return;
                Dictionary<string, HashSet<string>> map;
                if (!_reactions.TryGetValue(p.MessageId, out map)) { map = new Dictionary<string, HashSet<string>>(); _reactions[p.MessageId] = map; }
                HashSet<string> users;
                if (!map.TryGetValue(p.Reaction, out users)) { users = new HashSet<string>(StringComparer.OrdinalIgnoreCase); map[p.Reaction] = users; }
                // کلیک مجدد روی همان ایموجی، واکنش کاربر را حذف می‌کند.
                if (!users.Add(sender.UserName)) users.Remove(sender.UserName);
                if (users.Count == 0) map.Remove(p.Reaction);
                result = ReactionSnapshot(p.MessageId);
            }
            Broadcast(new ChatPacket { Type = "ReactionUpdate", MessageId = p.MessageId, Reactions = result });
        }

        private ReactionCounter[] ReactionSnapshot(string id)
        {
            Dictionary<string, HashSet<string>> map;
            if (!_reactions.TryGetValue(id, out map)) return new ReactionCounter[0];
            return map.Select(x => new ReactionCounter { Emoji = x.Key, Count = x.Value.Count, Users = x.Value.ToArray() }).ToArray();
        }

        private void ReadReceipt(ClientSession reader, string id)
        {
            string author; bool first;
            lock (_sync)
            {
                var m = _history.FirstOrDefault(x => x.MessageId == id);
                if (m == null || m.IsServer || m.SenderName == reader.UserName) return;
                author = m.SenderName; first = _readMessages.Add(id);
            }
            // اولین کاربرِ خواننده، وضعیت پیام را برای فرستنده به سه تیک تغییر می‌دهد.
            if (first)
            {
                ClientSession owner = _clients.Values.FirstOrDefault(x => x.UserName == author);
                if (owner != null) Send(owner, new ChatPacket { Type = "MessageRead", MessageId = id, UserName = reader.UserName });
            }
        }

        private void Store(ChatMessageInfo m)
        {
            lock (_sync) { _history.Add(m); if (_history.Count > 300) _history.RemoveAt(0); }
        }

        private ChatPacket MessagePacket(string type, ChatMessageInfo m)
        {
            ReactionCounter[] reactions; lock (_sync) reactions = ReactionSnapshot(m.MessageId);
            return new ChatPacket { Type = type, UserName = m.SenderName, Avatar = m.SenderAvatar, Text = m.Text,
                MessageId = m.MessageId, ReplyToId = m.ReplyToId, ReplyToText = m.ReplyToText, ReplyToUser = m.ReplyToUser,
                TimestampTicks = m.TimestampTicks, IsEdited = m.IsEdited, IsDeleted = m.IsDeleted, IsServer = m.IsServer, Reactions = reactions };
        }
        #endregion

        #region ارسال، تاریخچه، وضعیت کاربران و کنترل مدیریتی
        private bool Send(ClientSession target, ChatPacket packet)
        {
            try { if (target == null || !target.Client.Connected) return false; FrameProtocol.Send(target.Stream, packet, target.WriteLock); return true; }
            catch { RemoveClient(target, true); return false; }
        }
        private int Broadcast(ChatPacket packet, Guid? except = null)
        {
            int count = 0;
            foreach (ClientSession client in _clients.Values.ToArray())
                if ((!except.HasValue || client.Id != except.Value) && Send(client, packet)) count++;
            return count;
        }
        private UserInfo[] UserSnapshot()
        {
            return _clients.Values.OrderBy(x => x.UserName).Select(x => new UserInfo { UserName = x.UserName, Avatar = x.Avatar,
                IpAddress = x.IpAddress, ConnectedAtTicks = x.ConnectedAt.Ticks }).ToArray();
        }
        private ChatMessageInfo[] HistorySnapshot()
        {
            lock (_sync) return _history.Select(x => new ChatMessageInfo { MessageId = x.MessageId, SenderName = x.SenderName,
                SenderAvatar = x.SenderAvatar, Text = x.Text, TimestampTicks = x.TimestampTicks, ReplyToId = x.ReplyToId,
                ReplyToText = x.ReplyToText, ReplyToUser = x.ReplyToUser, IsEdited = x.IsEdited, IsDeleted = x.IsDeleted,
                IsServer = x.IsServer, Reactions = ReactionSnapshot(x.MessageId) }).ToArray();
        }
        private void BroadcastUsers()
        {
            Broadcast(new ChatPacket { Type = "Users", Users = UserSnapshot() }); RefreshUsers();
        }
        private void RemoveClient(ClientSession s, bool announce)
        {
            if (s == null) return;
            ClientSession removed;
            if (_clients.TryRemove(s.Id, out removed))
            {
                try { removed.Client.Close(); } catch { }
                if (announce && _running && !string.IsNullOrEmpty(removed.UserName))
                {
                    Log("اتصال " + removed.UserName + " قطع شد.");
                    Broadcast(new ChatPacket { Type = "System", Text = removed.UserName + " از گفتگو خارج شد.", UserName = "سیستم", Avatar = "⚙️", TimestampTicks = DateTime.Now.Ticks });
                }
                BroadcastUsers();
            }
            else { try { s.Client.Close(); } catch { } }
        }

        private void StopServer(bool closing)
        {
            if (!_running && _listener == null) return;
            _running = false;
            // توقف، فقط Listener و سوکت‌های کاربران را می‌بندد و فرم سرور را باز نگه می‌دارد.
            try { if (_listener != null) _listener.Stop(); } catch { } _listener = null;
            foreach (ClientSession client in _clients.Values.ToArray())
            {
                try { FrameProtocol.Send(client.Stream, new ChatPacket { Type = "ServerStopped", Text = "ارتباط با سرور توسط مدیر متوقف شد." }, client.WriteLock); } catch { }
                try { client.Client.Close(); } catch { }
            }
            _clients.Clear(); SetState(false); RefreshUsers();
            Log(closing ? "برنامه سرور در حال بسته‌شدن است." : "سرور متوقف شد؛ پنل مدیریت همچنان فعال است.");
        }

        private void DisconnectUser(object sender, EventArgs e)
        {
            if (_users.SelectedRows.Count == 0) { MessageBox.Show("ابتدا یک کاربر را از فهرست انتخاب کنید."); return; }
            Guid id = (Guid)_users.SelectedRows[0].Tag; ClientSession selected;
            if (_clients.TryGetValue(id, out selected))
            {
                Send(selected, new ChatPacket { Type = "Kicked", Text = "اتصال شما توسط مدیر سرور قطع شد." });
                RemoveClient(selected, true);
            }
        }

        // پیام مدیر با هویت سرور منتشر می‌شود و از پیام‌های عادی کاربران متمایز است.
        private void SendAnnouncement(object sender, EventArgs e)
        {
            string text = (_announcement.Text ?? "").Trim(); if (!_running || text.Length == 0) return;
            if (text.Length > 4000) { MessageBox.Show("پیام نباید بیشتر از ۴۰۰۰ نویسه باشد."); return; }
            var m = new ChatMessageInfo { MessageId = Guid.NewGuid().ToString("N"), SenderName = "سرور", SenderAvatar = "🖥️",
                Text = text, TimestampTicks = DateTime.Now.Ticks, IsServer = true };
            Store(m); AddServerMessage(m); Broadcast(MessagePacket("Message", m)); Log("پیام مدیریتی برای همه کاربران ارسال شد."); _announcement.Clear();
        }
        private void PinSelected(object sender, EventArgs e)
        {
            var item = _messages.SelectedItem as ServerMessageItem;
            if (item == null) { MessageBox.Show("ابتدا یک پیام را از فهرست انتخاب کنید."); return; }
            SetPinned(item.Id);
        }
        private void SetPinned(string id)
        {
            _pinnedId = id; ChatMessageInfo m = null;
            if (!string.IsNullOrEmpty(id)) lock (_sync) m = _history.FirstOrDefault(x => x.MessageId == id);
            Broadcast(new ChatPacket { Type = "Pinned", IsPinned = m != null, MessageId = m == null ? "" : m.MessageId,
                Text = m == null ? "" : m.Text, UserName = m == null ? "" : m.SenderName });
            _pinned.Text = m == null ? "پیامی سنجاق نشده است" : "سنجاق: " + m.SenderName + " — " + Short(m.Text, 40);
            Log(m == null ? "سنجاق پیام برداشته شد." : "پیام " + m.SenderName + " سنجاق شد.");
        }
        private string PinnedText()
        { lock (_sync) { var m = _history.FirstOrDefault(x => x.MessageId == _pinnedId); return m == null ? "" : m.Text; } }
        private string PinnedUser()
        { lock (_sync) { var m = _history.FirstOrDefault(x => x.MessageId == _pinnedId); return m == null ? "" : m.SenderName; } }
        private string Short(string s, int length) { if (string.IsNullOrEmpty(s) || s.Length <= length) return s ?? ""; return s.Substring(0, length) + "…"; }
        private void AddServerMessage(ChatMessageInfo m)
        {
            Ui(delegate { if (_messages == null || _messages.IsDisposed) return; _messages.Items.Add(new ServerMessageItem { Id = m.MessageId, User = m.SenderName, Text = m.Text }); _messages.TopIndex = _messages.Items.Count - 1; });
        }
        private void Log(string value)
        {
            Ui(delegate { if (_logs == null || _logs.IsDisposed) return; _logs.Items.Add(DateTime.Now.ToString("HH:mm:ss") + "  •  " + value); _logs.TopIndex = _logs.Items.Count - 1; });
        }
        private void RefreshUsers()
        {
            Ui(delegate
            {
                if (_users == null || _users.IsDisposed) return;
                Guid? selectedId = _users.SelectedRows.Count > 0 && _users.SelectedRows[0].Tag is Guid ? (Guid?)_users.SelectedRows[0].Tag : null;
                var list = _clients.Values.OrderBy(x => x.UserName).ToArray(); _users.Rows.Clear();
                foreach (var u in list)
                {
                    int row = _users.Rows.Add(u.Avatar, u.UserName, u.IpAddress, u.ConnectedAt.ToString("HH:mm:ss"), Duration(DateTime.Now - u.ConnectedAt));
                    _users.Rows[row].Tag = u.Id;
                    if (selectedId.HasValue && selectedId.Value == u.Id) _users.Rows[row].Selected = true;
                }
                _count.Text = list.Length + " کاربر آنلاین"; RefreshTrafficCore();
            });
        }
        private string Duration(TimeSpan t) { return string.Format("{0:00}:{1:00}:{2:00}", (int)t.TotalHours, t.Minutes, t.Seconds); }
        private void RefreshTraffic() { Ui(RefreshTrafficCore); }
        private void RefreshTrafficCore()
        {
            if (_traffic == null || _traffic.IsDisposed) return;
            int count; lock (_sync) count = _history.Count;
            _traffic.Text = "پیام‌های نگهداری‌شده: " + count + "  |  اتصال‌های فعال: " + _clients.Count;
        }
        private void SetState(bool running)
        {
            Ui(delegate
            {
                _status.Text = running ? "● سرور فعال است و آماده دریافت اتصال" : "● سرور متوقف است";
                _status.ForeColor = running ? Color.FromArgb(34, 197, 94) : Color.FromArgb(239, 100, 100);
                _start.Enabled = true;
                _start.Text = running ? "■  توقف سوکت" : "▶  اجرای سوکت";
                _start.BackColor = running ? Color.FromArgb(5, 150, 105) : Color.FromArgb(37, 99, 235);
                _start.FlatAppearance.MouseOverBackColor = Color.FromArgb(4, 120, 87);
                _port.Enabled = !running; _host.Enabled = !running;
            });
        }
        private void Ui(Action action)
        {
            try { if (IsDisposed || !IsHandleCreated) return; if (InvokeRequired) BeginInvoke(action); else action(); }
            catch (InvalidOperationException) { }
        }
        private void OnClosing(object sender, FormClosingEventArgs e)
        { if (_timer != null) { _timer.Stop(); _timer.Dispose(); } StopServer(true); }
        #endregion
    }
}
