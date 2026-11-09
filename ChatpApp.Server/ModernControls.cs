using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatpApp.Server
{
    // پس‌زمینه گرادیانی با خطوط موجی ظریف در بالای پنجره.
    public class EmeraldBackgroundPanel : Panel
    {
        public Color BottomColor { get; set; }
        public EmeraldBackgroundPanel()
        {
            BottomColor = Color.FromArgb(4, 35, 32);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Width < 1 || Height < 1) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (LinearGradientBrush b = new LinearGradientBrush(ClientRectangle, BackColor, BottomColor, 35F))
                e.Graphics.FillRectangle(b, ClientRectangle);
            using (Pen p = new Pen(Color.FromArgb(30, 16, 185, 129), 1.3F))
                e.Graphics.DrawBezier(p, Width * .38F, 0, Width * .50F, Height * .20F, Width * .62F, Height * .03F, Width * .78F, 0);
            using (Pen p = new Pen(Color.FromArgb(17, 52, 211, 153), 1F))
                e.Graphics.DrawBezier(p, Width * .48F, Height * .17F, Width * .63F, Height * .34F, Width * .76F, Height * .02F, Width * .92F, 0);
        }
    }

    // کارت گرادیانی با گوشه‌های گرد و خط دور سفارشی.
    public class ModernCardPanel : Panel
    {
        public int CornerRadius { get; set; }
        public Color EndColor { get; set; }
        public Color BorderColor { get; set; }
        public ModernCardPanel()
        {
            CornerRadius = 16; EndColor = Color.FromArgb(8, 29, 28); BorderColor = Color.FromArgb(27, 104, 82);
            BorderStyle = BorderStyle.None;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Width < 2 || Height < 2) return;
            using (GraphicsPath path = Shape(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius))
            using (LinearGradientBrush b = new LinearGradientBrush(ClientRectangle, BackColor, EndColor, 115F))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(b, path);
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Width < 3 || Height < 3) return;
            using (GraphicsPath path = Shape(new Rectangle(1, 1, Width - 3, Height - 3), CornerRadius))
            using (Pen p = new Pen(BorderColor, 1F))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(p, path);
            }
        }
        internal static GraphicsPath Shape(Rectangle r, int radius)
        {
            GraphicsPath p = new GraphicsPath();
            int d = Math.Max(2, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }
    }

    // دکمه واقعی WinForms با رندر اختصاصی، گرادیان، Hover و Pressed.
    public class ModernButton : Button
    {
        private bool _hover, _pressed;
        public Color HoverColor { get; set; }
        public Color PressedColor { get; set; }
        public Color BorderColor { get; set; }
        public int CornerRadius { get; set; }
        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0; UseVisualStyleBackColor = false; Cursor = Cursors.Hand;
            CornerRadius = 12; HoverColor = Color.FromArgb(5, 150, 105); PressedColor = Color.FromArgb(4, 105, 78);
            BorderColor = Color.FromArgb(61, 203, 153);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) _pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 3 || Height < 3) return;
            Rectangle r = new Rectangle(1, 1, Width - 3, Height - 3);
            Color top = BackColor, bottom = Darken(BackColor, .78F);
            if (_hover) { top = HoverColor; bottom = Darken(HoverColor, .78F); }
            if (_pressed) { top = PressedColor; bottom = Darken(PressedColor, .75F); }
            if (!Enabled) { top = Color.FromArgb(64, 79, 77); bottom = Color.FromArgb(40, 53, 51); }
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = ModernCardPanel.Shape(r, CornerRadius))
            using (LinearGradientBrush b = new LinearGradientBrush(r, top, bottom, 90F))
            using (Pen p = new Pen(Enabled ? BorderColor : Color.FromArgb(70, 85, 82), 1F))
            {
                e.Graphics.FillPath(b, path); e.Graphics.DrawPath(p, path);
            }
            using (Pen p = new Pen(Color.FromArgb(40, Color.White), 1F)) e.Graphics.DrawLine(p, 13, 2, Math.Max(13, Width - 13), 2);
            TextRenderer.DrawText(e.Graphics, Text, Font, r, Enabled ? ForeColor : Color.FromArgb(150, 165, 160),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft | TextFormatFlags.EndEllipsis);
        }
        private static Color Darken(Color c, float f) { return Color.FromArgb(c.A, (int)(c.R * f), (int)(c.G * f), (int)(c.B * f)); }
    }

    // ورودی با قاب گرد؛ TextBox داخلی بدون Border باعث می‌شود تایپ و انتخاب متن استاندارد باقی بماند.
    public class ModernTextBox : UserControl
    {
        private readonly TextBox _input;
        private bool _focused;
        public ModernTextBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            base.BackColor = Color.FromArgb(8, 28, 27); base.ForeColor = Color.FromArgb(235, 248, 243); base.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Height = 40;
            _input = new TextBox { BorderStyle = BorderStyle.None, BackColor = BackColor, ForeColor = ForeColor, Font = Font, TextAlign = HorizontalAlignment.Center };
            _input.TextChanged += delegate { OnTextChanged(EventArgs.Empty); };
            _input.GotFocus += delegate { _focused = true; Invalidate(); };
            _input.LostFocus += delegate { _focused = false; Invalidate(); };
            Controls.Add(_input); Resize += delegate { LayoutInput(); }; LayoutInput();
        }
        public override string Text
        {
            get { return _input == null ? base.Text : _input.Text; }
            set { base.Text = value; if (_input != null && _input.Text != value) _input.Text = value ?? ""; }
        }
        public new Font Font
        {
            get { return base.Font; }
            set { base.Font = value; if (_input != null) _input.Font = value; LayoutInput(); }
        }
        public HorizontalAlignment TextAlign { get { return _input.TextAlign; } set { _input.TextAlign = value; } }
        public int MaxLength { get { return _input.MaxLength; } set { _input.MaxLength = value; } }
        public bool ReadOnly { get { return _input.ReadOnly; } set { _input.ReadOnly = value; } }
        public new bool Focus() { return _input.Focus(); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); if (_input != null) _input.Enabled = Enabled; Invalidate(); }
        protected override void OnBackColorChanged(EventArgs e) { base.OnBackColorChanged(e); if (_input != null) _input.BackColor = BackColor; Invalidate(); }
        protected override void OnForeColorChanged(EventArgs e) { base.OnForeColorChanged(e); if (_input != null) _input.ForeColor = ForeColor; Invalidate(); }
        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); if (_input != null) _input.Font = base.Font; LayoutInput(); }
        private void LayoutInput()
        {
            if (_input == null) return;
            _input.Location = new Point(12, Math.Max(5, (Height - _input.Height) / 2));
            _input.Width = Math.Max(20, Width - 24); _input.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 3 || Height < 3) return;
            Rectangle r = new Rectangle(1, 1, Width - 3, Height - 3);
            Color border = _focused ? Color.FromArgb(52, 211, 153) : Color.FromArgb(39, 91, 78);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = ModernCardPanel.Shape(r, 10))
            using (LinearGradientBrush b = new LinearGradientBrush(r, BackColor, Color.FromArgb(Math.Max(0, BackColor.R - 2), Math.Max(0, BackColor.G - 3), Math.Max(0, BackColor.B - 3)), 90F))
            using (Pen p = new Pen(border, _focused ? 1.7F : 1F))
            { e.Graphics.FillPath(b, path); e.Graphics.DrawPath(p, path); }
        }
    }

    // فهرست سفارشی با ردیف‌های فاصله‌دار، نقطه وضعیت و انتخاب زمردی.
    public class ModernListBox : ListBox
    {
        public Color RowColor { get; set; }
        public Color AlternateRowColor { get; set; }
        public Color SelectedRowColor { get; set; }
        public Color AccentColor { get; set; }
        public ModernListBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed; IntegralHeight = false; ItemHeight = 36; BorderStyle = BorderStyle.None;
            RowColor = Color.FromArgb(8, 26, 25); AlternateRowColor = Color.FromArgb(10, 31, 29);
            SelectedRowColor = Color.FromArgb(15, 72, 57); AccentColor = Color.FromArgb(52, 211, 153);
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (SolidBrush b = new SolidBrush(selected ? SelectedRowColor : (e.Index % 2 == 0 ? RowColor : AlternateRowColor))) e.Graphics.FillRectangle(b, e.Bounds);
            using (SolidBrush dot = new SolidBrush(AccentColor)) e.Graphics.FillEllipse(dot, e.Bounds.Left + 12, e.Bounds.Top + (e.Bounds.Height - 7) / 2, 7, 7);
            Rectangle textRect = new Rectangle(e.Bounds.Left + 27, e.Bounds.Top + 1, Math.Max(10, e.Bounds.Width - 38), e.Bounds.Height - 2);
            TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, textRect, selected ? Color.White : ForeColor,
                TextFormatFlags.RightToLeft | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            using (Pen p = new Pen(Color.FromArgb(18, 75, 63), 1F)) e.Graphics.DrawLine(p, e.Bounds.Left + 10, e.Bounds.Bottom - 1, e.Bounds.Right - 10, e.Bounds.Bottom - 1);
        }
    }

    // تب‌های سفارشی با دکمه‌های گرادیانی و گوشه گرد.
    public class ModernTabControl : TabControl
    {
        public Color SelectedTop { get; set; }
        public Color SelectedBottom { get; set; }
        public Color IdleTop { get; set; }
        public Color IdleBottom { get; set; }
        public ModernTabControl()
        {
            DrawMode = TabDrawMode.OwnerDrawFixed; SizeMode = TabSizeMode.Fixed; ItemSize = new Size(132, 34); Padding = new Point(8, 5);
            SelectedTop = Color.FromArgb(18, 166, 119); SelectedBottom = Color.FromArgb(5, 108, 82);
            IdleTop = Color.FromArgb(15, 43, 40); IdleBottom = Color.FromArgb(9, 31, 29);
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= TabPages.Count) return;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Rectangle r = new Rectangle(e.Bounds.X + 3, e.Bounds.Y + 3, Math.Max(1, e.Bounds.Width - 6), Math.Max(1, e.Bounds.Height - 6));
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = ModernCardPanel.Shape(r, 8))
            using (LinearGradientBrush b = new LinearGradientBrush(r, selected ? SelectedTop : IdleTop, selected ? SelectedBottom : IdleBottom, 90F))
            using (Pen p = new Pen(selected ? Color.FromArgb(67, 220, 170) : Color.FromArgb(31, 89, 72), 1F))
            { e.Graphics.FillPath(b, path); e.Graphics.DrawPath(p, path); }
            using (Font f = new Font("Segoe UI", 8.2F, FontStyle.Bold))
                TextRenderer.DrawText(e.Graphics, TabPages[e.Index].Text, f, r, selected ? Color.White : Color.FromArgb(184, 215, 203),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft | TextFormatFlags.EndEllipsis);
        }
    }
}