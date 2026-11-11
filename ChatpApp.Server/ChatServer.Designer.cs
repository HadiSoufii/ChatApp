namespace ChatpApp.Server
{
    partial class ChatServer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatServer));
            this.header = new ChatpApp.Shared.GradientPanel();
            this.lblTitle = new ChatpApp.Shared.ThemedLabel();
            this.lblSub = new ChatpApp.Shared.ThemedLabel();
            this.pill = new ChatpApp.Shared.StatusPill();
            this.chipOnline = new ChatpApp.Shared.StatChip();
            this.chipMessages = new ChatpApp.Shared.StatChip();
            this.chipUptime = new ChatpApp.Shared.StatChip();
            this.toggle = new ChatpApp.Shared.ThemeToggle();
            this.settingsCard = new ChatpApp.Shared.RoundedPanel();
            this.lblIpCap = new ChatpApp.Shared.ThemedLabel();
            this.txtIp = new ChatpApp.Shared.ModernTextBox();
            this.lblPortCap = new ChatpApp.Shared.ThemedLabel();
            this.txtPort = new ChatpApp.Shared.ModernTextBox();
            this.lblWelCap = new ChatpApp.Shared.ThemedLabel();
            this.txtWelcome = new ChatpApp.Shared.ModernTextBox();
            this.btnStart = new ChatpApp.Shared.ModernButton();
            this.btnStop = new ChatpApp.Shared.ModernButton();
            this.lblNet = new ChatpApp.Shared.ThemedLabel();
            this.btnTabChat = new ChatpApp.Shared.ModernButton();
            this.btnTabLog = new ChatpApp.Shared.ModernButton();
            this.chat = new ChatpApp.Shared.ChatPanel();
            this.logCard = new ChatpApp.Shared.RoundedPanel();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.usersCard = new ChatpApp.Shared.RoundedPanel();
            this.lblUsers = new ChatpApp.Shared.ThemedLabel();
            this.users = new ChatpApp.Shared.UserListView();
            this.header.SuspendLayout();
            this.settingsCard.SuspendLayout();
            this.logCard.SuspendLayout();
            this.usersCard.SuspendLayout();
            this.SuspendLayout();
            // 
            // header
            // 
            this.header.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.header.Location = new System.Drawing.Point(0, 0);
            this.header.Name = "header";
            this.header.Size = new System.Drawing.Size(1260, 80);
            this.header.Controls.Add(this.lblTitle);
            this.header.Controls.Add(this.lblSub);
            this.header.Controls.Add(this.pill);
            this.header.Controls.Add(this.chipOnline);
            this.header.Controls.Add(this.chipMessages);
            this.header.Controls.Add(this.chipUptime);
            this.header.Controls.Add(this.toggle);
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(920, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Role = ChatpApp.Shared.LabelRole.OnAccent;
            this.lblTitle.Size = new System.Drawing.Size(320, 32);
            this.lblTitle.Text = "سرور چت‌روم";
            this.lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            // 
            // lblSub
            // 
            this.lblSub.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSub.Location = new System.Drawing.Point(920, 46);
            this.lblSub.Name = "lblSub";
            this.lblSub.Role = ChatpApp.Shared.LabelRole.OnAccent;
            this.lblSub.Size = new System.Drawing.Size(320, 22);
            this.lblSub.Text = "پنل مدیریت و نظارت بر گفتگو";
            this.lblSub.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            // 
            // pill
            // 
            this.pill.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.pill.Location = new System.Drawing.Point(770, 24);
            this.pill.Name = "pill";
            this.pill.Size = new System.Drawing.Size(140, 32);
            // 
            // chipOnline
            // 
            this.chipOnline.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.chipOnline.Location = new System.Drawing.Point(630, 15);
            this.chipOnline.Name = "chipOnline";
            this.chipOnline.Size = new System.Drawing.Size(130, 50);
            this.chipOnline.Title = "کاربران آنلاین";
            this.chipOnline.Value = "۰";
            // 
            // chipMessages
            // 
            this.chipMessages.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.chipMessages.Location = new System.Drawing.Point(490, 15);
            this.chipMessages.Name = "chipMessages";
            this.chipMessages.Size = new System.Drawing.Size(130, 50);
            this.chipMessages.Title = "کل پیام‌ها";
            this.chipMessages.Value = "۰";
            // 
            // chipUptime
            // 
            this.chipUptime.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.chipUptime.Location = new System.Drawing.Point(350, 15);
            this.chipUptime.Name = "chipUptime";
            this.chipUptime.Size = new System.Drawing.Size(130, 50);
            this.chipUptime.Title = "مدت فعالیت سرور";
            this.chipUptime.Value = "۰";
            // 
            // toggle
            // 
            this.toggle.Location = new System.Drawing.Point(20, 25);
            this.toggle.Name = "toggle";
            this.toggle.Size = new System.Drawing.Size(60, 30);
            this.toggle.TransparentBg = true;
            // 
            // settingsCard
            // 
            this.settingsCard.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.settingsCard.Location = new System.Drawing.Point(16, 94);
            this.settingsCard.Name = "settingsCard";
            this.settingsCard.Size = new System.Drawing.Size(1228, 124);
            this.settingsCard.Controls.Add(this.lblIpCap);
            this.settingsCard.Controls.Add(this.txtIp);
            this.settingsCard.Controls.Add(this.lblPortCap);
            this.settingsCard.Controls.Add(this.txtPort);
            this.settingsCard.Controls.Add(this.lblWelCap);
            this.settingsCard.Controls.Add(this.txtWelcome);
            this.settingsCard.Controls.Add(this.btnStart);
            this.settingsCard.Controls.Add(this.btnStop);
            this.settingsCard.Controls.Add(this.lblNet);
            // 
            // lblIpCap
            // 
            this.lblIpCap.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIpCap.Location = new System.Drawing.Point(988, 12);
            this.lblIpCap.Name = "lblIpCap";
            this.lblIpCap.Role = ChatpApp.Shared.LabelRole.Muted;
            this.lblIpCap.Size = new System.Drawing.Size(220, 20);
            this.lblIpCap.Text = "آی‌پی سرور (خصوصی)";
            this.lblIpCap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            // 
            // txtIp
            // 
            this.txtIp.Location = new System.Drawing.Point(988, 34);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(220, 44);
            this.txtIp.Rtl = false;
            this.txtIp.Text = "127.0.0.1";
            this.txtIp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            // 
            // lblPortCap
            // 
            this.lblPortCap.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPortCap.Location = new System.Drawing.Point(866, 12);
            this.lblPortCap.Name = "lblPortCap";
            this.lblPortCap.Role = ChatpApp.Shared.LabelRole.Muted;
            this.lblPortCap.Size = new System.Drawing.Size(110, 20);
            this.lblPortCap.Text = "پورت";
            this.lblPortCap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(866, 34);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(110, 44);
            this.txtPort.Rtl = false;
            this.txtPort.Text = "3001";
            this.txtPort.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            // 
            // lblWelCap
            // 
            this.lblWelCap.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWelCap.Location = new System.Drawing.Point(306, 12);
            this.lblWelCap.Name = "lblWelCap";
            this.lblWelCap.Role = ChatpApp.Shared.LabelRole.Muted;
            this.lblWelCap.Size = new System.Drawing.Size(548, 20);
            this.lblWelCap.Text = "متن خوشامدگویی به کاربران تازه‌وارد";
            this.lblWelCap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // 
            // txtWelcome
            // 
            this.txtWelcome.Location = new System.Drawing.Point(306, 34);
            this.txtWelcome.Name = "txtWelcome";
            this.txtWelcome.Size = new System.Drawing.Size(548, 44);
            this.txtWelcome.Text = "به چت‌روم خوش آمدی! اینجا می‌توانی با بقیه گفتگو کنی، پیام‌ها را پاسخ بدهی و واکنش نشان بدهی. لطفا مودب باش و از گفتگو لذت ببر.";
            this.txtWelcome.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.Location = new System.Drawing.Point(160, 34);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(130, 44);
            this.btnStart.Text = "روشن کردن سرور";
            this.btnStart.Kind = ChatpApp.Shared.BtnKind.Success;
            this.btnStart.Radius = 14;
            // 
            // btnStop
            // 
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.Location = new System.Drawing.Point(20, 34);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(130, 44);
            this.btnStop.Text = "توقف سرور";
            this.btnStop.Kind = ChatpApp.Shared.BtnKind.Danger;
            this.btnStop.Radius = 14;
            // 
            // lblNet
            // 
            this.lblNet.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNet.Location = new System.Drawing.Point(20, 88);
            this.lblNet.Name = "lblNet";
            this.lblNet.Role = ChatpApp.Shared.LabelRole.Muted;
            this.lblNet.Size = new System.Drawing.Size(1188, 26);
            this.lblNet.Text = "آی‌پی‌های شبکه این دستگاه: ...";
            this.lblNet.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // 
            // btnTabChat
            // 
            this.btnTabChat.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTabChat.Location = new System.Drawing.Point(710, 230);
            this.btnTabChat.Name = "btnTabChat";
            this.btnTabChat.Size = new System.Drawing.Size(140, 40);
            this.btnTabChat.Text = "گفتگوی زنده";
            this.btnTabChat.Radius = 14;
            this.btnTabChat.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            // 
            // btnTabLog
            // 
            this.btnTabLog.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTabLog.Location = new System.Drawing.Point(562, 230);
            this.btnTabLog.Name = "btnTabLog";
            this.btnTabLog.Size = new System.Drawing.Size(140, 40);
            this.btnTabLog.Text = "لاگ شبکه";
            this.btnTabLog.Kind = ChatpApp.Shared.BtnKind.Secondary;
            this.btnTabLog.Radius = 14;
            this.btnTabLog.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            // 
            // chat
            // 
            this.chat.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.chat.Location = new System.Drawing.Point(16, 278);
            this.chat.Name = "chat";
            this.chat.Size = new System.Drawing.Size(834, 506);
            // 
            // logCard
            // 
            this.logCard.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.logCard.Location = new System.Drawing.Point(16, 278);
            this.logCard.Name = "logCard";
            this.logCard.Size = new System.Drawing.Size(834, 506);
            this.logCard.Visible = false;
            this.logCard.Controls.Add(this.rtbLog);
            // 
            // rtbLog
            // 
            this.rtbLog.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.rtbLog.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.rtbLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbLog.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbLog.Location = new System.Drawing.Point(14, 14);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.rtbLog.Size = new System.Drawing.Size(806, 478);
            this.rtbLog.Text = "";
            // 
            // usersCard
            // 
            this.usersCard.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.usersCard.Location = new System.Drawing.Point(864, 230);
            this.usersCard.Name = "usersCard";
            this.usersCard.Size = new System.Drawing.Size(380, 554);
            this.usersCard.Controls.Add(this.lblUsers);
            this.usersCard.Controls.Add(this.users);
            // 
            // lblUsers
            // 
            this.lblUsers.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsers.Location = new System.Drawing.Point(18, 14);
            this.lblUsers.Name = "lblUsers";
            this.lblUsers.Role = ChatpApp.Shared.LabelRole.Text;
            this.lblUsers.Size = new System.Drawing.Size(344, 26);
            this.lblUsers.Text = "کاربران آنلاین (۰)";
            // 
            // users
            // 
            this.users.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.users.Location = new System.Drawing.Point(6, 50);
            this.users.Name = "users";
            this.users.Size = new System.Drawing.Size(368, 496);
            // 
            // ChatServer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 249);
            this.ClientSize = new System.Drawing.Size(1260, 800);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1120, 720);
            this.Name = "ChatServer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "سرور چت‌روم";
            this.Controls.Add(this.header);
            this.Controls.Add(this.settingsCard);
            this.Controls.Add(this.btnTabChat);
            this.Controls.Add(this.btnTabLog);
            this.Controls.Add(this.chat);
            this.Controls.Add(this.logCard);
            this.Controls.Add(this.usersCard);
            this.header.ResumeLayout(false);
            this.settingsCard.ResumeLayout(false);
            this.logCard.ResumeLayout(false);
            this.usersCard.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private ChatpApp.Shared.GradientPanel header;
        private ChatpApp.Shared.ThemedLabel lblTitle;
        private ChatpApp.Shared.ThemedLabel lblSub;
        private ChatpApp.Shared.StatusPill pill;
        private ChatpApp.Shared.StatChip chipOnline;
        private ChatpApp.Shared.StatChip chipMessages;
        private ChatpApp.Shared.StatChip chipUptime;
        private ChatpApp.Shared.ThemeToggle toggle;
        private ChatpApp.Shared.RoundedPanel settingsCard;
        private ChatpApp.Shared.ThemedLabel lblIpCap;
        private ChatpApp.Shared.ModernTextBox txtIp;
        private ChatpApp.Shared.ThemedLabel lblPortCap;
        private ChatpApp.Shared.ModernTextBox txtPort;
        private ChatpApp.Shared.ThemedLabel lblWelCap;
        private ChatpApp.Shared.ModernTextBox txtWelcome;
        private ChatpApp.Shared.ModernButton btnStart;
        private ChatpApp.Shared.ModernButton btnStop;
        private ChatpApp.Shared.ThemedLabel lblNet;
        private ChatpApp.Shared.ModernButton btnTabChat;
        private ChatpApp.Shared.ModernButton btnTabLog;
        private ChatpApp.Shared.ChatPanel chat;
        private ChatpApp.Shared.RoundedPanel logCard;
        private System.Windows.Forms.RichTextBox rtbLog;
        private ChatpApp.Shared.RoundedPanel usersCard;
        private ChatpApp.Shared.ThemedLabel lblUsers;
        private ChatpApp.Shared.UserListView users;
    }
}
