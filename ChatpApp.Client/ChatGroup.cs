using ChatpApp.Protocol;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ChatpApp.Client
{
    public sealed partial class ChatGroup : Form
    {
        private readonly string _userName, _avatar, _host; private readonly int _port;
        private TcpClient _client; private NetworkStream _stream; private readonly object _writeLock = new object();
        private Thread _receiver; private volatile bool _connected; private bool _closing;
        private readonly Dictionary<string, ChatMessageInfo> _messages = new Dictionary<string, ChatMessageInfo>();
        private readonly Dictionary<string, Panel> _messageCards = new Dictionary<string, Panel>();
        private readonly Dictionary<string, int> _statuses = new Dictionary<string, int>();
        private readonly HashSet<string> _readPending = new HashSet<string>();
        private string _replyId, _replyText, _replyUser, _editingId, _pinnedId;
        private System.Windows.Forms.Timer _typingTimer, _remoteTypingTimer;
        private NotifyIcon _notify;
        private bool _typingSent; private bool _dark;

        // سوکت و بسته Welcome از فرم ورود می‌آیند؛ بنابراین فرم Login بعد از ورود کاملاً بسته شده است.
        // سازنده بدون پارامتر برای طراح فرم Rider و Visual Studio استفاده می‌شود.
        // در این حالت هیچ سوکت یا تایمر شبکه‌ای ساخته نمی‌شود.
        public ChatGroup()
        {
            _userName = "کاربر"; _avatar = "🙂"; _host = "127.0.0.1"; _port = 5050;
            _dark = ThemeSettings.Dark;
            InitializeComponent();
            WireUiEvents();
            _currentUserLabel.Text = _avatar + "  " + _userName;
            _typingLabel.Text = "";
            ApplyTheme();
        }

        public ChatGroup(string userName, string avatar, string host, int port, TcpClient client, ChatPacket welcome)
            : this()
        {
            _userName = userName; _avatar = avatar; _host = host; _port = port;
            _client = client; _stream = client.GetStream(); _dark = ThemeSettings.Dark;
            _currentUserLabel.Text = _avatar + "  " + _userName;
            // کارت‌های نمونه فقط برای پیش‌نمایش Designer هستند و در اجرای واقعی حذف می‌شوند.
            _messageList.Controls.Clear();
            _userList.Controls.Clear();
            _pinPanel.Visible = false; _replyPanel.Visible = false; _typingLabel.Text = "";
            // پاک‌کردن متن نمونه پیش از فعال‌شدن وضعیت شبکه انجام می‌شود تا رویداد تایپ ناخواسته ارسال نشود.
            _composer.Clear();
            _connected = true;
            ApplyTheme();
            ProcessWelcome(welcome);
            // دریافت شبکه پس از ساخته‌شدن Handle فرم شروع می‌شود تا اولین پیام‌ها از دست نروند.
            Shown += delegate { StartReceiver(); MarkVisibleMessagesRead(); };
            _notify = new NotifyIcon { Icon = SystemIcons.Application, Visible = true, Text = "Nexa Chat - پیام‌رسان شبکه" };
            _typingTimer = new System.Windows.Forms.Timer { Interval = 900 };
            _typingTimer.Tick += delegate { _typingTimer.Stop(); if (_typingSent) { Send(new ChatPacket { Type = "Typing", Text = "0" }); _typingSent = false; } };
            _remoteTypingTimer = new System.Windows.Forms.Timer { Interval = 2500 };
            _remoteTypingTimer.Tick += delegate { _remoteTypingTimer.Stop(); _typingLabel.Text = ""; };
            FormClosing += OnClosing; Activated += delegate { MarkVisibleMessagesRead(); };
        }

        #region اتصال رویدادهای رابط کاربری
        // این متد فقط رویدادها را به کنترل‌های ساخته‌شده در فایل Designer متصل می‌کند.
        private void WireUiEvents()
        {
            _messageList.Resize += delegate { ResizeMessageCards(); };
            _reconnect.Click += Reconnect_Click;
            _theme.Click += delegate { ThemeSettings.Dark = !ThemeSettings.Dark; ApplyTheme(); };
            _pinPanel.Click += delegate { JumpToMessage(_pinnedId); };
            _pinLabel.Click += delegate { JumpToMessage(_pinnedId); };
            _cancelReply.Click += delegate { ClearReply(); };
            _emoji.Click += ShowEmojiPanel;
            _composer.TextChanged += Composer_TextChanged;
            _composer.KeyDown += Composer_KeyDown;
            _send.Click += SendMessage_Click;
            Shown += delegate
            {
                // پس از تکمیل چیدمان فرم، اندازه پنل گفتگو و فهرست اعضا را ایمن تنظیم می‌کنیم.
                SetSafeSplitterDistance();
            };
        }
        // این متد فاصله جداکننده پنل گفتگو و فهرست اعضا را با اندازه واقعی فرم هماهنگ می‌کند.
        private void SetSafeSplitterDistance()
        {
            // تا قبل از ساخته‌شدن Handle، اندازه نهایی کنترل قابل اتکا نیست.
            if (_mainSplit == null || !_mainSplit.IsHandleCreated)
                return;

            int availableWidth = _mainSplit.ClientSize.Width;
            int splitterWidth = _mainSplit.SplitterWidth;
            if (availableWidth <= splitterWidth)
                return;

            // حداقل عرض مطلوب برای ناحیه گفتگو و فهرست کاربران.
            int panel1Minimum = 650;
            int panel2Minimum = 230;

            // در پنجره‌های کوچک‌تر، حداقل‌ها را متناسب با فضای موجود کاهش می‌دهیم.
            int availableForPanels = availableWidth - splitterWidth;
            int requiredWidth = panel1Minimum + panel2Minimum;
            if (requiredWidth > availableForPanels)
            {
                double scale = availableForPanels / (double)requiredWidth;
                panel1Minimum = (int)Math.Floor(panel1Minimum * scale);
                panel2Minimum = availableForPanels - panel1Minimum;
            }

            // محدودیت‌های قبلی را موقتاً آزاد می‌کنیم تا فاصله جدید بدون تعارض تنظیم شود.
            _mainSplit.Panel1MinSize = 0;
            _mainSplit.Panel2MinSize = 0;

            // فضای پیشنهادی پنل گفتگو را از عرض کل کم می‌کنیم و مقدار را در بازه مجاز می‌بندیم.
            int desiredDistance = availableWidth - 310;
            int maximumDistance = availableWidth - panel2Minimum - splitterWidth;
            int safeDistance = Math.Max(panel1Minimum, Math.Min(desiredDistance, maximumDistance));
            _mainSplit.SplitterDistance = safeDistance;

            // پس از تنظیم فاصله معتبر، حداقل عرض نهایی پنل‌ها را اعمال می‌کنیم.
            _mainSplit.Panel1MinSize = panel1Minimum;
            _mainSplit.Panel2MinSize = panel2Minimum;
        }

        #endregion

        #region دریافت و پردازش رویدادهای سرور
        private void ProcessWelcome(ChatPacket welcome)
        {
            if (welcome == null) return;
            if (welcome.History != null)
            {
                foreach (ChatMessageInfo message in welcome.History)
                {
                    UpsertMessage(message, false);
                    if (message.SenderName != _userName && message.SenderName != "سرور") _readPending.Add(message.MessageId);
                }
                MarkVisibleMessagesRead();
            }
            UpdateUsers(welcome.Users);
            if (welcome.IsPinned) SetPinned(welcome.MessageId, welcome.ReplyToUser, welcome.ReplyToText);
            // کارت خوش‌آمدگویی مستقل و متمایز از پیام‌های کاربران نمایش داده می‌شود.
            UpsertMessage(new ChatMessageInfo { MessageId = "welcome-" + Guid.NewGuid().ToString("N"), SenderName = "سرور",
                SenderAvatar = "🖥️", Text = welcome.Text, TimestampTicks = DateTime.Now.Ticks, IsServer = true }, true);
        }

        private void StartReceiver()
        {
            // هر Thread به همان Stream زمان شروع خودش متصل می‌ماند؛ Thread قدیمی بعد از اتصال مجدد روی سوکت تازه اثر نمی‌گذارد.
            NetworkStream listeningStream = _stream;
            _receiver = new Thread(new ThreadStart(delegate { ReceiveLoop(listeningStream); })) { IsBackground = true, Name = "NexaChat-Receiver" }; _receiver.Start();
        }
        private void ReceiveLoop(NetworkStream listeningStream)
        {
            try
            {
                while (_connected && !_closing && object.ReferenceEquals(_stream, listeningStream))
                {
                    ChatPacket packet = FrameProtocol.Receive(listeningStream);
                    if (packet == null) break;
                    // بسته توقف یا اخراج را روی همان Thread ثبت می‌کنیم تا EOF بعدی پیام قطع اتصال تکراری نسازد.
                    if (packet.Type == "ServerStopped" || packet.Type == "Kicked") _connected = false;
                    HandlePacket(packet);
                }
            }
            catch (Exception) { }
            if (!_closing && _connected && object.ReferenceEquals(_stream, listeningStream)) Ui(delegate { SetConnectionState(false, "ارتباط با سرور قطع شد."); AddSystemText("ارتباط شبکه قطع شد. برای ادامه از دکمه «اتصال مجدد» استفاده کن."); });
        }

        private void HandlePacket(ChatPacket packet)
        {
            Ui(delegate
            {
                switch (packet.Type)
                {
                    case "Welcome": ProcessWelcome(packet); SetConnectionState(true, "متصل"); break;
                    case "Users": UpdateUsers(packet.Users); break;
                    case "Message":
                        ChatMessageInfo incoming = FromPacket(packet); UpsertMessage(incoming, true);
                        if (incoming.SenderName != _userName) { _readPending.Add(incoming.MessageId); MarkVisibleMessagesRead(); NotifyIncoming(incoming); }
                        break;
                    case "MessageConfirmed":
                        _statuses[packet.MessageId] = packet.DeliveredCount > 0 ? 1 : 0;
                        ChatMessageInfo confirmed; if (_messages.TryGetValue(packet.MessageId, out confirmed) && packet.TimestampTicks > 0) confirmed.TimestampTicks = packet.TimestampTicks;
                        RefreshMessage(packet.MessageId); break;
                    case "MessageRead": _statuses[packet.MessageId] = 2; RefreshMessage(packet.MessageId); break;
                    case "MessageUpdated": UpsertMessage(FromPacket(packet), false); break;
                    case "MessageDeleted": UpsertMessage(FromPacket(packet), false); break;
                    case "ReactionUpdate":
                        ChatMessageInfo changed;
                        if (_messages.TryGetValue(packet.MessageId, out changed)) { changed.Reactions = packet.Reactions; UpsertMessage(changed, false); }
                        break;
                    case "System": UpsertMessage(new ChatMessageInfo { MessageId = Guid.NewGuid().ToString("N"), SenderName = "سیستم", SenderAvatar = "⚙️", Text = packet.Text, TimestampTicks = DateTime.Now.Ticks, IsServer = true }, true); break;
                    case "Typing": ShowTyping(packet.UserName, packet.Text != "0"); break;
                    case "Pinned": SetPinned(packet.IsPinned ? packet.MessageId : null, packet.UserName, packet.Text); break;
                    case "ServerStopped": SetConnectionState(false, packet.Text); AddSystemText("سرور متوقف شده است؛ پنجره گفتگو باز می‌ماند."); break;
                    case "Kicked": SetConnectionState(false, packet.Text); AddSystemText(packet.Text); MessageBox.Show(packet.Text, "قطع اتصال توسط مدیر", MessageBoxButtons.OK, MessageBoxIcon.Information); break;
                    case "Error": AddSystemText(packet.Text); break;
                }
            });
        }

        private ChatMessageInfo FromPacket(ChatPacket p)
        {
            return new ChatMessageInfo { MessageId = p.MessageId, SenderName = p.UserName, SenderAvatar = p.Avatar,
                Text = p.Text, TimestampTicks = p.TimestampTicks, ReplyToId = p.ReplyToId, ReplyToText = p.ReplyToText,
                ReplyToUser = p.ReplyToUser, IsEdited = p.IsEdited, IsDeleted = p.IsDeleted, IsServer = p.IsServer, Reactions = p.Reactions };
        }

        private void Send(ChatPacket packet)
        {
            try
            {
                if (!_connected || _stream == null) return;
                FrameProtocol.Send(_stream, packet, _writeLock);
            }
            catch { SetConnectionState(false, "ارسال به سرور ناموفق بود."); }
        }

        private void SetConnectionState(bool connected, string message)
        {
            _connected = connected; _status.Text = (connected ? "● " : "● ") + message;
            _status.ForeColor = connected ? Color.FromArgb(34, 197, 94) : Color.FromArgb(239, 100, 100);
            if (_reconnect != null) _reconnect.Enabled = !connected;
            if (_send != null) _send.Enabled = connected;
            if (!connected) { try { if (_client != null) _client.Close(); } catch { } _stream = null; }
        }

        private void Reconnect_Click(object sender, EventArgs e)
        {
            if (_connected) return;
            // اتصال مجدد با همان نام و آیکون انجام می‌شود و فرم ورود دوباره باز نخواهد شد.
            TcpClient client = null;
            try
            {
                client = new TcpClient { ReceiveTimeout = 7000, SendTimeout = 7000 };
                client.Connect(_host, _port);
                NetworkStream stream = client.GetStream();
                stream.ReadTimeout = 7000; stream.WriteTimeout = 7000;
                FrameProtocol.Send(stream, new ChatPacket { Type = "Join", UserName = _userName, Avatar = _avatar }, new object());
                ChatPacket welcome = FrameProtocol.Receive(stream);
                if (welcome == null || welcome.Type != "Welcome")
                    throw new InvalidOperationException(welcome == null ? "پاسخی از سرور دریافت نشد." : welcome.Text);
                // پس از Handshake، انتظار شبکه نامحدود می‌شود تا چت در زمان سکوت هم متصل بماند.
                stream.ReadTimeout = Timeout.Infinite; stream.WriteTimeout = Timeout.Infinite; client.ReceiveTimeout = 0; client.SendTimeout = 0;
                _client = client; _stream = stream; _connected = true;
                SetConnectionState(true, "متصل شد"); ProcessWelcome(welcome); StartReceiver();
            }
            catch (Exception ex)
            {
                if (client != null) try { client.Close(); } catch { }
                SetConnectionState(false, "اتصال برقرار نشد.");
                MessageBox.Show("اتصال مجدد ناموفق بود:\n" + ex.Message);
            }
        }
        #endregion

        #region ارسال، ویرایش، حذف، پاسخ و وضعیت پیام
        private void SendMessage_Click(object sender, EventArgs e)
        {
            string text = (_composer.Text ?? "").Trim(); if (text.Length == 0 || !_connected) return;
            if (text.Length > 4000) { MessageBox.Show("متن پیام نباید بیشتر از ۴۰۰۰ نویسه باشد."); return; }
            if (!string.IsNullOrEmpty(_editingId))
            {
                // فقط سرور اجازه ویرایش پیام را پس از بررسی مالکیت آن ثبت و برای بقیه منتشر می‌کند.
                Send(new ChatPacket { Type = "EditMessage", MessageId = _editingId, Text = text });
                var old = _messages[_editingId]; old.Text = text; old.IsEdited = true; old.TimestampTicks = DateTime.Now.Ticks; UpsertMessage(old, false);
                _editingId = null; _send.Text = "ارسال  ➤";
            }
            else
            {
                string id = Guid.NewGuid().ToString("N");
                var m = new ChatMessageInfo { MessageId = id, SenderName = _userName, SenderAvatar = _avatar, Text = text,
                    TimestampTicks = DateTime.Now.Ticks, ReplyToId = _replyId, ReplyToText = _replyText, ReplyToUser = _replyUser };
                // پیام ابتدا با یک تیک در رابط دیده می‌شود؛ پس از تایید سرور به دو تیک تبدیل می‌شود.
                _statuses[id] = 0; UpsertMessage(m, true);
                Send(new ChatPacket { Type = "Chat", MessageId = id, Text = text, ReplyToId = _replyId, ReplyToText = _replyText, ReplyToUser = _replyUser });
            }
            _composer.Clear(); ClearReply();
            if (_typingSent) { Send(new ChatPacket { Type = "Typing", Text = "0" }); _typingSent = false; }
        }

        private void Composer_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter پیام را می‌فرستد و Shift+Enter برای نوشتن چندخطی باقی می‌ماند.
            if (e.KeyCode == Keys.Enter && !e.Shift) { e.SuppressKeyPress = true; SendMessage_Click(sender, EventArgs.Empty); }
        }
        private void Composer_TextChanged(object sender, EventArgs e)
        {
            if (!_connected || _composer.TextLength == 0) return;
            if (!_typingSent) { Send(new ChatPacket { Type = "Typing", Text = "1" }); _typingSent = true; }
            _typingTimer.Stop(); _typingTimer.Start();
        }

        private void BeginReply(ChatMessageInfo message)
        {
            _replyId = message.MessageId; _replyText = message.Text; _replyUser = message.SenderName;
            _replyLabel.Text = "پاسخ به " + message.SenderName + " : " + Short(message.Text, 75);
            _replyPanel.Visible = true; _composer.Focus();
        }
        private void ClearReply()
        {
            _replyId = null; _replyText = null; _replyUser = null;
            // دکمه × هم نقل‌قول و هم حالت ویرایش را لغو می‌کند تا متن بعدی اشتباهاً جای پیام قبلی ننشیند.
            _editingId = null; if (_send != null) _send.Text = "ارسال  ➤";
            if (_replyPanel != null) _replyPanel.Visible = false;
        }

        private void EditOwnMessage(ChatMessageInfo message)
        {
            _editingId = message.MessageId; _composer.Text = message.Text; _composer.Focus(); _composer.SelectionStart = _composer.TextLength;
            _send.Text = "ذخیره تغییر"; _replyPanel.Visible = true; _replyLabel.Text = "در حال ویرایش پیام خودت هستی";
        }
        private void DeleteOwnMessage(ChatMessageInfo message)
        {
            if (MessageBox.Show("این پیام برای همه حذف شود؟", "حذف پیام", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Send(new ChatPacket { Type = "DeleteMessage", MessageId = message.MessageId });
        }
        private void ReactToMessage(string messageId, string emoji)
        { Send(new ChatPacket { Type = "Reaction", MessageId = messageId, Reaction = emoji }); }

        private void ShowEmojiPanel(object sender, EventArgs e)
        {
            // پنل ساده ایموجی با کلیک روی هر گزینه، همان ایموجی را به متن در حال نوشتن اضافه می‌کند.
            var menu = new ContextMenuStrip(); string[] emojis = { "😀", "😂", "😍", "🥳", "❤️", "👍", "🔥", "🙏", "😎", "🤔", "🎉", "👏" };
            foreach (string emoji in emojis)
            {
                ToolStripItem item = menu.Items.Add(emoji); item.Font = new Font("Segoe UI Emoji", 14);
                item.Click += delegate { _composer.AppendText(emoji); _composer.Focus(); };
            }
            menu.Show(_emoji, new Point(0, -menu.Height));
        }
        #endregion

        #region ساخت حباب پیام، واکنش‌ها و رسید خواندن
        private void UpsertMessage(ChatMessageInfo message, bool scrollToEnd)
        {
            if (message == null) return;
            if (string.IsNullOrEmpty(message.MessageId)) message.MessageId = Guid.NewGuid().ToString("N");
            int oldIndex = -1; Panel oldCard;
            if (_messageCards.TryGetValue(message.MessageId, out oldCard))
            {
                oldIndex = _messageList.Controls.IndexOf(oldCard); _messageList.Controls.Remove(oldCard); oldCard.Dispose();
            }
            _messages[message.MessageId] = message;
            Panel card = BuildMessageCard(message); _messageCards[message.MessageId] = card;
            _messageList.Controls.Add(card);
            if (oldIndex >= 0 && oldIndex < _messageList.Controls.Count) _messageList.Controls.SetChildIndex(card, oldIndex);
            ResizeMessageCards();
            if (scrollToEnd) _messageList.ScrollControlIntoView(card);
        }

        private Panel BuildMessageCard(ChatMessageInfo message)
        {
            int fullWidth = Math.Max(420, _messageList.ClientSize.Width - 35);
            bool mine = message.SenderName == _userName;
            bool system = message.SenderName == "سیستم";
            int width = Math.Min(650, Math.Max(300, fullWidth - 35));
            Font bodyFont = new Font("Segoe UI", 10.5f);
            string bodyText = message.Text ?? "";
            Size bodySize = TextRenderer.MeasureText(bodyText, bodyFont, new Size(width - 34, 700), TextFormatFlags.WordBreak | TextFormatFlags.RightToLeft);
            int bodyHeight = Math.Max(24, bodySize.Height + 5);
            bool hasReply = !string.IsNullOrEmpty(message.ReplyToId) || !string.IsNullOrEmpty(message.ReplyToText);
            int replyHeight = hasReply ? 38 : 0;
            int actionsHeight = system ? 0 : 34;
            int cardHeight = 36 + replyHeight + bodyHeight + actionsHeight + 20;
            var outer = new Panel { Width = fullWidth, Height = cardHeight + 8, Margin = new Padding(0, 4, 0, 6), BackColor = _dark ? Color.FromArgb(11, 18, 32) : Color.FromArgb(241, 245, 249), Tag = message.MessageId };
            Color cardColor = system ? (_dark ? Color.FromArgb(45, 58, 78) : Color.FromArgb(226, 232, 240)) :
                message.IsServer ? (_dark ? Color.FromArgb(49, 57, 87) : Color.FromArgb(224, 231, 255)) :
                mine ? (_dark ? Color.FromArgb(29, 78, 150) : Color.FromArgb(219, 234, 254)) :
                (_dark ? Color.FromArgb(27, 39, 59) : Color.White);
            Color textColor = _dark ? Color.FromArgb(239, 244, 252) : Color.FromArgb(30, 41, 59);
            if (!_dark && mine) textColor = Color.FromArgb(20, 45, 80);
            var card = new Panel { Width = width, Height = cardHeight, BackColor = cardColor, Padding = new Padding(12), Location = new Point(mine ? fullWidth - width - 6 : 6, 2) };
            outer.Controls.Add(card);

            // سربرگ هر پیام همیشه نام، آواتار و زمان ارسال را نمایش می‌دهد.
            string time = SafeTime(message.TimestampTicks);
            string edited = message.IsEdited ? "  •  ویرایش‌شده" : "";
            var head = new Label { Text = (message.SenderAvatar ?? "🙂") + "  " + message.SenderName + "     " + time + edited,
                Dock = DockStyle.Top, Height = 27, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = _dark ? Color.FromArgb(190, 205, 225) : Color.FromArgb(71, 85, 105) };
            card.Controls.Add(head);

            int y = 32;
            if (hasReply)
            {
                var reply = new Label { Text = "↪ پاسخ به " + (message.ReplyToUser ?? "پیام") + "  |  " + Short(message.ReplyToText, 65),
                    Location = new Point(10, y), Size = new Size(width - 24, 30), TextAlign = ContentAlignment.MiddleRight,
                    BackColor = _dark ? Color.FromArgb(38, 55, 78) : Color.FromArgb(241, 245, 249), Cursor = Cursors.Hand,
                    ForeColor = textColor, Font = new Font("Segoe UI", 8.5f, FontStyle.Italic) };
                reply.Click += delegate { JumpToMessage(message.ReplyToId); }; card.Controls.Add(reply); y += replyHeight;
            }

            var body = new Label { Text = bodyText, Location = new Point(12, y), Size = new Size(width - 28, bodyHeight),
                TextAlign = ContentAlignment.TopRight, RightToLeft = RightToLeft.Yes, Font = bodyFont, ForeColor = message.IsDeleted ? Color.Gray : textColor };
            card.Controls.Add(body); y += bodyHeight + 3;

            if (!system)
            {
                var actions = new FlowLayoutPanel { Location = new Point(8, y), Size = new Size(width - 16, 32), FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false, AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0) };
                string[] emojis = { "❤️", "😂", "👍", "🔥" };
                foreach (string emoji in emojis)
                {
                    int count = ReactionCount(message, emoji);
                    Button reaction = SmallAction(emoji + (count > 0 ? " " + count : ""), 52);
                    reaction.Font = new Font("Segoe UI Emoji", 9); reaction.Click += delegate { ReactToMessage(message.MessageId, emoji); }; actions.Controls.Add(reaction);
                }
                Button replyButton = SmallAction("↩ پاسخ", 64); replyButton.Click += delegate { BeginReply(message); }; actions.Controls.Add(replyButton);
                if (mine && !message.IsDeleted)
                {
                    Button editButton = SmallAction("ویرایش", 58); editButton.Click += delegate { EditOwnMessage(message); }; actions.Controls.Add(editButton);
                    Button deleteButton = SmallAction("حذف", 48); deleteButton.Click += delegate { DeleteOwnMessage(message); }; actions.Controls.Add(deleteButton);
                }
                card.Controls.Add(actions);

                // یک تیک یعنی ارسال شده، دو تیک یعنی به حداقل یک گیرنده رسیده و سه تیک یعنی توسط کاربری خوانده شده است.
                if (mine)
                {
                    int state; _statuses.TryGetValue(message.MessageId, out state);
                    string ticks = state >= 2 ? "✓✓✓" : state == 1 ? "✓✓" : "✓";
                    Color tickColor = state >= 2 ? Color.FromArgb(34, 197, 94) : state == 1 ? Color.FromArgb(96, 190, 255) : Color.Gray;
                    var status = new Label { Text = ticks, AutoSize = true, Location = new Point(width - 58, cardHeight - 18), ForeColor = tickColor, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
                    card.Controls.Add(status); status.BringToFront();
                }
            }
            return outer;
        }

        private Button SmallAction(string text, int width)
        {
            return new Button { Text = text, Width = width, Height = 27, Margin = new Padding(2, 0, 2, 0), FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8), Cursor = Cursors.Hand, BackColor = _dark ? Color.FromArgb(43, 58, 80) : Color.FromArgb(241, 245, 249),
                ForeColor = _dark ? Color.White : Color.FromArgb(51, 65, 85) };
        }
        private int ReactionCount(ChatMessageInfo message, string emoji)
        {
            if (message.Reactions == null) return 0;
            ReactionCounter item = message.Reactions.FirstOrDefault(x => x.Emoji == emoji); return item == null ? 0 : item.Count;
        }
        private string SafeTime(long ticks)
        { try { return new DateTime(ticks).ToString("HH:mm"); } catch { return DateTime.Now.ToString("HH:mm"); } }
        private string Short(string text, int max)
        { if (string.IsNullOrEmpty(text) || text.Length <= max) return text ?? ""; return text.Substring(0, max) + "…"; }
        private void ResizeMessageCards()
        {
            if (_messageList == null) return;
            foreach (Control c in _messageList.Controls)
            {
                Panel outer = c as Panel; if (outer == null || outer.Controls.Count == 0) continue;
                Panel card = outer.Controls[0] as Panel; if (card == null) continue;
                outer.Width = Math.Max(420, _messageList.ClientSize.Width - 35);
                int width = Math.Min(650, Math.Max(300, outer.Width - 35));
                card.Width = width; bool mine = false; ChatMessageInfo m;
                if (_messages.TryGetValue(Convert.ToString(outer.Tag), out m)) mine = m.SenderName == _userName;
                card.Left = mine ? outer.Width - width - 6 : 6;
            }
        }
        private void RefreshMessage(string id)
        { ChatMessageInfo m; if (_messages.TryGetValue(id, out m)) UpsertMessage(m, false); }
        private void JumpToMessage(string id)
        {
            if (string.IsNullOrEmpty(id)) return; Panel panel;
            if (_messageCards.TryGetValue(id, out panel))
            {
                _messageList.ScrollControlIntoView(panel);
                if (panel.Controls.Count > 0) panel.Controls[0].BackColor = _dark ? Color.FromArgb(48, 82, 120) : Color.FromArgb(219, 234, 254);
            }
        }
        private void SetPinned(string id, string user, string text)
        {
            _pinnedId = id; _pinPanel.Visible = !string.IsNullOrEmpty(id);
            _pinLabel.Text = string.IsNullOrEmpty(id) ? "" : "📌 پیام سنجاق‌شده از " + user + " : " + Short(text, 100);
        }
        private void UpdateUsers(UserInfo[] users)
        {
            _userList.Controls.Clear(); if (users == null) return;
            foreach (UserInfo user in users)
            {
                var row = new Panel { Width = Math.Max(190, _userList.ClientSize.Width - 30), Height = 48, Margin = new Padding(2), BackColor = _dark ? Color.FromArgb(27, 39, 59) : Color.FromArgb(248, 250, 252) };
                row.Controls.Add(new Label { Text = user.Avatar, Font = new Font("Segoe UI Emoji", 20), AutoSize = true, Location = new Point(8, 5) });
                row.Controls.Add(new Label { Text = user.UserName + (user.UserName == _userName ? " (شما)" : ""), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Location = new Point(55, 14) });
                _userList.Controls.Add(row);
            }
        }
        private void AddSystemText(string text)
        { UpsertMessage(new ChatMessageInfo { MessageId = Guid.NewGuid().ToString("N"), SenderName = "سیستم", SenderAvatar = "⚙️", Text = text, TimestampTicks = DateTime.Now.Ticks, IsServer = true }, true); }
        #endregion

        #region نشانگر تایپ، اعلان صوتی و وضعیت شبکه
        private void ShowTyping(string user, bool typing)
        {
            if (string.IsNullOrEmpty(user) || user == _userName) return;
            _remoteTypingTimer.Stop();
            _typingLabel.Text = typing ? user + " در حال تایپ است…" : "";
            if (typing) _remoteTypingTimer.Start();
        }
        private void NotifyIncoming(ChatMessageInfo message)
        {
            // صدای اعلان برای پیام جدید پخش می‌شود؛ در حالت پس‌زمینه آیکون برنامه نیز در Taskbar چشمک می‌زند.
            try { SystemSounds.Asterisk.Play(); } catch { }
            if (WindowState == FormWindowState.Minimized || !ContainsFocus)
            {
                try
                {
                    FlashInfo info = new FlashInfo(); info.Size = (uint)Marshal.SizeOf(typeof(FlashInfo)); info.Handle = Handle; info.Flags = 3; info.Count = 4; info.Timeout = 0;
                    FlashWindowEx(ref info);
                    if (_notify != null) { _notify.BalloonTipTitle = "پیام جدید از " + message.SenderName; _notify.BalloonTipText = Short(message.Text, 100); _notify.BalloonTipIcon = ToolTipIcon.Info; _notify.ShowBalloonTip(2500); }
                }
                catch { }
            }
        }
        private void MarkVisibleMessagesRead()
        {
            if (WindowState == FormWindowState.Minimized || !ContainsFocus) return;
            foreach (string id in _readPending.ToArray())
            {
                Send(new ChatPacket { Type = "ReadReceipt", MessageId = id }); _readPending.Remove(id);
            }
        }
                [StructLayout(LayoutKind.Sequential)]
        private struct FlashInfo
        {
            public uint Size; public IntPtr Handle; public uint Flags; public uint Count; public uint Timeout;
        }
        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FlashInfo info);
        #endregion

        #region تم، اتصال مجدد و بستن امن فرم
        private void ApplyTheme()
        {
            bool dark = ThemeSettings.Dark; _dark = dark;
            Color back = dark ? Color.FromArgb(10, 16, 29) : Color.FromArgb(239, 244, 250);
            Color card = dark ? Color.FromArgb(20, 30, 48) : Color.White;
            Color fore = dark ? Color.FromArgb(235, 241, 250) : Color.FromArgb(30, 41, 59);
            BackColor = back; ForeColor = fore; ThemeControls(this, card, fore, dark);
            _rootPanel.BackColor = back;
            _headerPanel.BackColor = card;
            _sidebarPanel.BackColor = card;
            _brandLabel.ForeColor = dark ? Color.FromArgb(147, 197, 253) : Color.FromArgb(37, 99, 235);
            _theme.BackColor = Color.FromArgb(37, 99, 235); _theme.ForeColor = Color.White;
            _reconnect.BackColor = dark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(226, 232, 240);
            _reconnect.ForeColor = dark ? Color.White : Color.FromArgb(30, 41, 59);
            _emoji.BackColor = dark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(226, 232, 240);
            _emoji.ForeColor = dark ? Color.White : Color.FromArgb(30, 41, 59);
            _cancelReply.BackColor = dark ? Color.FromArgb(71, 85, 105) : Color.FromArgb(203, 213, 225);
            _cancelReply.ForeColor = dark ? Color.White : Color.FromArgb(30, 41, 59);
            _send.BackColor = Color.FromArgb(37, 99, 235); _send.ForeColor = Color.White;
            if (_messageList != null)
            {
                _messageList.BackColor = back;
                foreach (ChatMessageInfo m in _messages.Values.ToArray()) UpsertMessage(m, false);
            }
            if (_userList != null) _userList.BackColor = card;
            if (_pinPanel != null) { _pinPanel.BackColor = dark ? Color.FromArgb(34, 55, 82) : Color.FromArgb(219, 234, 254); _pinLabel.ForeColor = fore; }
            if (_replyPanel != null) { _replyPanel.BackColor = dark ? Color.FromArgb(31, 45, 67) : Color.FromArgb(226, 232, 240); _replyLabel.ForeColor = fore; }
        }
        private void ThemeControls(Control parent, Color card, Color fore, bool dark)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBox) { c.BackColor = dark ? Color.FromArgb(12, 21, 36) : Color.FromArgb(248, 250, 252); c.ForeColor = fore; }
                else if (c is Button) { c.BackColor = Color.FromArgb(37, 99, 235); c.ForeColor = Color.White; }
                else if (c is Panel || c is TableLayoutPanel || c is SplitContainer || c is FlowLayoutPanel) { c.BackColor = card; c.ForeColor = fore; }
                else { c.ForeColor = fore; }
                ThemeControls(c, card, fore, dark);
            }
        }
        private void Ui(Action action)
        {
            try { if (IsDisposed || !IsHandleCreated) return; if (InvokeRequired) BeginInvoke(action); else action(); }
            catch (InvalidOperationException) { }
        }
        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            _closing = true; _connected = false;
            if (_typingTimer != null) { _typingTimer.Stop(); _typingTimer.Dispose(); }
            if (_remoteTypingTimer != null) { _remoteTypingTimer.Stop(); _remoteTypingTimer.Dispose(); }
            try { if (_notify != null) { _notify.Visible = false; _notify.Dispose(); } } catch { }
            try { if (_client != null) _client.Close(); } catch { }
        }
        #endregion
    }
}
