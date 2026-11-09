using System.Drawing;
using System.Windows.Forms;

namespace ChatpApp.Client
{
    // چیدمان فرم گفتگو در این فایل قرار دارد تا پنجره در حالت Design خالی نمایش داده نشود.
    public sealed partial class ChatGroup
    {
        private System.ComponentModel.IContainer components = null;

        // منابع و اجزای احتمالی فرم هنگام بسته‌شدن آزاد می‌شوند.
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private Panel _rootPanel;
        private TableLayoutPanel _rootLayout;
        private Panel _headerPanel;
        private Label _brandLabel;
        private Label _currentUserLabel;
        private Label _status;
        private Button _reconnect;
        private Button _theme;
        private Panel _pinPanel;
        private Label _pinLabel;
        private SplitContainer _mainSplit;
        private TableLayoutPanel _chatLayout;
        private FlowLayoutPanel _messageList;
        private Label _typingLabel;
        private Panel _replyPanel;
        private Label _replyLabel;
        private Button _cancelReply;
        private System.Windows.Forms.TableLayoutPanel _composeLayout;
        private System.Windows.Forms.Button _emoji;
        private System.Windows.Forms.TextBox _composer;
        private System.Windows.Forms.Button _send;
        private Panel _sidebarPanel;
        private TableLayoutPanel _sidebarLayout;
        private System.Windows.Forms.Label _membersTitle;
        private FlowLayoutPanel _userList;
        private Panel _designMessageSample;
        private Panel _sampleBubble;
        private Label _sampleMessageName;
        private Label _sampleMessageText;
        private Label _sampleMessageTime;
        private Panel _designUserSample;
        private Label _sampleUserName;
        private Label _sampleUserStatus;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._rootPanel = new System.Windows.Forms.Panel();
            this._rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this._headerPanel = new System.Windows.Forms.Panel();
            this._theme = new System.Windows.Forms.Button();
            this._reconnect = new System.Windows.Forms.Button();
            this._status = new System.Windows.Forms.Label();
            this._currentUserLabel = new System.Windows.Forms.Label();
            this._brandLabel = new System.Windows.Forms.Label();
            this._pinPanel = new System.Windows.Forms.Panel();
            this._pinLabel = new System.Windows.Forms.Label();
            this._mainSplit = new System.Windows.Forms.SplitContainer();
            this._chatLayout = new System.Windows.Forms.TableLayoutPanel();
            this._messageList = new System.Windows.Forms.FlowLayoutPanel();
            this._designMessageSample = new System.Windows.Forms.Panel();
            this._sampleBubble = new System.Windows.Forms.Panel();
            this._sampleMessageText = new System.Windows.Forms.Label();
            this._sampleMessageName = new System.Windows.Forms.Label();
            this._sampleMessageTime = new System.Windows.Forms.Label();
            this._typingLabel = new System.Windows.Forms.Label();
            this._replyPanel = new System.Windows.Forms.Panel();
            this._replyLabel = new System.Windows.Forms.Label();
            this._cancelReply = new System.Windows.Forms.Button();
            this._composeLayout = new System.Windows.Forms.TableLayoutPanel();
            this._emoji = new System.Windows.Forms.Button();
            this._composer = new System.Windows.Forms.TextBox();
            this._send = new System.Windows.Forms.Button();
            this._sidebarPanel = new System.Windows.Forms.Panel();
            this._sidebarLayout = new System.Windows.Forms.TableLayoutPanel();
            this._membersTitle = new System.Windows.Forms.Label();
            this._userList = new System.Windows.Forms.FlowLayoutPanel();
            this._designUserSample = new System.Windows.Forms.Panel();
            this._sampleUserName = new System.Windows.Forms.Label();
            this._sampleUserStatus = new System.Windows.Forms.Label();
            this._rootPanel.SuspendLayout();
            this._rootLayout.SuspendLayout();
            this._headerPanel.SuspendLayout();
            this._pinPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._mainSplit)).BeginInit();
            this._mainSplit.Panel1.SuspendLayout();
            this._mainSplit.Panel2.SuspendLayout();
            this._mainSplit.SuspendLayout();
            this._chatLayout.SuspendLayout();
            this._messageList.SuspendLayout();
            this._designMessageSample.SuspendLayout();
            this._sampleBubble.SuspendLayout();
            this._replyPanel.SuspendLayout();
            this._composeLayout.SuspendLayout();
            this._sidebarPanel.SuspendLayout();
            this._sidebarLayout.SuspendLayout();
            this._userList.SuspendLayout();
            this._designUserSample.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootPanel
            // 
            this._rootPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(16)))), ((int)(((byte)(29)))));
            this._rootPanel.Controls.Add(this._rootLayout);
            this._rootPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rootPanel.Location = new System.Drawing.Point(0, 0);
            this._rootPanel.Name = "_rootPanel";
            this._rootPanel.Padding = new System.Windows.Forms.Padding(16);
            this._rootPanel.Size = new System.Drawing.Size(1280, 800);
            this._rootPanel.TabIndex = 0;
            // 
            // _rootLayout
            // 
            this._rootLayout.ColumnCount = 1;
            this._rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._rootLayout.Controls.Add(this._headerPanel, 0, 0);
            this._rootLayout.Controls.Add(this._pinPanel, 0, 1);
            this._rootLayout.Controls.Add(this._mainSplit, 0, 2);
            this._rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rootLayout.Location = new System.Drawing.Point(16, 16);
            this._rootLayout.Name = "_rootLayout";
            this._rootLayout.RowCount = 3;
            this._rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this._rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this._rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._rootLayout.Size = new System.Drawing.Size(1248, 768);
            this._rootLayout.TabIndex = 0;
            // 
            // _headerPanel
            // 
            this._headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(30)))), ((int)(((byte)(48)))));
            this._headerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._headerPanel.Controls.Add(this._theme);
            this._headerPanel.Controls.Add(this._reconnect);
            this._headerPanel.Controls.Add(this._status);
            this._headerPanel.Controls.Add(this._currentUserLabel);
            this._headerPanel.Controls.Add(this._brandLabel);
            this._headerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._headerPanel.Location = new System.Drawing.Point(3, 3);
            this._headerPanel.Name = "_headerPanel";
            this._headerPanel.Padding = new System.Windows.Forms.Padding(18, 10, 16, 10);
            this._headerPanel.Size = new System.Drawing.Size(1242, 72);
            this._headerPanel.TabIndex = 0;
            // 
            // _theme
            // 
            this._theme.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._theme.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this._theme.Cursor = System.Windows.Forms.Cursors.Hand;
            this._theme.FlatAppearance.BorderSize = 0;
            this._theme.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._theme.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._theme.ForeColor = System.Drawing.Color.White;
            this._theme.Location = new System.Drawing.Point(2155, 14);
            this._theme.Name = "_theme";
            this._theme.Size = new System.Drawing.Size(100, 38);
            this._theme.TabIndex = 0;
            this._theme.Text = "☀  پوسته";
            this._theme.UseVisualStyleBackColor = false;
            // 
            // _reconnect
            // 
            this._reconnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._reconnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this._reconnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this._reconnect.FlatAppearance.BorderSize = 0;
            this._reconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._reconnect.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._reconnect.ForeColor = System.Drawing.Color.White;
            this._reconnect.Location = new System.Drawing.Point(2040, 14);
            this._reconnect.Name = "_reconnect";
            this._reconnect.Size = new System.Drawing.Size(108, 38);
            this._reconnect.TabIndex = 1;
            this._reconnect.Text = "اتصال مجدد";
            this._reconnect.UseVisualStyleBackColor = false;
            // 
            // _status
            // 
            this._status.AutoSize = true;
            this._status.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._status.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(197)))), ((int)(((byte)(94)))));
            this._status.Location = new System.Drawing.Point(430, 24);
            this._status.Name = "_status";
            this._status.Size = new System.Drawing.Size(61, 20);
            this._status.TabIndex = 2;
            this._status.Text = "● متصل";
            // 
            // _currentUserLabel
            // 
            this._currentUserLabel.AutoSize = true;
            this._currentUserLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._currentUserLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this._currentUserLabel.Location = new System.Drawing.Point(250, 22);
            this._currentUserLabel.Name = "_currentUserLabel";
            this._currentUserLabel.Size = new System.Drawing.Size(114, 23);
            this._currentUserLabel.TabIndex = 3;
            this._currentUserLabel.Text = "🙂  نام کاربری";
            // 
            // _brandLabel
            // 
            this._brandLabel.AutoSize = true;
            this._brandLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this._brandLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(197)))), ((int)(((byte)(253)))));
            this._brandLabel.Location = new System.Drawing.Point(18, 17);
            this._brandLabel.Name = "_brandLabel";
            this._brandLabel.Size = new System.Drawing.Size(205, 37);
            this._brandLabel.TabIndex = 4;
            this._brandLabel.Text = "✦  NEXA CHAT";
            // 
            // _pinPanel
            // 
            this._pinPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(55)))), ((int)(((byte)(82)))));
            this._pinPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pinPanel.Controls.Add(this._pinLabel);
            this._pinPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._pinPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pinPanel.Location = new System.Drawing.Point(3, 81);
            this._pinPanel.Name = "_pinPanel";
            this._pinPanel.Padding = new System.Windows.Forms.Padding(14, 5, 14, 5);
            this._pinPanel.Size = new System.Drawing.Size(1242, 42);
            this._pinPanel.TabIndex = 1;
            // 
            // _pinLabel
            // 
            this._pinLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pinLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this._pinLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(234)))), ((int)(((byte)(254)))));
            this._pinLabel.Location = new System.Drawing.Point(14, 5);
            this._pinLabel.Name = "_pinLabel";
            this._pinLabel.Size = new System.Drawing.Size(1212, 30);
            this._pinLabel.TabIndex = 0;
            this._pinLabel.Text = "📌  پیام سنجاق‌شده نمونه؛ برای رفتن به پیام کلیک کن";
            this._pinLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _mainSplit
            // 
            this._mainSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mainSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this._mainSplit.Location = new System.Drawing.Point(3, 129);
            this._mainSplit.Name = "_mainSplit";
            // 
            // _mainSplit.Panel1
            // 
            this._mainSplit.Panel1.Controls.Add(this._chatLayout);
            this._mainSplit.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this._mainSplit.Panel1MinSize = 650;
            // 
            // _mainSplit.Panel2
            // 
            this._mainSplit.Panel2.Controls.Add(this._sidebarPanel);
            this._mainSplit.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this._mainSplit.Panel2MinSize = 230;
            this._mainSplit.Size = new System.Drawing.Size(1242, 636);
            this._mainSplit.SplitterDistance = 972;
            this._mainSplit.TabIndex = 2;
            // 
            // _chatLayout
            // 
            this._chatLayout.ColumnCount = 1;
            this._chatLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._chatLayout.Controls.Add(this._messageList, 0, 0);
            this._chatLayout.Controls.Add(this._typingLabel, 0, 1);
            this._chatLayout.Controls.Add(this._replyPanel, 0, 2);
            this._chatLayout.Controls.Add(this._composeLayout, 0, 3);
            this._chatLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._chatLayout.Location = new System.Drawing.Point(0, 0);
            this._chatLayout.Name = "_chatLayout";
            this._chatLayout.Padding = new System.Windows.Forms.Padding(8, 8, 8, 4);
            this._chatLayout.RowCount = 4;
            this._chatLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._chatLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this._chatLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this._chatLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this._chatLayout.Size = new System.Drawing.Size(972, 636);
            this._chatLayout.TabIndex = 0;
            // 
            // _messageList
            // 
            this._messageList.AutoScroll = true;
            this._messageList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(21)))), ((int)(((byte)(36)))));
            this._messageList.Controls.Add(this._designMessageSample);
            this._messageList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._messageList.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this._messageList.Location = new System.Drawing.Point(11, 11);
            this._messageList.Name = "_messageList";
            this._messageList.Padding = new System.Windows.Forms.Padding(12);
            this._messageList.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this._messageList.Size = new System.Drawing.Size(950, 448);
            this._messageList.TabIndex = 0;
            this._messageList.WrapContents = false;
            // 
            // _designMessageSample
            // 
            this._designMessageSample.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(21)))), ((int)(((byte)(36)))));
            this._designMessageSample.Controls.Add(this._sampleBubble);
            this._designMessageSample.Location = new System.Drawing.Point(223, 16);
            this._designMessageSample.Margin = new System.Windows.Forms.Padding(3, 4, 3, 8);
            this._designMessageSample.Name = "_designMessageSample";
            this._designMessageSample.Size = new System.Drawing.Size(700, 118);
            this._designMessageSample.TabIndex = 0;
            // 
            // _sampleBubble
            // 
            this._sampleBubble.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(43)))), ((int)(((byte)(65)))));
            this._sampleBubble.Controls.Add(this._sampleMessageText);
            this._sampleBubble.Controls.Add(this._sampleMessageName);
            this._sampleBubble.Controls.Add(this._sampleMessageTime);
            this._sampleBubble.Location = new System.Drawing.Point(265, 4);
            this._sampleBubble.Name = "_sampleBubble";
            this._sampleBubble.Padding = new System.Windows.Forms.Padding(12);
            this._sampleBubble.Size = new System.Drawing.Size(420, 104);
            this._sampleBubble.TabIndex = 0;
            // 
            // _sampleMessageText
            // 
            this._sampleMessageText.AutoSize = true;
            this._sampleMessageText.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._sampleMessageText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this._sampleMessageText.Location = new System.Drawing.Point(12, 43);
            this._sampleMessageText.Name = "_sampleMessageText";
            this._sampleMessageText.Size = new System.Drawing.Size(305, 23);
            this._sampleMessageText.TabIndex = 0;
            this._sampleMessageText.Text = "سلام! به گفتگوی گروهی خوش اومدی 👋";
            // 
            // _sampleMessageName
            // 
            this._sampleMessageName.AutoSize = true;
            this._sampleMessageName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._sampleMessageName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(197)))), ((int)(((byte)(253)))));
            this._sampleMessageName.Location = new System.Drawing.Point(12, 12);
            this._sampleMessageName.Name = "_sampleMessageName";
            this._sampleMessageName.Size = new System.Drawing.Size(81, 20);
            this._sampleMessageName.TabIndex = 1;
            this._sampleMessageName.Text = "مریم  ·  👩🏻‍🎨";
            // 
            // _sampleMessageTime
            // 
            this._sampleMessageTime.AutoSize = true;
            this._sampleMessageTime.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._sampleMessageTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this._sampleMessageTime.Location = new System.Drawing.Point(330, 78);
            this._sampleMessageTime.Name = "_sampleMessageTime";
            this._sampleMessageTime.Size = new System.Drawing.Size(64, 19);
            this._sampleMessageTime.TabIndex = 2;
            this._sampleMessageTime.Text = "۱۰:۴۲  ✓✓";
            // 
            // _typingLabel
            // 
            this._typingLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._typingLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this._typingLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(165)))), ((int)(((byte)(250)))));
            this._typingLabel.Location = new System.Drawing.Point(11, 462);
            this._typingLabel.Name = "_typingLabel";
            this._typingLabel.Size = new System.Drawing.Size(950, 30);
            this._typingLabel.TabIndex = 1;
            this._typingLabel.Text = "مریم در حال تایپ است…";
            this._typingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _replyPanel
            // 
            this._replyPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(45)))), ((int)(((byte)(67)))));
            this._replyPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._replyPanel.Controls.Add(this._replyLabel);
            this._replyPanel.Controls.Add(this._cancelReply);
            this._replyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._replyPanel.Location = new System.Drawing.Point(11, 495);
            this._replyPanel.Name = "_replyPanel";
            this._replyPanel.Padding = new System.Windows.Forms.Padding(8, 5, 8, 5);
            this._replyPanel.Size = new System.Drawing.Size(950, 46);
            this._replyPanel.TabIndex = 2;
            // 
            // _replyLabel
            // 
            this._replyLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._replyLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._replyLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this._replyLabel.Location = new System.Drawing.Point(44, 5);
            this._replyLabel.Name = "_replyLabel";
            this._replyLabel.Size = new System.Drawing.Size(896, 34);
            this._replyLabel.TabIndex = 0;
            this._replyLabel.Text = "در پاسخ به مریم: سلام! به گفتگوی گروهی خوش اومدی";
            this._replyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _cancelReply
            // 
            this._cancelReply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this._cancelReply.Dock = System.Windows.Forms.DockStyle.Left;
            this._cancelReply.FlatAppearance.BorderSize = 0;
            this._cancelReply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._cancelReply.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this._cancelReply.ForeColor = System.Drawing.Color.White;
            this._cancelReply.Location = new System.Drawing.Point(8, 5);
            this._cancelReply.Name = "_cancelReply";
            this._cancelReply.Size = new System.Drawing.Size(36, 34);
            this._cancelReply.TabIndex = 1;
            this._cancelReply.Text = "×";
            this._cancelReply.UseVisualStyleBackColor = false;
            // 
            // _composeLayout
            // 
            this._composeLayout.ColumnCount = 3;
            this._composeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this._composeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._composeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 138F));
            this._composeLayout.Controls.Add(this._emoji, 0, 0);
            this._composeLayout.Controls.Add(this._composer, 1, 0);
            this._composeLayout.Controls.Add(this._send, 2, 0);
            this._composeLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._composeLayout.Location = new System.Drawing.Point(11, 547);
            this._composeLayout.Name = "_composeLayout";
            this._composeLayout.Padding = new System.Windows.Forms.Padding(4, 8, 4, 4);
            this._composeLayout.RowCount = 1;
            this._composeLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._composeLayout.Size = new System.Drawing.Size(950, 82);
            this._composeLayout.TabIndex = 3;
            // 
            // _emoji
            // 
            this._emoji.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this._emoji.Cursor = System.Windows.Forms.Cursors.Hand;
            this._emoji.Dock = System.Windows.Forms.DockStyle.Fill;
            this._emoji.FlatAppearance.BorderSize = 0;
            this._emoji.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._emoji.Font = new System.Drawing.Font("Segoe UI Emoji", 19F);
            this._emoji.ForeColor = System.Drawing.Color.White;
            this._emoji.Location = new System.Drawing.Point(891, 11);
            this._emoji.Name = "_emoji";
            this._emoji.Size = new System.Drawing.Size(52, 64);
            this._emoji.TabIndex = 0;
            this._emoji.Text = "😊";
            this._emoji.UseVisualStyleBackColor = false;
            // 
            // _composer
            // 
            this._composer.AcceptsReturn = true;
            this._composer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._composer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._composer.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this._composer.Location = new System.Drawing.Point(150, 8);
            this._composer.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this._composer.Multiline = true;
            this._composer.Name = "_composer";
            this._composer.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._composer.Size = new System.Drawing.Size(730, 70);
            this._composer.TabIndex = 1;
            this._composer.Text = "پیامت رو اینجا بنویس...";
            // 
            // _send
            // 
            this._send.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this._send.Cursor = System.Windows.Forms.Cursors.Hand;
            this._send.Dock = System.Windows.Forms.DockStyle.Fill;
            this._send.FlatAppearance.BorderSize = 0;
            this._send.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(64)))), ((int)(((byte)(175)))));
            this._send.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this._send.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._send.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._send.ForeColor = System.Drawing.Color.White;
            this._send.Location = new System.Drawing.Point(7, 11);
            this._send.Name = "_send";
            this._send.Size = new System.Drawing.Size(132, 64);
            this._send.TabIndex = 2;
            this._send.Text = "ارسال پیام  ➤";
            this._send.UseVisualStyleBackColor = false;
            // 
            // _sidebarPanel
            // 
            this._sidebarPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(30)))), ((int)(((byte)(48)))));
            this._sidebarPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._sidebarPanel.Controls.Add(this._sidebarLayout);
            this._sidebarPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sidebarPanel.Location = new System.Drawing.Point(0, 0);
            this._sidebarPanel.Name = "_sidebarPanel";
            this._sidebarPanel.Padding = new System.Windows.Forms.Padding(12);
            this._sidebarPanel.Size = new System.Drawing.Size(266, 636);
            this._sidebarPanel.TabIndex = 0;
            // 
            // _sidebarLayout
            // 
            this._sidebarLayout.ColumnCount = 1;
            this._sidebarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._sidebarLayout.Controls.Add(this._membersTitle, 0, 0);
            this._sidebarLayout.Controls.Add(this._userList, 0, 1);
            this._sidebarLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sidebarLayout.Location = new System.Drawing.Point(12, 12);
            this._sidebarLayout.Name = "_sidebarLayout";
            this._sidebarLayout.RowCount = 2;
            this._sidebarLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this._sidebarLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._sidebarLayout.Size = new System.Drawing.Size(240, 610);
            this._sidebarLayout.TabIndex = 0;
            // 
            // _membersTitle
            // 
            this._membersTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this._membersTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this._membersTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this._membersTitle.Location = new System.Drawing.Point(3, 0);
            this._membersTitle.Name = "_membersTitle";
            this._membersTitle.Size = new System.Drawing.Size(234, 58);
            this._membersTitle.TabIndex = 0;
            this._membersTitle.Text = "اعضای آنلاین  •  ۳ نفر";
            this._membersTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _userList
            // 
            this._userList.AutoScroll = true;
            this._userList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(30)))), ((int)(((byte)(48)))));
            this._userList.Controls.Add(this._designUserSample);
            this._userList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._userList.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this._userList.Location = new System.Drawing.Point(3, 61);
            this._userList.Name = "_userList";
            this._userList.Padding = new System.Windows.Forms.Padding(2, 8, 2, 2);
            this._userList.Size = new System.Drawing.Size(234, 546);
            this._userList.TabIndex = 1;
            this._userList.WrapContents = false;
            // 
            // _designUserSample
            // 
            this._designUserSample.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(45)))), ((int)(((byte)(67)))));
            this._designUserSample.Controls.Add(this._sampleUserName);
            this._designUserSample.Controls.Add(this._sampleUserStatus);
            this._designUserSample.Location = new System.Drawing.Point(-2, 11);
            this._designUserSample.Margin = new System.Windows.Forms.Padding(2, 3, 2, 5);
            this._designUserSample.Name = "_designUserSample";
            this._designUserSample.Size = new System.Drawing.Size(245, 54);
            this._designUserSample.TabIndex = 0;
            // 
            // _sampleUserName
            // 
            this._sampleUserName.AutoSize = true;
            this._sampleUserName.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this._sampleUserName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this._sampleUserName.Location = new System.Drawing.Point(12, 17);
            this._sampleUserName.Name = "_sampleUserName";
            this._sampleUserName.Size = new System.Drawing.Size(72, 21);
            this._sampleUserName.TabIndex = 0;
            this._sampleUserName.Text = "مریم  👩🏻‍🎨";
            // 
            // _sampleUserStatus
            // 
            this._sampleUserStatus.AutoSize = true;
            this._sampleUserStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._sampleUserStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(197)))), ((int)(((byte)(94)))));
            this._sampleUserStatus.Location = new System.Drawing.Point(160, 19);
            this._sampleUserStatus.Name = "_sampleUserStatus";
            this._sampleUserStatus.Size = new System.Drawing.Size(53, 19);
            this._sampleUserStatus.TabIndex = 1;
            this._sampleUserStatus.Text = "● آنلاین";
            // 
            // ChatGroup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(16)))), ((int)(((byte)(29)))));
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Controls.Add(this._rootPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.MinimumSize = new System.Drawing.Size(1024, 680);
            this.Name = "ChatGroup";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nexa Chat | گفتگوی آنلاین";
            this._rootPanel.ResumeLayout(false);
            this._rootLayout.ResumeLayout(false);
            this._headerPanel.ResumeLayout(false);
            this._headerPanel.PerformLayout();
            this._pinPanel.ResumeLayout(false);
            this._mainSplit.Panel1.ResumeLayout(false);
            this._mainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._mainSplit)).EndInit();
            this._mainSplit.ResumeLayout(false);
            this._chatLayout.ResumeLayout(false);
            this._messageList.ResumeLayout(false);
            this._designMessageSample.ResumeLayout(false);
            this._sampleBubble.ResumeLayout(false);
            this._sampleBubble.PerformLayout();
            this._replyPanel.ResumeLayout(false);
            this._composeLayout.ResumeLayout(false);
            this._composeLayout.PerformLayout();
            this._sidebarPanel.ResumeLayout(false);
            this._sidebarLayout.ResumeLayout(false);
            this._userList.ResumeLayout(false);
            this._designUserSample.ResumeLayout(false);
            this._designUserSample.PerformLayout();
            this.ResumeLayout(false);
        }

    }
}
