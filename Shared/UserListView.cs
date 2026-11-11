using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WinTimer = System.Windows.Forms.Timer;

namespace ChatpApp.Shared
{
    /// <summary>
    /// اطلاعات یک کاربر آنلاین برای نمایش در لیست
    /// </summary>
    public class UserRow
    {
        public string Name = "";
        public int Avatar;
        public string Ip = "";
        public int Port;
        public DateTime ConnectedAt;
        public int Messages;
    }

    /// <summary>
    /// لیست کاربران آنلاین (کاملا سفارشی)
    /// در حالت ساده (کلاینت): آواتار + نام + نقطه آنلاین
    /// در حالت Detailed (سرور): آی‌پی و پورت، زمان اتصال، مدت آنلاین بودن، تعداد پیام‌ها و دکمه قطع اتصال
    /// </summary>
    public class UserListView : Control, ISurface
    {
        private List<UserRow> rows = new List<UserRow>();
        private float scroll;
        private int hoverIndex = -1;
        private bool hoverKick;
        private readonly WinTimer timer = new WinTimer();

        // نمایش اطلاعات کامل شبکه (در سرور)
        [DefaultValue(false)]
        public bool Detailed { get; set; }

        // نام کاربر فعلی (در کلاینت با «شما» مشخص می‌شود)
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MyName { get; set; }

        public event Action<string> KickClicked;

        public UserListView()
        {
            MyName = "";
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Theme.Changed += OnThemeChanged;

            // هر ثانیه لیست دوباره کشیده می‌شود تا «مدت آنلاین بودن» زنده بماند
            timer.Interval = 1000;
            timer.Tick += delegate { if (Detailed && rows.Count > 0) Invalidate(); };
            timer.Start();
        }

        public Color SurfaceColor { get { return Theme.Surface; } }

        private void OnThemeChanged(object s, EventArgs e) { Invalidate(); }

        private int RowHeight { get { return Detailed ? 96 : 58; } }

        public int Count { get { return rows.Count; } }

        public void SetUsers(List<UserRow> list)
        {
            rows = list ?? new List<UserRow>();
            ClampScroll();
            Invalidate();
        }

        private void ClampScroll()
        {
            float max = Math.Max(0, rows.Count * RowHeight - Height);
            scroll = Math.Max(0, Math.Min(max, scroll));
        }

        private Rectangle RowRect(int i)
        {
            return new Rectangle(6, (int)(i * RowHeight - scroll) + 2, Width - 12, RowHeight - 4);
        }

        private Rectangle KickRect(int i)
        {
            Rectangle r = RowRect(i);
            return new Rectangle(r.X + 10, r.Y + (r.Height - 30) / 2, 92, 30);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            scroll -= e.Delta * 0.5f;
            ClampScroll();
            Invalidate();
            base.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int idx = -1;
            for (int i = 0; i < rows.Count; i++)
            {
                if (RowRect(i).Contains(e.Location)) { idx = i; break; }
            }
            bool kick = Detailed && idx >= 0 && KickRect(idx).Contains(e.Location);
            if (idx != hoverIndex || kick != hoverKick)
            {
                hoverIndex = idx;
                hoverKick = kick;
                Cursor = kick ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hoverIndex = -1;
            hoverKick = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && hoverKick && hoverIndex >= 0 && hoverIndex < rows.Count)
            {
                if (KickClicked != null) KickClicked(rows[hoverIndex].Name);
            }
            base.OnMouseDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Theme.Surface);

            if (rows.Count == 0)
            {
                TextRenderer.DrawText(g, "هنوز کسی آنلاین نیست", Theme.Font(10), new Rectangle(0, 0, Width, Math.Min(Height, 120)), Theme.Muted,
                    UiUtil.Flags(true, false, true, true, false));
                return;
            }

            for (int i = 0; i < rows.Count; i++)
            {
                Rectangle r = RowRect(i);
                if (r.Bottom < 0 || r.Top > Height) continue;
                DrawRow(g, rows[i], r, i);
            }
        }

        private void DrawRow(Graphics g, UserRow u, Rectangle r, int index)
        {
            if (index == hoverIndex) UiUtil.FillRound(g, Theme.Hover, r, 14);

            // آواتار سمت راست
            int av = Detailed ? 50 : 40;
            Rectangle ar = new Rectangle(r.Right - 10 - av, r.Y + (r.Height - av) / 2, av, av);
            AvatarPainter.Draw(g, u.Avatar, ar);

            // نقطه سبز آنلاین
            Rectangle dot = new Rectangle(ar.Right - 12, ar.Bottom - 12, 13, 13);
            using (Brush rb = new SolidBrush(index == hoverIndex ? Theme.Hover : Theme.Surface)) g.FillEllipse(rb, dot.X - 2, dot.Y - 2, dot.Width + 4, dot.Height + 4);
            using (Brush db = new SolidBrush(Theme.Success)) g.FillEllipse(db, dot);

            int textRight = ar.X - 10;
            bool me = u.Name == MyName;

            if (!Detailed)
            {
                Rectangle nr = new Rectangle(r.X + 8, r.Y, textRight - r.X - 8, r.Height);
                string text = me ? u.Name + "  (شما)" : u.Name;
                TextRenderer.DrawText(g, text, Theme.Font(10.5f, me ? FontStyle.Bold : FontStyle.Regular), nr,
                    me ? Theme.Accent : Theme.Text, UiUtil.Flags(true, false, false, true, true));
                return;
            }

            // نام
            Rectangle name = new Rectangle(r.X + 110, r.Y + 8, textRight - r.X - 110, 20);
            TextRenderer.DrawText(g, u.Name, Theme.Font(10.5f, FontStyle.Bold), name, Theme.Text, UiUtil.Flags(true, false, false, true, true));

            // سه خط اطلاعات شبکه
            Font small = Theme.Font(8.5f);
            DrawInfo(g, "آی‌پی", u.Ip + " : " + u.Port, textRight, r.Y + 30, small);
            DrawInfo(g, "متصل از", Fa.Time(u.ConnectedAt) + ":" + Fa.Digits(u.ConnectedAt.Second.ToString("00")), textRight, r.Y + 48, small);
            DrawInfo(g, "آنلاین", Fa.Duration(DateTime.Now - u.ConnectedAt) + "   |   پیام: " + Fa.Digits(u.Messages), textRight, r.Y + 66, small);

            // دکمه قطع اتصال
            Rectangle k = KickRect(index);
            Color kc = hoverKick && hoverIndex == index ? Theme.Mix(Theme.Danger, Color.Black, 0.12f) : Theme.Danger;
            UiUtil.FillRound(g, kc, k, 10);
            TextRenderer.DrawText(g, "قطع اتصال", Theme.Font(9, FontStyle.Bold), k, Color.White,
                UiUtil.Flags(true, false, true, true, false));
        }

        // یک خط «برچسب: مقدار»؛ برچسب فارسی سمت راست و مقدار (آی‌پی/عدد) کنار آن
        private void DrawInfo(Graphics g, string label, string value, int right, int y, Font font)
        {
            int lw = UiUtil.MeasureLine(label + ":", font).Width;
            TextRenderer.DrawText(g, label + ":", font, new Rectangle(right - lw, y, lw + 2, 16), Theme.Muted,
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);
            int vw = UiUtil.MeasureLine(value, font).Width + 4;
            TextRenderer.DrawText(g, value, font, new Rectangle(right - lw - 6 - vw, y, vw, 16), Theme.Text,
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
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
