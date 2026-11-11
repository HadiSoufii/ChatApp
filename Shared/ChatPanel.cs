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
    /// نوار پیام‌های سنجاق شده بالای چت. با کلیک روی آن به پیام سنجاق شده می‌پریم (و به پیام سنجاق شده بعدی می‌رویم)
    /// </summary>
    public class PinnedBar : Control
    {
        private List<PinInfo> pins = new List<PinInfo>();
        private int index;
        public bool ShowClose;                       // در سرور دکمه برداشتن سنجاق نشان داده می‌شود
        private bool hover;

        public event Action<PinInfo> PinClicked;
        public event Action<long> UnpinClicked;

        public PinnedBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);
            Cursor = Cursors.Hand;
            Theme.Changed += OnThemeChanged;
        }

        private void OnThemeChanged(object s, EventArgs e) { Invalidate(); }

        public int Count { get { return pins.Count; } }

        public void SetPins(List<PinInfo> list)
        {
            pins = list ?? new List<PinInfo>();
            if (index >= pins.Count) index = 0;
            Invalidate();
        }

        private Rectangle CloseRect() { return new Rectangle(10, (Height - 28) / 2, 28, 28); }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (pins.Count == 0 || e.Button != MouseButtons.Left) return;
            PinInfo cur = pins[index];
            if (ShowClose && CloseRect().Contains(e.Location))
            {
                if (UnpinClicked != null) UnpinClicked(cur.Id);
                return;
            }
            if (PinClicked != null) PinClicked(cur);
            index = (index + 1) % pins.Count;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(hover ? Theme.Mix(Theme.Surface, Theme.Hover, 0.6f) : Theme.Surface);
            using (Pen p = new Pen(Theme.Border)) g.DrawLine(p, 0, Height - 1, Width, Height - 1);
            if (pins.Count == 0) return;

            PinInfo cur = pins[index];

            // نوار رنگی و آیکون سنجاق سمت راست
            UiUtil.FillRound(g, Theme.Accent, new RectangleF(Width - 8, 8, 3, Height - 16), 1.5f);
            Icons.Pin(g, new Rectangle(Width - 36, (Height - 20) / 2, 20, 20), Theme.Accent);

            string head = "پیام سنجاق‌شده";
            if (pins.Count > 1) head += "  (" + Fa.Digits(index + 1) + " از " + Fa.Digits(pins.Count) + ")";
            int textRight = Width - 44;
            int left = ShowClose ? 44 : 12;
            Rectangle r1 = new Rectangle(left, 5, textRight - left, 18);
            Rectangle r2 = new Rectangle(left, 23, textRight - left, 18);
            TextRenderer.DrawText(g, head, Theme.Font(9, FontStyle.Bold), r1, Theme.Accent, UiUtil.Flags(true, false, false, true, true));
            string body = cur.Sender + ": " + ChatText.Snippet(cur.Text, 80);
            TextRenderer.DrawText(g, body, Theme.Font(9.5f), r2, Theme.Text, UiUtil.Flags(Fa.IsRtl(cur.Text), false, false, true, true));

            if (ShowClose)
            {
                Rectangle c = CloseRect();
                UiUtil.FillRound(g, Theme.Surface2, c, 14);
                Icons.Close(g, new Rectangle(c.X + 6, c.Y + 6, 16, 16), Theme.Muted);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Theme.Changed -= OnThemeChanged;
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// نوار بالای ورودی که نشان می‌دهد در حال پاسخ دادن یا ویرایش چه پیامی هستیم
    /// </summary>
    public class ContextBar : Control
    {
        private string title = "";
        private string snippet = "";
        private bool editMode;

        public event Action Cancelled;

        public ContextBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);
            Theme.Changed += OnThemeChanged;
        }

        private void OnThemeChanged(object s, EventArgs e) { Invalidate(); }

        public void Set(string newTitle, string newSnippet, bool edit)
        {
            title = newTitle;
            snippet = newSnippet;
            editMode = edit;
            Invalidate();
        }

        private Rectangle CloseRect() { return new Rectangle(12, (Height - 28) / 2, 28, 28); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (CloseRect().Contains(e.Location) && Cancelled != null) Cancelled();
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Cursor = CloseRect().Contains(e.Location) ? Cursors.Hand : Cursors.Default;
            base.OnMouseMove(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Theme.Surface2);
            Color accent = editMode ? Theme.Warning : Theme.Accent;
            UiUtil.FillRound(g, accent, new RectangleF(Width - 14, 8, 3, Height - 16), 1.5f);

            Rectangle r1 = new Rectangle(48, 5, Width - 48 - 26, 18);
            Rectangle r2 = new Rectangle(48, 23, Width - 48 - 26, 18);
            TextRenderer.DrawText(g, title, Theme.Font(9, FontStyle.Bold), r1, accent, UiUtil.Flags(true, false, false, true, true));
            TextRenderer.DrawText(g, snippet, Theme.Font(9.5f), r2, Theme.Muted, UiUtil.Flags(Fa.IsRtl(snippet), false, false, true, true));

            Rectangle c = CloseRect();
            Icons.Close(g, new Rectangle(c.X + 6, c.Y + 6, 16, 16), Theme.Muted);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Theme.Changed -= OnThemeChanged;
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// نشانگر «... در حال تایپ است» با سه نقطه متحرک
    /// </summary>
    public class TypingIndicator : Control
    {
        private string text = "";
        private readonly WinTimer timer = new WinTimer();

        public TypingIndicator()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);
            timer.Interval = 50;
            timer.Tick += delegate { Invalidate(); };
            Theme.Changed += OnThemeChanged;
            Visible = false;
        }

        private void OnThemeChanged(object s, EventArgs e) { Invalidate(); }

        public string TypingText
        {
            get { return text; }
            set
            {
                text = value ?? "";
                if (text.Length > 0) timer.Start(); else timer.Stop();
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Theme.Surface);
            if (text.Length == 0) return;

            // سه نقطه که به ترتیب بالا و پایین می‌روند
            double t = Environment.TickCount / 250.0;
            for (int i = 0; i < 3; i++)
            {
                float dy = (float)Math.Sin(t - i * 0.8) * 3f;
                using (Brush b = new SolidBrush(Theme.Accent))
                {
                    g.FillEllipse(b, Width - 30 + i * 8, Height / 2f - 2.5f + dy, 5, 5);
                }
            }
            TextRenderer.DrawText(g, text, Theme.Font(9, FontStyle.Italic), new Rectangle(14, 0, Width - 52, Height), Theme.Muted,
                UiUtil.Flags(true, false, false, true, true));
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

    /// <summary>
    /// پنل ساده ایموجی: با کلیک روی هر ایموجی، همان در کادر پیام درج می‌شود
    /// </summary>
    public class EmojiPopup : Control
    {
        private static readonly string[] Emojis = {
            "\U0001F600", "\U0001F601", "\U0001F602", "\U0001F923", "\U0001F60A", "\U0001F60D", "\U0001F618", "\U0001F60E",
            "\U0001F914", "\U0001F622", "\U0001F62D", "\U0001F621", "\U0001F44D", "\U0001F44E", "\U0001F44F", "\U0001F64F",
            "\U0001F4AA", "\U0001F525", "\u2764", "\U0001F494", "\U0001F389", "\u2728", "\u2B50", "\U0001F339",
            "\u2615", "\U0001F355", "\U0001F381", "\U0001F680", "\U0001F4AF", "\u2705", "\u274C", "\U0001F44B",
            "\U0001F91D", "\U0001F634", "\U0001F973", "\U0001F929", "\U0001F605", "\U0001F60C", "\U0001F648", "\U0001F340" };

        private const int Cols = 8;
        private const int Cell = 38;
        private const int Pad = 10;
        private int hover = -1;

        public event Action<string> EmojiPicked;

        public EmojiPopup()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);
            int rows = (Emojis.Length + Cols - 1) / Cols;
            Size = new Size(Cols * Cell + 2 * Pad, rows * Cell + 2 * Pad);
            Visible = false;
            Theme.Changed += OnThemeChanged;
        }

        private void OnThemeChanged(object s, EventArgs e) { Invalidate(); }

        private Rectangle CellRect(int i)
        {
            return new Rectangle(Pad + (i % Cols) * Cell, Pad + (i / Cols) * Cell, Cell, Cell);
        }

        private int HitTest(Point p)
        {
            for (int i = 0; i < Emojis.Length; i++) if (CellRect(i).Contains(p)) return i;
            return -1;
        }

        private static Color Tint(string emoji)
        {
            Color t = Theme.EmojiTint(emoji);
            if (t != Theme.Accent) return t;
            int cp = char.ConvertToUtf32(emoji, 0);
            if (cp >= 0x1F600 && cp <= 0x1F64F && cp != 0x1F64F) return Color.FromArgb(245, 158, 11);
            return Theme.Accent;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int h = HitTest(e.Location);
            if (h != hover) { hover = h; Cursor = h >= 0 ? Cursors.Hand : Cursors.Default; Invalidate(); }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) { hover = -1; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            int h = HitTest(e.Location);
            if (h >= 0 && EmojiPicked != null) EmojiPicked(Emojis[h]);
            base.OnMouseDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Theme.Bg);
            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
            using (GraphicsPath p = UiUtil.RoundRect(r, 16))
            using (Brush b = new SolidBrush(Theme.Surface))
            using (Pen pen = new Pen(Theme.Border))
            {
                g.FillPath(b, p);
                g.DrawPath(pen, p);
            }
            for (int i = 0; i < Emojis.Length; i++)
            {
                Rectangle c = CellRect(i);
                if (i == hover) UiUtil.FillRound(g, Theme.Hover, new RectangleF(c.X + 2, c.Y + 2, c.Width - 4, c.Height - 4), 10);
                TextRenderer.DrawText(g, Emojis[i], Theme.EmojiFont(14), c, Tint(Emojis[i]),
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Theme.Changed -= OnThemeChanged;
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// پنل کامل چت: نوار سنجاق، لیست پیام‌ها، نشانگر تایپ، نوار پاسخ/ویرایش، کادر ورودی، دکمه ایموجی و ارسال
    /// هم در کلاینت و هم در سرور (با AdminMode) استفاده می‌شود
    /// </summary>
    public class ChatPanel : Control
    {
        public readonly ChatView View = new ChatView();
        private readonly PinnedBar pinnedBar = new PinnedBar();
        private readonly TypingIndicator typing = new TypingIndicator();
        private readonly ContextBar contextBar = new ContextBar();
        private readonly SurfacePanel inputArea = new SurfacePanel(SurfaceRole.Surface);
        private readonly ModernTextBox input = new ModernTextBox();
        private readonly ModernButton sendBtn = new ModernButton();
        private readonly ModernButton emojiBtn = new ModernButton();
        private readonly EmojiPopup emojiPopup = new EmojiPopup();
        private readonly WinTimer typerTimer = new WinTimer();
        private readonly Dictionary<string, DateTime> typers = new Dictionary<string, DateTime>();

        private ChatMessageInfo replyTo;
        private ChatMessageInfo editing;
        private DateTime lastTypingSent = DateTime.MinValue;
        private bool inputEnabled = true;

        public event Action<string, long> SendRequested;           // متن، شناسه پیام پاسخ داده شده (۰ = بدون پاسخ)
        public event Action<long, string> EditSubmitted;           // شناسه پیام، متن جدید
        public event Action<ChatMessageInfo> DeleteRequested;
        public event Action<ChatMessageInfo, string> ReactRequested;
        public event Action<ChatMessageInfo> PinToggleRequested;
        public event Action<long> UnpinRequested;
        public event Action TypingNotify;

        public ChatPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(700, 500);

            // دکمه ارسال و ایموجی
            sendBtn.Kind = BtnKind.Primary;
            sendBtn.Radius = 23;
            sendBtn.Size = new Size(46, 46);
            sendBtn.IconPainter = Icons.Send;
            sendBtn.Click += delegate { Submit(); };

            emojiBtn.Kind = BtnKind.Secondary;
            emojiBtn.Radius = 23;
            emojiBtn.Size = new Size(46, 46);
            emojiBtn.IconPainter = Icons.Smile;
            emojiBtn.Click += delegate { emojiPopup.Visible = !emojiPopup.Visible; if (emojiPopup.Visible) emojiPopup.BringToFront(); };

            // کادر ورودی چندخطی راست‌به‌چپ
            input.Multiline = true;
            input.Rtl = true;
            input.Hint = "پیام خود را بنویسید...";
            input.Inner.KeyDown += OnInputKeyDown;
            input.Inner.TextChanged += OnInputTextChanged;

            inputArea.Controls.Add(sendBtn);
            inputArea.Controls.Add(input);
            inputArea.Controls.Add(emojiBtn);

            Controls.Add(emojiPopup);
            Controls.Add(typing);
            Controls.Add(contextBar);
            Controls.Add(inputArea);
            Controls.Add(pinnedBar);
            Controls.Add(View);
            emojiPopup.BringToFront();

            pinnedBar.Visible = false;
            contextBar.Visible = false;

            // اتصال رویدادها
            View.ReplyRequested += BeginReply;
            View.EditRequested += BeginEdit;
            View.DeleteRequested += delegate(ChatMessageInfo m) { if (DeleteRequested != null) DeleteRequested(m); };
            View.ReactRequested += delegate(ChatMessageInfo m, string emoji) { if (ReactRequested != null) ReactRequested(m, emoji); };
            View.PinToggleRequested += delegate(ChatMessageInfo m) { if (PinToggleRequested != null) PinToggleRequested(m); };
            contextBar.Cancelled += CancelContext;
            pinnedBar.PinClicked += delegate(PinInfo p) { View.ScrollToMessage(p.Id); };
            pinnedBar.UnpinClicked += delegate(long id) { if (UnpinRequested != null) UnpinRequested(id); };
            emojiPopup.EmojiPicked += delegate(string emoji)
            {
                input.Inner.SelectedText = emoji;
                input.FocusInner();
            };

            typerTimer.Interval = 500;
            typerTimer.Tick += OnTyperTick;
            typerTimer.Start();

            Theme.Changed += OnThemeChanged;
            LayoutChildren();
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MyName
        {
            get { return View.MyName; }
            set { View.MyName = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AdminMode
        {
            get { return View.AdminMode; }
            set { View.AdminMode = value; pinnedBar.ShowClose = value; }
        }

        public void FocusInput() { input.FocusInner(); }

        private void OnThemeChanged(object s, EventArgs e) { Invalidate(true); }

        // ---------- پیام‌های سنجاق شده ----------
        public void SetPins(List<PinInfo> pins)
        {
            HashSet<long> ids = new HashSet<long>();
            foreach (PinInfo p in pins) ids.Add(p.Id);
            View.PinnedIds = ids;
            pinnedBar.SetPins(pins);
            pinnedBar.Visible = pins.Count > 0;
            LayoutChildren();
        }

        // ---------- فعال / غیرفعال کردن ورودی ----------
        public void SetInputEnabled(bool enabled, string disabledHint)
        {
            inputEnabled = enabled;
            input.Enabled = enabled;
            sendBtn.Enabled = enabled;
            emojiBtn.Enabled = enabled;
            input.Hint = enabled ? "پیام خود را بنویسید..." : disabledHint;
            if (!enabled)
            {
                emojiPopup.Visible = false;
                CancelContext();
            }
        }

        // ---------- نشانگر در حال تایپ ----------
        public void AddTyper(string name)
        {
            typers[name] = DateTime.Now.AddSeconds(3.5);
            UpdateTypingText();
        }

        public void RemoveTyper(string name)
        {
            if (typers.Remove(name)) UpdateTypingText();
        }

        private void OnTyperTick(object s, EventArgs e)
        {
            List<string> expired = new List<string>();
            foreach (KeyValuePair<string, DateTime> kv in typers) if (kv.Value < DateTime.Now) expired.Add(kv.Key);
            foreach (string n in expired) typers.Remove(n);
            if (expired.Count > 0) UpdateTypingText();
        }

        private void UpdateTypingText()
        {
            List<string> names = new List<string>(typers.Keys);
            string t = "";
            if (names.Count == 1) t = names[0] + " در حال تایپ است";
            else if (names.Count == 2) t = names[0] + " و " + names[1] + " در حال تایپ هستند";
            else if (names.Count > 2) t = "چند نفر در حال تایپ هستند";
            bool show = t.Length > 0;
            typing.TypingText = t;
            if (typing.Visible != show)
            {
                typing.Visible = show;
                LayoutChildren();
            }
        }

        // ---------- پاسخ و ویرایش ----------
        private void BeginReply(ChatMessageInfo m)
        {
            if (!inputEnabled) return;
            editing = null;
            replyTo = m;
            contextBar.Set("پاسخ به " + m.Sender, ChatText.Snippet(m.Text, 70), false);
            contextBar.Visible = true;
            LayoutChildren();
            input.FocusInner();
        }

        private void BeginEdit(ChatMessageInfo m)
        {
            if (!inputEnabled) return;
            replyTo = null;
            editing = m;
            contextBar.Set("ویرایش پیام", ChatText.Snippet(m.Text, 70), true);
            contextBar.Visible = true;
            input.Text = m.Text;
            input.Inner.SelectionStart = input.Inner.TextLength;
            LayoutChildren();
            input.FocusInner();
        }

        private void CancelContext()
        {
            bool wasEditing = editing != null;
            replyTo = null;
            editing = null;
            contextBar.Visible = false;
            if (wasEditing) input.Text = "";
            LayoutChildren();
        }

        // ---------- ارسال ----------
        private void Submit()
        {
            if (!inputEnabled) return;
            string text = ChatText.LimitText(input.Text);
            if (text.Length == 0) return;

            ChatMessageInfo edit = editing;
            ChatMessageInfo reply = replyTo;
            replyTo = null;
            editing = null;
            contextBar.Visible = false;
            input.Text = "";
            emojiPopup.Visible = false;
            LayoutChildren();

            if (edit != null)
            {
                if (text != edit.Text && EditSubmitted != null) EditSubmitted(edit.Id, text);
            }
            else if (SendRequested != null)
            {
                SendRequested(text, reply != null ? reply.Id : 0);
            }
            lastTypingSent = DateTime.MinValue;
        }

        private void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                // Enter = ارسال ، Shift+Enter = خط جدید
                e.SuppressKeyPress = true;
                e.Handled = true;
                Submit();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                emojiPopup.Visible = false;
                CancelContext();
            }
        }

        private void OnInputTextChanged(object sender, EventArgs e)
        {
            // اطلاع به سرور که در حال تایپ هستیم (حداکثر هر ۲ ثانیه یک بار)
            if (editing == null && input.Text.Length > 0 && (DateTime.Now - lastTypingSent).TotalSeconds > 2)
            {
                lastTypingSent = DateTime.Now;
                if (TypingNotify != null) TypingNotify();
            }
        }

        // ---------- چیدمان ----------
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutChildren();
        }

        private void LayoutChildren()
        {
            if (View == null || Width < 10 || Height < 10) return;
            int w = Width, h = Height;
            const int inputH = 70;

            int pinH = pinnedBar.Visible ? 46 : 0;
            int ctxH = contextBar.Visible ? 46 : 0;
            int typH = typing.Visible ? 24 : 0;

            pinnedBar.SetBounds(0, 0, w, pinH);
            inputArea.SetBounds(0, h - inputH, w, inputH);
            contextBar.SetBounds(0, h - inputH - ctxH, w, ctxH);
            typing.SetBounds(0, h - inputH - ctxH - typH, w, typH);
            View.SetBounds(0, pinH, w, Math.Max(20, h - pinH - inputH - ctxH - typH));

            // داخل ناحیه ورودی: ارسال سمت چپ، ایموجی سمت راست و کادر متن در وسط
            sendBtn.Location = new Point(14, 12);
            emojiBtn.Location = new Point(w - 14 - 46, 12);
            input.SetBounds(14 + 46 + 10, 11, Math.Max(60, w - 2 * (14 + 46 + 10)), 48);

            emojiPopup.Location = new Point(Math.Max(4, w - emojiPopup.Width - 14), Math.Max(4, h - inputH - ctxH - emojiPopup.Height - 6));
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.Bg);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Theme.Changed -= OnThemeChanged;
                typerTimer.Stop();
                typerTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
