namespace ChatpApp.Client
{
    partial class ChatGroup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatGroup));
            this.header = new ChatpApp.Shared.GradientPanel();
            this.lblTitle = new ChatpApp.Shared.ThemedLabel();
            this.lblSub = new ChatpApp.Shared.ThemedLabel();
            this.pill = new ChatpApp.Shared.StatusPill();
            this.toggle = new ChatpApp.Shared.ThemeToggle();
            this.btnSound = new ChatpApp.Shared.ModernButton();
            this.btnReturn = new ChatpApp.Shared.ModernButton();
            this.usersCard = new ChatpApp.Shared.RoundedPanel();
            this.lblUsers = new ChatpApp.Shared.ThemedLabel();
            this.users = new ChatpApp.Shared.UserListView();
            this.chat = new ChatpApp.Shared.ChatPanel();
            this.header.SuspendLayout();
            this.usersCard.SuspendLayout();
            this.SuspendLayout();
            // 
            // header
            // 
            this.header.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.header.Location = new System.Drawing.Point(0, 0);
            this.header.Name = "header";
            this.header.Size = new System.Drawing.Size(1120, 66);
            this.header.Controls.Add(this.lblTitle);
            this.header.Controls.Add(this.lblSub);
            this.header.Controls.Add(this.pill);
            this.header.Controls.Add(this.toggle);
            this.header.Controls.Add(this.btnSound);
            this.header.Controls.Add(this.btnReturn);
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(820, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Role = ChatpApp.Shared.LabelRole.OnAccent;
            this.lblTitle.Size = new System.Drawing.Size(280, 28);
            this.lblTitle.Text = "چت‌روم";
            this.lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            // 
            // lblSub
            // 
            this.lblSub.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSub.Location = new System.Drawing.Point(820, 38);
            this.lblSub.Name = "lblSub";
            this.lblSub.Role = ChatpApp.Shared.LabelRole.OnAccent;
            this.lblSub.Size = new System.Drawing.Size(280, 20);
            this.lblSub.Text = "متصل به 127.0.0.1 : 3001";
            this.lblSub.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            // 
            // pill
            // 
            this.pill.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.pill.Location = new System.Drawing.Point(670, 17);
            this.pill.Name = "pill";
            this.pill.Size = new System.Drawing.Size(140, 32);
            // 
            // toggle
            // 
            this.toggle.Location = new System.Drawing.Point(20, 18);
            this.toggle.Name = "toggle";
            this.toggle.Size = new System.Drawing.Size(60, 30);
            this.toggle.TransparentBg = true;
            // 
            // btnSound
            // 
            this.btnSound.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSound.Location = new System.Drawing.Point(92, 13);
            this.btnSound.Name = "btnSound";
            this.btnSound.Size = new System.Drawing.Size(40, 40);
            this.btnSound.Text = "";
            this.btnSound.Kind = ChatpApp.Shared.BtnKind.Ghost;
            this.btnSound.Radius = 20;
            this.btnSound.TransparentBg = true;
            // 
            // btnReturn
            // 
            this.btnReturn.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReturn.Location = new System.Drawing.Point(142, 13);
            this.btnReturn.Name = "btnReturn";
            this.btnReturn.Size = new System.Drawing.Size(130, 40);
            this.btnReturn.Text = "خروج";
            this.btnReturn.Kind = ChatpApp.Shared.BtnKind.Ghost;
            this.btnReturn.Radius = 20;
            this.btnReturn.TransparentBg = true;
            // 
            // usersCard
            // 
            this.usersCard.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.usersCard.Location = new System.Drawing.Point(830, 80);
            this.usersCard.Name = "usersCard";
            this.usersCard.Size = new System.Drawing.Size(276, 626);
            this.usersCard.Controls.Add(this.lblUsers);
            this.usersCard.Controls.Add(this.users);
            // 
            // lblUsers
            // 
            this.lblUsers.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsers.Location = new System.Drawing.Point(16, 14);
            this.lblUsers.Name = "lblUsers";
            this.lblUsers.Role = ChatpApp.Shared.LabelRole.Text;
            this.lblUsers.Size = new System.Drawing.Size(244, 26);
            this.lblUsers.Text = "کاربران آنلاین (۰)";
            // 
            // users
            // 
            this.users.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.users.Location = new System.Drawing.Point(6, 48);
            this.users.Name = "users";
            this.users.Size = new System.Drawing.Size(264, 570);
            // 
            // chat
            // 
            this.chat.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.chat.Location = new System.Drawing.Point(14, 80);
            this.chat.Name = "chat";
            this.chat.Size = new System.Drawing.Size(802, 626);
            // 
            // ChatGroup
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 249);
            this.ClientSize = new System.Drawing.Size(1120, 720);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "ChatGroup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "چت‌روم";
            this.Controls.Add(this.header);
            this.Controls.Add(this.usersCard);
            this.Controls.Add(this.chat);
            this.header.ResumeLayout(false);
            this.usersCard.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private ChatpApp.Shared.GradientPanel header;
        private ChatpApp.Shared.ThemedLabel lblTitle;
        private ChatpApp.Shared.ThemedLabel lblSub;
        private ChatpApp.Shared.StatusPill pill;
        private ChatpApp.Shared.ThemeToggle toggle;
        private ChatpApp.Shared.ModernButton btnSound;
        private ChatpApp.Shared.ModernButton btnReturn;
        private ChatpApp.Shared.RoundedPanel usersCard;
        private ChatpApp.Shared.ThemedLabel lblUsers;
        private ChatpApp.Shared.UserListView users;
        private ChatpApp.Shared.ChatPanel chat;
    }
}
