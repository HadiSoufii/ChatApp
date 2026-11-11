namespace ChatpApp.Client
{
    partial class Login
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.art = new ChatpApp.Client.LoginArt();
            this.formArea = new ChatpApp.Shared.SurfacePanel();
            this.toggle = new ChatpApp.Shared.ThemeToggle();
            this.lblTitle = new ChatpApp.Shared.ThemedLabel();
            this.lblSub = new ChatpApp.Shared.ThemedLabel();
            this.picker = new ChatpApp.Shared.AvatarPicker();
            this.lblNameCap = new ChatpApp.Shared.ThemedLabel();
            this.txtName = new ChatpApp.Shared.ModernTextBox();
            this.lblIpCap = new ChatpApp.Shared.ThemedLabel();
            this.txtIp = new ChatpApp.Shared.ModernTextBox();
            this.lblPortCap = new ChatpApp.Shared.ThemedLabel();
            this.txtPort = new ChatpApp.Shared.ModernTextBox();
            this.lblError = new ChatpApp.Shared.ThemedLabel();
            this.btnLogin = new ChatpApp.Shared.ModernButton();
            this.lblFooter = new ChatpApp.Shared.ThemedLabel();
            this.formArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // art
            // 
            this.art.Location = new System.Drawing.Point(0, 0);
            this.art.Name = "art";
            this.art.Size = new System.Drawing.Size(420, 640);
            // 
            // formArea
            // 
            this.formArea.Location = new System.Drawing.Point(420, 0);
            this.formArea.Name = "formArea";
            this.formArea.Size = new System.Drawing.Size(560, 640);
            this.formArea.Controls.Add(this.toggle);
            this.formArea.Controls.Add(this.lblTitle);
            this.formArea.Controls.Add(this.lblSub);
            this.formArea.Controls.Add(this.picker);
            this.formArea.Controls.Add(this.lblNameCap);
            this.formArea.Controls.Add(this.txtName);
            this.formArea.Controls.Add(this.lblIpCap);
            this.formArea.Controls.Add(this.txtIp);
            this.formArea.Controls.Add(this.lblPortCap);
            this.formArea.Controls.Add(this.txtPort);
            this.formArea.Controls.Add(this.lblError);
            this.formArea.Controls.Add(this.btnLogin);
            this.formArea.Controls.Add(this.lblFooter);
            // 
            // toggle
            // 
            this.toggle.Location = new System.Drawing.Point(24, 22);
            this.toggle.Name = "toggle";
            this.toggle.Size = new System.Drawing.Size(60, 30);
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 23F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(50, 62);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Role = ChatpApp.Shared.LabelRole.Text;
            this.lblTitle.Size = new System.Drawing.Size(460, 44);
            this.lblTitle.Text = "ورود به چت‌روم";
            // 
            // lblSub
            // 
            this.lblSub.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSub.Location = new System.Drawing.Point(50, 108);
            this.lblSub.Name = "lblSub";
            this.lblSub.Role = ChatpApp.Shared.LabelRole.Muted;
            this.lblSub.Size = new System.Drawing.Size(460, 26);
            this.lblSub.Text = "آواتار و نام کاربری‌ات را انتخاب کن و به گفتگو بپیوند";
            // 
            // picker
            // 
            this.picker.Location = new System.Drawing.Point(50, 146);
            this.picker.Name = "picker";
            this.picker.Size = new System.Drawing.Size(460, 108);
            // 
            // lblNameCap
            // 
            this.lblNameCap.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNameCap.Location = new System.Drawing.Point(50, 268);
            this.lblNameCap.Name = "lblNameCap";
            this.lblNameCap.Role = ChatpApp.Shared.LabelRole.Muted;
            this.lblNameCap.Size = new System.Drawing.Size(460, 20);
            this.lblNameCap.Text = "نام کاربری";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(50, 292);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(460, 46);
            this.txtName.Hint = "مثلا: علی";
            // 
            // lblIpCap
            // 
            this.lblIpCap.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIpCap.Location = new System.Drawing.Point(220, 352);
            this.lblIpCap.Name = "lblIpCap";
            this.lblIpCap.Role = ChatpApp.Shared.LabelRole.Muted;
            this.lblIpCap.Size = new System.Drawing.Size(290, 20);
            this.lblIpCap.Text = "آدرس سرور (IP)";
            // 
            // txtIp
            // 
            this.txtIp.Location = new System.Drawing.Point(220, 376);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(290, 46);
            this.txtIp.Rtl = false;
            this.txtIp.Text = "127.0.0.1";
            // 
            // lblPortCap
            // 
            this.lblPortCap.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPortCap.Location = new System.Drawing.Point(50, 352);
            this.lblPortCap.Name = "lblPortCap";
            this.lblPortCap.Role = ChatpApp.Shared.LabelRole.Muted;
            this.lblPortCap.Size = new System.Drawing.Size(160, 20);
            this.lblPortCap.Text = "پورت";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(50, 376);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(160, 46);
            this.txtPort.Rtl = false;
            this.txtPort.Text = "3001";
            // 
            // lblError
            // 
            this.lblError.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblError.Location = new System.Drawing.Point(50, 432);
            this.lblError.Name = "lblError";
            this.lblError.Role = ChatpApp.Shared.LabelRole.Danger;
            this.lblError.Size = new System.Drawing.Size(460, 24);
            this.lblError.Text = "";
            // 
            // btnLogin
            // 
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogin.Location = new System.Drawing.Point(50, 464);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(460, 54);
            this.btnLogin.Text = "ورود به چت";
            this.btnLogin.Radius = 16;
            // 
            // lblFooter
            // 
            this.lblFooter.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFooter.Location = new System.Drawing.Point(50, 548);
            this.lblFooter.Name = "lblFooter";
            this.lblFooter.Role = ChatpApp.Shared.LabelRole.Muted;
            this.lblFooter.Size = new System.Drawing.Size(460, 22);
            this.lblFooter.Text = "پروژه پایانی درس برنامه‌نویسی شبکه";
            this.lblFooter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Login
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 249);
            this.ClientSize = new System.Drawing.Size(980, 640);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ورود به چت‌روم";
            this.Controls.Add(this.art);
            this.Controls.Add(this.formArea);
            this.formArea.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private ChatpApp.Client.LoginArt art;
        private ChatpApp.Shared.SurfacePanel formArea;
        private ChatpApp.Shared.ThemeToggle toggle;
        private ChatpApp.Shared.ThemedLabel lblTitle;
        private ChatpApp.Shared.ThemedLabel lblSub;
        private ChatpApp.Shared.AvatarPicker picker;
        private ChatpApp.Shared.ThemedLabel lblNameCap;
        private ChatpApp.Shared.ModernTextBox txtName;
        private ChatpApp.Shared.ThemedLabel lblIpCap;
        private ChatpApp.Shared.ModernTextBox txtIp;
        private ChatpApp.Shared.ThemedLabel lblPortCap;
        private ChatpApp.Shared.ModernTextBox txtPort;
        private ChatpApp.Shared.ThemedLabel lblError;
        private ChatpApp.Shared.ModernButton btnLogin;
        private ChatpApp.Shared.ThemedLabel lblFooter;
    }
}
