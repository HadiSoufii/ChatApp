using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WinTimer = System.Windows.Forms.Timer;

namespace ChatpApp.Shared
{
    /// <summary>
    /// کنترل نمایش پیام‌های چت (کاملا سفارشی و دستی رسم می‌شود)
    /// امکانات: حباب پیام با آواتار، نام و ساعت، پاسخ (Reply)، واکنش‌ها، تیک وضعیت، پیام سیستمی، کارت خوشامدگویی،
    /// اسکرول نرم، نوار ابزار شناور هنگام هاور، منوی راست‌کلیک (ویرایش / حذف / سنجاق) و پرش به پیام اصلی
    /// </summary>
    public class ChatView : Control
    {
        private enum ItemKind { Message, System, Welcome }

        private class ChipInfo
        {
            public Rectangle Rect;
            public string Emoji;
        }

        // هر ردیف لیست (پیام / پیام سیستمی / کارت خوشامد) یک ViewItem است
        private class ViewItem
        {
            public ItemKind Kind;
            public ChatMessageInfo Msg;

            // پیام سیستمی
            public string SysText = "";
            public DateTime SysTime;
            public int SysKind;          // ۰ = اطلاعات ، ۱ = ورود ، ۲ = خروج/هشدار

            // کارت خوشامد
            public string WelcomeName = "";
            public int WelcomeAvatar;
            public string WelcomeText = "";
            public int WelcomeOnline;

            // چیدمان (مختصات داخل ردیف؛ مختصات اجزای حباب نسبت به گوشه حباب است)
            public int Y, Height;
            public Rectangle Bubble, AvatarRect, NameRect, ReplyRect, TextRect, FooterRect;
            public Rectangle CardRect, TitleRect, WTextRect, WChipRect, SysRect;
            public List<ChipInfo> Chips = new List<ChipInfo>();
            public bool TextRtl = true;
            public string Shown = "";
            public float Appear = 1f;     // انیمیشن ظاهر شدن (۰ تا ۱)
        }

        // ---------- ثابت‌های چیدمان ----------
        private const int PadX = 14;
        private const int AvatarSize = 38;
        private const int AvatarGap = 8;
        private const int BubblePadX = 12;
        private const int BubblePadTop = 8;
        private const int BubblePadBottom = 6;
        private const int ItemPadY = 5;
        private const int TopPad = 10;
        private const int BottomPad = 10;
        private const int MaxItems = 600;

        private readonly List<ViewItem> items = new List<ViewItem>();
        private float scrollY, scrollTarget;
        private int contentHeight = TopPad + BottomPad;
        private readonly WinTimer timer = new WinTimer();

        private ViewItem hoverItem;
        private int hoverToolbarBtn = -1;
        private bool hoverToBottom;
        private bool draggingThumb;
        private int dragStartY;
        private float dragStartScroll;
        private long flashId;
        private float flashT;
        private int newCount;

        // ---------- تنظیمات و رویدادها ----------
        public string MyName = "";                      // نام کاربر فعلی (برای تشخیص پیام‌های خودم)
        public bool AdminMode;                          // در سرور true است (حذف هر پیام و سنجاق کردن)
        public HashSet<long> PinnedIds = new HashSet<long>();

        public event Action<ChatMessageInfo> ReplyRequested;
        public event Action<ChatMessageInfo> EditRequested;
        public event Action<ChatMessageInfo> DeleteRequested;
        public event Action<ChatMessageInfo> PinToggleRequested;
        public event Action<ChatMessageInfo, string> ReactRequested;

        public ChatView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, true);
            TabStop = false;
            timer.Interval = 16;
            timer.Tick += OnTick;
            Theme.Changed += OnThemeChanged;
        }

        private void OnThemeChanged(object s, EventArgs e) { Invalidate(); }

        // ==================================================================================
        // API عمومی
        // ==================================================================================

        /// <summary>
        /// اضافه کردن یک پیام کاربر یا سرور
        /// </summary>
        public void AddMessage(ChatMessageInfo m, bool animate)
        {
            ViewItem it = new ViewItem();
            it.Kind = ItemKind.Message;
            it.Msg = m;
            AddItem(it, animate, IsMine(m));
        }

        /// <summary>
        /// اضافه کردن پیام سیستمی (مثلا «علی وارد چت شد»)
        /// </summary>
        public void AddSystem(string text, DateTime time, int kind)
        {
            ViewItem it = new ViewItem();
            it.Kind = ItemKind.System;
            it.SysText = text;
            it.SysTime = time;
            it.SysKind = kind;
            AddItem(it, true, false);
        }

        /// <summary>
        /// اضافه کردن یک خط جداکننده بدون ساعت (مثلا «پیام‌های قبلی»)
        /// </summary>
        public void AddDivider(string text)
        {
            ViewItem it = new ViewItem();
            it.Kind = ItemKind.System;
            it.SysText = text;
            it.SysTime = DateTime.MinValue;
            it.SysKind = 0;
            AddItem(it, false, false);
        }

        /// <summary>
        /// اضافه کردن کارت زیبای خوشامدگویی
        /// </summary>
        public void AddWelcome(string name, int avatar, string text, int online)
        {
            ViewItem it = new ViewItem();
            it.Kind = ItemKind.Welcome;
            it.WelcomeName = name;
            it.WelcomeAvatar = avatar;
            it.WelcomeText = text;
            it.WelcomeOnline = online;
            AddItem(it, true, true);
        }

        public ChatMessageInfo Find(long id)
        {
            foreach (ViewItem it in items)
            {
                if (it.Kind == ItemKind.Message && it.Msg.Id == id) return it.Msg;
            }
            return null;
        }

        private ViewItem FindItem(long id)
        {
            foreach (ViewItem it in items)
            {
                if (it.Kind == ItemKind.Message && it.Msg.Id == id) return it;
            }
            return null;
        }

        // تغییر شناسه موقت پیام به شناسه واقعی سرور
        public void ReplaceId(long oldId, long newId)
        {
            ViewItem it = FindItem(oldId);
            if (it != null) it.Msg.Id = newId;
        }

        // تغییر وضعیت تحویل پیام (تیک‌ها)
        public void SetStatus(long id, int status)
        {
            ViewItem it = FindItem(id);
            if (it != null && status > it.Msg.Status)
            {
                it.Msg.Status = status;
                Invalidate();
            }
        }

        // ویرایش متن پیام
        public void SetEdited(long id, string text)
        {
            ViewItem it = FindItem(id);
            if (it == null) return;
            it.Msg.Text = text;
            it.Msg.Edited = true;
            Relayout(true);
        }

        // حذف پیام (به جای آن عبارت «این پیام حذف شد» نشان داده می‌شود)
        public void MarkDeleted(long id)
        {
            ViewItem it = FindItem(id);
            if (it == null) return;
            it.Msg.Deleted = true;
            it.Msg.Reactions.Clear();
            Relayout(true);
        }

        // به‌روزرسانی واکنش‌های پیام
        public void SetReactions(long id, List<ReactionInfo> reactions)
        {
            ViewItem it = FindItem(id);
            if (it == null) return;
            it.Msg.Reactions = reactions;
            Relayout(true);
        }

        public void Clear()
        {
            items.Clear();
            contentHeight = TopPad + BottomPad;
            scrollY = scrollTarget = 0;
            newCount = 0;
            hoverItem = null;
            Invalidate();
        }

        // پرش به یک پیام مشخص و چشمک زدن آن
        public bool ScrollToMessage(long id)
        {
            ViewItem it = FindItem(id);
            if (it == null) return false;
            scrollTarget = ClampScroll(it.Y - Height / 3f);
            flashId = id;
            flashT = 1f;
            StartTimer();
            return true;
        }

        public void ScrollToBottom()
        {
            scrollTarget = MaxScroll;
            newCount = 0;
            StartTimer();
            Invalidate();
        }

        /// <summary>
        /// پیام‌های دیگران که الان در صفحه دیده می‌شوند و هنوز «خوانده شد» آن‌ها گزارش نشده را برمی‌گرداند
        /// </summary>
        public List<ChatMessageInfo> TakeVisibleUnread()
        {
            List<ChatMessageInfo> list = new List<ChatMessageInfo>();
            foreach (ViewItem it in items)
            {
                if (it.Kind != ItemKind.Message) continue;
                ChatMessageInfo m = it.Msg;
                if (m.Deleted || m.ReadReported || IsMine(m) || m.Id <= 0) continue;

                float mid = it.Y + it.Height / 2f;
                if (mid >= scrollY && mid <= scrollY + Height)
                {
                    m.ReadReported = true;
                    list.Add(m);
                }
            }
            return list;
        }

        // ==================================================================================
        // چیدمان
        // ==================================================================================

        private bool IsMine(ChatMessageInfo m)
        {
            return MyName.Length > 0 && m.Sender == MyName;
        }

        private float MaxScroll { get { return Math.Max(0, contentHeight - Height); } }

        private bool AtBottom { get { return scrollTarget >= MaxScroll - 40; } }

        private float ClampScroll(float v)
        {
            return Math.Max(0, Math.Min(MaxScroll, v));
        }

        private void AddItem(ViewItem it, bool animate, bool forceScroll)
        {
            bool wasBottom = AtBottom;
            it.Appear = animate ? 0f : 1f;

            // اگر تعداد ردیف‌ها زیاد شد، قدیمی‌ترها حذف می‌شوند
            if (items.Count >= MaxItems)
            {
                int remove = 60;
                int removedH = 0;
                for (int i = 0; i < remove; i++) removedH += items[i].Height;
                items.RemoveRange(0, remove);
                RelayoutNoScroll();
                scrollY = Math.Max(0, scrollY - removedH);
                scrollTarget = Math.Max(0, scrollTarget - removedH);
            }

            items.Add(it);
            LayoutItem(it);
            it.Y = contentHeight - BottomPad;
            contentHeight = it.Y + it.Height + BottomPad;

            bool isOtherMessage = it.Kind == ItemKind.Message && !IsMine(it.Msg);
            if (wasBottom || forceScroll)
            {
                scrollTarget = MaxScroll;
                if (!animate) scrollY = scrollTarget;
            }
            else if (isOtherMessage)
            {
                newCount++;
            }

            StartTimer();
            Invalidate();
        }

        // چیدمان دوباره همه ردیف‌ها (بعد از تغییر عرض یا محتوا)
        private void RelayoutNoScroll()
        {
            int y = TopPad;
            foreach (ViewItem it in items)
            {
                LayoutItem(it);
                it.Y = y;
                y += it.Height;
            }
            contentHeight = y + BottomPad;
        }

        private void Relayout(bool keepBottom)
        {
            bool wasBottom = AtBottom;
            RelayoutNoScroll();
            if (keepBottom && wasBottom) scrollTarget = MaxScroll;
            scrollTarget = ClampScroll(scrollTarget);
            scrollY = Math.Min(scrollY, MaxScroll);
            StartTimer();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Width > 50 && Height > 50) Relayout(true);
        }

        private void LayoutItem(ViewItem it)
        {
            if (it.Kind == ItemKind.Message) LayoutMessage(it);
            else if (it.Kind == ItemKind.System) LayoutSystem(it);
            else LayoutWelcome(it);
        }

        private int ViewW { get { return Math.Max(240, ClientSize.Width); } }

        private void LayoutMessage(ViewItem it)
        {
            ChatMessageInfo m = it.Msg;
            bool mine = IsMine(m);
            int avail = ViewW - 2 * PadX;
            int maxBubble = Math.Min(560, avail - AvatarSize - AvatarGap - 20);
            if (maxBubble < 150) maxBubble = 150;
            int innerMax = maxBubble - 2 * BubblePadX;

            Font fName = Theme.Font(9, FontStyle.Bold);
            Font fText = Theme.Font(10.5f);
            Font fSmall = Theme.Font(8);
            Font fRepName = Theme.Font(8.5f, FontStyle.Bold);
            Font fRepText = Theme.Font(9);

            it.TextRtl = Fa.IsRtl(m.Deleted ? "ا" : m.Text);
            it.Shown = m.Deleted ? "این پیام حذف شد" : m.Text.Replace("\r\n", "\n").Replace("\n", "\r\n");

            // نام
            int contentW = Math.Min(UiUtil.MeasureLine(m.Sender, fName).Width, innerMax);

            // پاسخ
            bool hasReply = m.ReplyId != 0 && !m.Deleted;
            if (hasReply)
            {
                int w1 = UiUtil.MeasureLine(m.ReplyName, fRepName).Width;
                int w2 = UiUtil.MeasureLine(ChatText.Snippet(m.ReplyText, 60), fRepText).Width;
                contentW = Math.Max(contentW, Math.Min(innerMax, Math.Max(w1, w2) + 22));
            }

            // متن
            TextFormatFlags messageFlags = UiUtil.Flags(it.TextRtl, true, false, false, false);
            Size ts = TextRenderer.MeasureText(it.Shown,
                m.Deleted ? Theme.Font(10, FontStyle.Italic) : fText,
                new Size(innerMax, 10000), messageFlags);
            // فضای عمودی کمی اضافه می‌کنیم تا نقطه‌های بالا و پایین حروف فارسی بریده نشوند.
            ts.Height = Math.Max(20, ts.Height + 4);
            contentW = Math.Max(contentW, Math.Min(ts.Width, innerMax));

            // واکنش‌ها: عرض هر چیپ و چیدمان در ردیف‌ها
            List<ReactionInfo> rs = new List<ReactionInfo>();
            if (!m.Deleted)
            {
                foreach (ReactionInfo r in m.Reactions) if (r.Users.Count > 0) rs.Add(r);
            }
            List<int> chipWidths = new List<int>();
            foreach (ReactionInfo r in rs)
            {
                int cw = UiUtil.MeasureLine(Fa.Digits(r.Users.Count), Theme.Font(8.5f, FontStyle.Bold)).Width;
                chipWidths.Add(6 + 18 + 3 + cw + 9);
            }
            List<List<int>> rows = new List<List<int>>();
            List<int> rowW = new List<int>();
            for (int i = 0; i < chipWidths.Count; i++)
            {
                if (rows.Count == 0 || rowW[rows.Count - 1] + chipWidths[i] + 6 > innerMax)
                {
                    rows.Add(new List<int>());
                    rowW.Add(0);
                }
                rows[rows.Count - 1].Add(i);
                rowW[rows.Count - 1] += chipWidths[i] + 6;
            }
            int chipsW = 0;
            foreach (int w in rowW) chipsW = Math.Max(chipsW, w - 6);
            contentW = Math.Max(contentW, chipsW);

            // پاورقی: ساعت + «ویرایش شده» + تیک‌ها
            string timeText = Fa.Time(m.Time);
            int timeW = UiUtil.MeasureLine(timeText, fSmall).Width;
            int editedW = (m.Edited && !m.Deleted) ? UiUtil.MeasureLine("ویرایش شده", fSmall).Width + 8 : 0;
            int tickW = (mine && m.Kind == "user" && !m.Deleted) ? 28 : 0;
            contentW = Math.Max(contentW, timeW + editedW + tickW);
            contentW = Math.Max(60, Math.Min(contentW, innerMax));

            // ساخت مستطیل‌ها (مختصات نسبت به گوشه حباب)
            int y = BubblePadTop;
            it.NameRect = new Rectangle(BubblePadX, y, contentW, 16);
            y += 18;
            if (hasReply)
            {
                it.ReplyRect = new Rectangle(BubblePadX, y, contentW, 40);
                y += 44;
            }
            else
            {
                it.ReplyRect = Rectangle.Empty;
            }
            it.TextRect = new Rectangle(BubblePadX, y, contentW, ts.Height + 2);
            y += ts.Height + 5;

            it.Chips.Clear();
            for (int r = 0; r < rows.Count; r++)
            {
                int x = it.TextRtl ? BubblePadX + contentW : BubblePadX;
                foreach (int idx in rows[r])
                {
                    int w = chipWidths[idx];
                    ChipInfo ci = new ChipInfo();
                    ci.Emoji = rs[idx].Emoji;
                    if (it.TextRtl)
                    {
                        x -= w;
                        ci.Rect = new Rectangle(x, y, w, 22);
                        x -= 6;
                    }
                    else
                    {
                        ci.Rect = new Rectangle(x, y, w, 22);
                        x += w + 6;
                    }
                    it.Chips.Add(ci);
                }
                y += 26;
            }

            it.FooterRect = new Rectangle(BubblePadX, y, contentW, 14);
            y += 16;

            int bubbleW = contentW + 2 * BubblePadX;
            int bubbleH = y + BubblePadBottom;

            int bx, ax;
            if (mine)
            {
                ax = ViewW - PadX - AvatarSize;
                bx = ax - AvatarGap - bubbleW;
            }
            else
            {
                ax = PadX;
                bx = ax + AvatarSize + AvatarGap;
            }
            it.Bubble = new Rectangle(bx, ItemPadY, bubbleW, bubbleH);
            it.AvatarRect = new Rectangle(ax, ItemPadY, AvatarSize, AvatarSize);
            it.Height = bubbleH + 2 * ItemPadY;
        }

        private void LayoutSystem(ViewItem it)
        {
            string full = it.SysTime == DateTime.MinValue ? it.SysText : it.SysText + "   " + Fa.Time(it.SysTime);
            int tw = UiUtil.MeasureLine(full, Theme.Font(9)).Width;
            int w = Math.Min(ViewW - 40, tw + 38);
            it.SysRect = new Rectangle((ViewW - w) / 2, 4, w, 26);
            it.Height = 34;
        }

        private void LayoutWelcome(ViewItem it)
        {
            int avail = ViewW - 2 * PadX;
            int cardW = Math.Min(avail, 480);
            int textW = cardW - 56;
            bool rtl = Fa.IsRtl(it.WelcomeText);
            Size ts = TextRenderer.MeasureText(it.WelcomeText, Theme.Font(10.5f), new Size(textW, 0), UiUtil.Flags(rtl, true, true, false, false));
            int y = 22;
            int avatarBottom = y + 72;
            it.TitleRect = new Rectangle(16, avatarBottom + 8, cardW - 32, 30);
            it.WTextRect = new Rectangle(28, avatarBottom + 42, cardW - 56, ts.Height + 2);
            int chipY = it.WTextRect.Bottom + 12;
            string chipText = Fa.Digits(it.WelcomeOnline) + " نفر در چت آنلاین هستند";
            int cw = UiUtil.MeasureLine(chipText, Theme.Font(9.5f, FontStyle.Bold)).Width + 44;
            it.WChipRect = new Rectangle((cardW - cw) / 2, chipY, cw, 28);
            int cardH = it.WChipRect.Bottom + 22;
            it.CardRect = new Rectangle((ViewW - cardW) / 2, ItemPadY + 6, cardW, cardH);
            it.Height = cardH + 2 * ItemPadY + 12;
        }

        // ==================================================================================
        // انیمیشن
        // ==================================================================================

        private void StartTimer()
        {
            if (!timer.Enabled) timer.Start();
        }

        private void OnTick(object s, EventArgs e)
        {
            bool again = false;

            // انیمیشن ظاهر شدن پیام‌های تازه
            for (int i = items.Count - 1; i >= 0 && i >= items.Count - 12; i--)
            {
                if (items[i].Appear < 1f)
                {
                    items[i].Appear = Math.Min(1f, items[i].Appear + 0.08f);
                    again = true;
                }
            }

            // اسکرول نرم
            scrollTarget = ClampScroll(scrollTarget);
            if (Math.Abs(scrollTarget - scrollY) > 0.6f)
            {
                scrollY += (scrollTarget - scrollY) * 0.28f;
                again = true;
            }
            else
            {
                scrollY = scrollTarget;
            }

            // چشمک زدن پیام پیدا شده
            if (flashT > 0f)
            {
                flashT = Math.Max(0f, flashT - 0.012f);
                again = true;
            }

            if (AtBottom && newCount > 0) newCount = 0;

            Invalidate();
            if (!again) timer.Stop();
        }

        // ==================================================================================
        // رسم
        // ==================================================================================

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (Width < 4 || Height < 4) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // پس‌زمینه گرادیان با دو دایره محو تزئینی
            using (LinearGradientBrush bg = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), Theme.ChatBgTop, Theme.ChatBgBottom, 90f))
            {
                g.FillRectangle(bg, 0, 0, Width, Height);
            }
            using (Brush deco = new SolidBrush(Color.FromArgb(Theme.IsDark ? 12 : 20, Theme.Accent)))
            {
                g.FillEllipse(deco, Width * 0.55f, Height * 0.05f, 280, 280);
                g.FillEllipse(deco, -90, Height * 0.55f, 260, 260);
            }

            Rectangle clip = e.ClipRectangle;
            foreach (ViewItem it in items)
            {
                int top = (int)(it.Y - scrollY);
                if (top + it.Height < clip.Top - 60 || top > clip.Bottom + 60) continue;
                DrawItem(g, it);
            }

            DrawToolbar(g);
            DrawScrollbar(g);
            DrawToBottom(g);
        }

        // جابه‌جایی مستطیل برای رسم در مختصات نهایی کنترل؛ از تبدیل Graphics استفاده نمی‌شود.
        private static Rectangle OffsetRectY(Rectangle rect, int offsetY)
        {
            rect.Offset(0, offsetY);
            return rect;
        }

        // مستطیل‌های داخل حباب نسبت به گوشه بالای چپ حباب ذخیره شده‌اند؛
        // هنگام رسم باید هم جابه‌جایی افقی حباب و هم جابه‌جایی عمودی اسکرول/انیمیشن اعمال شود.
        private static Rectangle OffsetBubbleRect(Rectangle rect, Rectangle bubble, int offsetY)
        {
            // Bubble.Y هم بخشی از مختصات واقعی کارت است؛ آن را نیز به مختصات داخلی اضافه می‌کنیم.
            rect.Offset(bubble.X, bubble.Y + offsetY);
            return rect;
        }

        private void DrawItem(Graphics g, ViewItem it)
        {
            // انیمیشن ورود فقط مقدار Y را تغییر می‌دهد؛ از TranslateTransform استفاده نمی‌کنیم،
            // چون TextRenderer در بعضی نسخه‌های GDI مختصات تبدیل‌شده Graphics را رعایت نمی‌کند.
            float ease = 1f - (1f - it.Appear) * (1f - it.Appear);
            float off = (1f - ease) * 18f;
            int offsetY = (int)Math.Round(it.Y - scrollY + off);

            if (it.Kind == ItemKind.Message) DrawMessage(g, it, offsetY);
            else if (it.Kind == ItemKind.System) DrawSystem(g, it, offsetY);
            else DrawWelcome(g, it, offsetY);
        }

        private void DrawSystem(Graphics g, ViewItem it, int offsetY)
        {
            // مستطیل پیام سیستمی به مختصات نهایی صفحه منتقل می‌شود.
            Rectangle r = OffsetRectY(it.SysRect, offsetY);
            UiUtil.FillRound(g, Theme.SystemChip, r, r.Height / 2f);

            // نقطه رنگی بر اساس نوع پیام
            Color dot = it.SysKind == 1 ? Theme.Success : (it.SysKind == 2 ? Theme.Danger : Theme.Accent);
            if (it.SysTime == DateTime.MinValue) dot = Theme.Muted;
            using (Brush b = new SolidBrush(dot)) g.FillEllipse(b, r.Right - 18, r.Y + 9, 8, 8);

            string full = it.SysTime == DateTime.MinValue ? it.SysText : it.SysText + "   " + Fa.Time(it.SysTime);
            Rectangle tr = new Rectangle(r.X + 10, r.Y, r.Width - 34, r.Height);
            TextRenderer.DrawText(g, full, Theme.Font(9), tr, Theme.SystemText,
                UiUtil.Flags(true, false, true, true, true));
        }

        private void DrawWelcome(Graphics g, ViewItem it, int offsetY)
        {
            // کارت خوشامدگویی هم با مختصات نهایی رسم می‌شود تا متن و شکل‌ها دقیقاً هم‌تراز باشند.
            Rectangle card = OffsetRectY(it.CardRect, offsetY);
            using (GraphicsPath p = UiUtil.RoundRect(card, 24))
            {
                // سایه کارت
                UiUtil.FillRound(g, Color.FromArgb(Theme.IsDark ? 50 : 28, 30, 20, 90),
                    new RectangleF(card.X + 2, card.Y + 6, card.Width, card.Height), 24);

                using (LinearGradientBrush gb = new LinearGradientBrush(card, Theme.Accent, Theme.AccentAlt, 35f))
                    g.FillPath(gb, p);

                // تزئینات نیمه‌شفاف داخل کارت
                GraphicsState st = g.Save();
                g.SetClip(p, CombineMode.Intersect);
                using (Brush w1 = new SolidBrush(Color.FromArgb(28, 255, 255, 255)))
                using (Brush w2 = new SolidBrush(Color.FromArgb(18, 255, 255, 255)))
                {
                    g.FillEllipse(w1, card.Right - 110, card.Y - 60, 200, 200);
                    g.FillEllipse(w2, card.X - 70, card.Bottom - 90, 190, 190);
                    g.FillEllipse(w2, card.X + 20, card.Y + 14, 14, 14);
                    g.FillEllipse(w1, card.Right - 46, card.Y + 70, 10, 10);
                    g.FillEllipse(w1, card.X + 42, card.Y + 120, 8, 8);
                }
                g.Restore(st);
            }

            // آواتار بزرگ با حلقه سفید
            RectangleF av = new RectangleF(card.X + card.Width / 2f - 36, card.Y + 22, 72, 72);
            using (Pen ring = new Pen(Color.FromArgb(230, 255, 255, 255), 3.5f))
            {
                RectangleF rr = av;
                rr.Inflate(3, 3);
                g.DrawEllipse(ring, rr);
            }
            AvatarPainter.Draw(g, it.WelcomeAvatar, av);

            // مختصات عنوان و متن نسبت به کارت هستند و ابتدا به مختصات صفحه منتقل می‌شوند.
            Rectangle title = new Rectangle(card.X + it.TitleRect.X, card.Y + it.TitleRect.Y,
                it.TitleRect.Width, it.TitleRect.Height);
            TextRenderer.DrawText(g, "سلام " + it.WelcomeName + " عزیز!", Theme.Font(15, FontStyle.Bold),
                title, Color.White, UiUtil.Flags(true, false, true, true, true));

            Rectangle wt = new Rectangle(card.X + it.WTextRect.X, card.Y + it.WTextRect.Y,
                it.WTextRect.Width, it.WTextRect.Height);
            TextRenderer.DrawText(g, it.WelcomeText, Theme.Font(10.5f), wt,
                Color.FromArgb(235, 255, 255, 255),
                UiUtil.Flags(Fa.IsRtl(it.WelcomeText), true, true, false, false));

            // قرص تعداد کاربران آنلاین
            Rectangle chip = new Rectangle(card.X + it.WChipRect.X, card.Y + it.WChipRect.Y,
                it.WChipRect.Width, it.WChipRect.Height);
            UiUtil.FillRound(g, Color.FromArgb(48, 255, 255, 255), chip, chip.Height / 2f);
            using (Brush gb = new SolidBrush(Color.FromArgb(74, 222, 128)))
                g.FillEllipse(gb, chip.Right - 22, chip.Y + 9, 10, 10);
            TextRenderer.DrawText(g, Fa.Digits(it.WelcomeOnline) + " نفر در چت آنلاین هستند",
                Theme.Font(9.5f, FontStyle.Bold), new Rectangle(chip.X + 6, chip.Y, chip.Width - 32, chip.Height),
                Color.White, UiUtil.Flags(true, false, true, true, true));
        }

        private void DrawMessage(Graphics g, ViewItem it, int offsetY)
        {
            ChatMessageInfo m = it.Msg;
            bool mine = IsMine(m);
            bool server = m.Kind == "server";
            bool dark = Theme.IsDark;

            // آواتار و حباب هر دو در مختصات نهایی صفحه رسم می‌شوند.
            Rectangle avatar = OffsetRectY(it.AvatarRect, offsetY);
            using (Pen ring = new Pen(Theme.Surface, 2f)) g.DrawEllipse(ring, avatar);
            AvatarPainter.Draw(g, m.Avatar, avatar);

            Rectangle b = OffsetRectY(it.Bubble, offsetY);
            RectangleF br = new RectangleF(b.X, b.Y, b.Width, b.Height);
            float big = 16, small = 4;
            using (GraphicsPath path = mine
                ? UiUtil.RoundRect(br, big, small, big, big)
                : UiUtil.RoundRect(br, small, big, big, big))
            {
                if (!dark && !m.Deleted)
                {
                    RectangleF shadowRect = new RectangleF(b.X, b.Y + 2, b.Width, b.Height);
                    using (GraphicsPath sp = mine
                        ? UiUtil.RoundRect(shadowRect, big, small, big, big)
                        : UiUtil.RoundRect(shadowRect, small, big, big, big))
                    using (Brush sb = new SolidBrush(Color.FromArgb(22, 40, 50, 110)))
                        g.FillPath(sb, sp);
                }

                if (m.Deleted)
                {
                    using (Brush fb = new SolidBrush(Theme.Surface2)) g.FillPath(fb, path);
                    using (Pen bp = new Pen(Theme.Border, 1f)) g.DrawPath(bp, path);
                }
                else if (server)
                {
                    using (LinearGradientBrush fb = new LinearGradientBrush(br, Theme.BubbleServerA, Theme.BubbleServerB, 45f))
                        g.FillPath(fb, path);
                    using (Pen bp = new Pen(Color.FromArgb(110, Theme.Warning), 1f)) g.DrawPath(bp, path);
                }
                else if (mine)
                {
                    using (LinearGradientBrush fb = new LinearGradientBrush(br, Theme.Accent, Theme.AccentAlt, 45f))
                        g.FillPath(fb, path);
                }
                else
                {
                    using (Brush fb = new SolidBrush(Theme.BubbleOther)) g.FillPath(fb, path);
                    using (Pen bp = new Pen(Theme.Border, 1f)) g.DrawPath(bp, path);
                }

                // هایلایت موقت هنگام پرش به پیام اصلی
                if (flashT > 0f && flashId == m.Id)
                {
                    using (Brush fl = new SolidBrush(Color.FromArgb((int)(130 * flashT),
                        mine && !server ? Color.White : Theme.Accent)))
                        g.FillPath(fl, path);
                }
            }

            bool onAccent = mine && !server && !m.Deleted;
            Color textColor = m.Deleted ? Theme.Muted : (server ? Theme.ServerText : (mine ? Color.White : Theme.Text));
            Color metaColor = onAccent ? Color.FromArgb(205, 255, 255, 255)
                : (server ? Color.FromArgb(170, Theme.ServerText) : Theme.Muted);
            Color nameColor = server ? (dark ? Color.FromArgb(253, 224, 71) : Color.FromArgb(180, 83, 9))
                : (onAccent ? Color.FromArgb(235, 255, 255, 255) : Fa.NameColor(m.Sender, dark));

            // نام فرستنده با همان مختصات حباب
            TextRenderer.DrawText(g, m.Sender, Theme.Font(9, FontStyle.Bold),
                OffsetBubbleRect(it.NameRect, it.Bubble, offsetY), nameColor,
                UiUtil.Flags(it.TextRtl, false, false, true, true));

            // بلوک پاسخ
            if (!it.ReplyRect.IsEmpty)
                DrawReplyBlock(g, it, onAccent, server, offsetY);

            // متن پیام بدون NoClipping رسم می‌شود تا از محدوده حباب خارج نشود.
            // TextRect نسبت به حباب است؛ مختصات X حباب را هم اضافه می‌کنیم تا متن داخل همان کارت رسم شود.
            Rectangle textRect = OffsetBubbleRect(it.TextRect, it.Bubble, offsetY);
            TextRenderer.DrawText(g, it.Shown,
                m.Deleted ? Theme.Font(10, FontStyle.Italic) : Theme.Font(10.5f),
                textRect, textColor, UiUtil.Flags(it.TextRtl, true, false, false, false));

            // واکنش‌ها
            foreach (ChipInfo ci in it.Chips)
            {
                ReactionInfo ri = null;
                foreach (ReactionInfo r in m.Reactions)
                    if (r.Emoji == ci.Emoji) { ri = r; break; }
                if (ri != null) DrawChip(g, ci, ri, onAccent, server, it.Bubble, offsetY);
            }

            // ساعت، برچسب ویرایش و وضعیت تحویل
            DrawFooter(g, it, metaColor, mine, offsetY);
        }

        private void DrawReplyBlock(Graphics g, ViewItem it, bool onAccent, bool server, int offsetY)
        {
            ChatMessageInfo m = it.Msg;
            Rectangle rr = OffsetBubbleRect(it.ReplyRect, it.Bubble, offsetY);
            Color fill = onAccent ? Color.FromArgb(48, 255, 255, 255)
                : (server ? Color.FromArgb(40, 120, 70, 10)
                : Color.FromArgb(Theme.IsDark ? 48 : 30, Theme.Accent));
            UiUtil.FillRound(g, fill, rr, 8);

            Color barColor = onAccent ? Color.White : Theme.Accent;
            RectangleF bar = it.TextRtl
                ? new RectangleF(rr.Right - 4, rr.Y + 4, 3, rr.Height - 8)
                : new RectangleF(rr.X + 1, rr.Y + 4, 3, rr.Height - 8);
            UiUtil.FillRound(g, barColor, bar, 1.5f);

            Rectangle nameR = new Rectangle(rr.X + 10, rr.Y + 3, rr.Width - 20, 16);
            Rectangle textR = new Rectangle(rr.X + 10, rr.Y + 20, rr.Width - 20, 16);
            TextRenderer.DrawText(g, m.ReplyName, Theme.Font(8.5f, FontStyle.Bold), nameR,
                barColor, UiUtil.Flags(it.TextRtl, false, false, true, true));
            TextRenderer.DrawText(g, ChatText.Snippet(m.ReplyText, 60), Theme.Font(9), textR,
                onAccent ? Color.FromArgb(220, 255, 255, 255) : Theme.Muted,
                UiUtil.Flags(it.TextRtl, false, false, true, true));
        }

        private void DrawChip(Graphics g, ChipInfo ci, ReactionInfo ri, bool onAccent, bool server, Rectangle bubble, int offsetY)
        {
            Rectangle r = OffsetBubbleRect(ci.Rect, bubble, offsetY);
            bool iReacted = ri.Users.Contains(MyName);
            Color fill;
            if (onAccent) fill = Color.FromArgb(iReacted ? 95 : 42, 255, 255, 255);
            else if (server) fill = Color.FromArgb(iReacted ? 80 : 40, 120, 70, 10);
            else fill = Color.FromArgb(iReacted ? 70 : 26, Theme.Accent);

            using (GraphicsPath p = UiUtil.RoundRect(r, 11))
            using (Brush fb = new SolidBrush(fill))
            {
                g.FillPath(fb, p);
                if (iReacted)
                {
                    using (Pen bp = new Pen(onAccent ? Color.White : Theme.Accent, 1.2f))
                        g.DrawPath(bp, p);
                }
            }

            Color tint = onAccent ? Color.White : Theme.EmojiTint(ri.Emoji);
            TextRenderer.DrawText(g, ri.Emoji, Theme.EmojiFont(10),
                new Rectangle(r.X + 5, r.Y, 20, r.Height), tint,
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine |
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(g, Fa.Digits(ri.Users.Count), Theme.Font(8.5f, FontStyle.Bold),
                new Rectangle(r.X + 26, r.Y, r.Width - 28, r.Height),
                onAccent ? Color.White : Theme.Text,
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine |
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        private void DrawFooter(Graphics g, ViewItem it, Color meta, bool mine, int offsetY)
        {
            ChatMessageInfo m = it.Msg;
            Font fSmall = Theme.Font(8);
            string timeText = Fa.Time(m.Time);
            int timeW = UiUtil.MeasureLine(timeText, fSmall).Width;
            bool edited = m.Edited && !m.Deleted;
            int editedW = edited ? UiUtil.MeasureLine("ویرایش شده", fSmall).Width + 8 : 0;
            bool ticks = mine && m.Kind == "user" && !m.Deleted;
            int tickW = ticks ? 28 : 0;
            int total = timeW + editedW + tickW;

            Rectangle fr = OffsetBubbleRect(it.FooterRect, it.Bubble, offsetY);
            int x = it.TextRtl ? fr.X : fr.Right - total;
            TextFormatFlags f = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine |
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter;

            // ترتیب از چپ به راست: در متن فارسی [تیک][ساعت][ویرایش شده]
            if (it.TextRtl)
            {
                if (ticks) { DrawTicksFor(g, x, fr.Y, m, meta); x += tickW; }
                TextRenderer.DrawText(g, timeText, fSmall,
                    new Rectangle(x, fr.Y, timeW + 2, fr.Height), meta, f);
                x += timeW;
                if (edited)
                    TextRenderer.DrawText(g, "ویرایش شده", fSmall,
                        new Rectangle(x + 8, fr.Y, editedW, fr.Height), meta,
                        f | TextFormatFlags.RightToLeft);
            }
            else
            {
                if (edited)
                {
                    TextRenderer.DrawText(g, "ویرایش شده", fSmall,
                        new Rectangle(x, fr.Y, editedW, fr.Height), meta,
                        f | TextFormatFlags.RightToLeft);
                    x += editedW;
                }
                TextRenderer.DrawText(g, timeText, fSmall,
                    new Rectangle(x, fr.Y, timeW + 2, fr.Height), meta, f);
                x += timeW;
                if (ticks) DrawTicksFor(g, x + 6, fr.Y, m, meta);
            }
        }

        private void DrawTicksFor(Graphics g, int x, int y, ChatMessageInfo m, Color meta)
        {
            int count = Math.Max(0, Math.Min(2, m.Status)) + 1;
            Color c = m.Status >= 2 ? Color.FromArgb(120, 255, 200) : meta;
            Icons.Ticks(g, x, y + 1, count, c);
        }

        // ---------- نوار ابزار شناور (واکنش‌ها و پاسخ) ----------

        private bool ToolbarAllowed(ViewItem it)
        {
            return it != null && it.Kind == ItemKind.Message && !it.Msg.Deleted && it.Msg.Id > 0;
        }

        private Rectangle BubbleView(ViewItem it)
        {
            return new Rectangle(it.Bubble.X, (int)(it.Y - scrollY) + it.Bubble.Y, it.Bubble.Width, it.Bubble.Height);
        }

        private Rectangle ToolbarRect(ViewItem it)
        {
            Rectangle bv = BubbleView(it);
            int w = 5 * 30 + 8, h = 34;
            int x = IsMine(it.Msg) ? bv.Right - w : bv.X;
            x = Math.Max(4, Math.Min(Width - w - 12, x));
            int y = Math.Max(2, bv.Y - 22);
            return new Rectangle(x, y, w, h);
        }

        private void DrawToolbar(Graphics g)
        {
            if (!ToolbarAllowed(hoverItem)) return;
            Rectangle tb = ToolbarRect(hoverItem);

            UiUtil.FillRound(g, Color.FromArgb(Theme.IsDark ? 70 : 34, 20, 25, 80), new RectangleF(tb.X, tb.Y + 2, tb.Width, tb.Height), 17);
            using (GraphicsPath p = UiUtil.RoundRect(tb, 17))
            using (Brush fb = new SolidBrush(Theme.Surface))
            using (Pen bp = new Pen(Theme.Border, 1f))
            {
                g.FillPath(fb, p);
                g.DrawPath(bp, p);
            }

            for (int i = 0; i < 5; i++)
            {
                Rectangle br = new Rectangle(tb.X + 4 + i * 30, tb.Y + 3, 30, 28);
                if (i == hoverToolbarBtn)
                {
                    UiUtil.FillRound(g, Theme.Hover, br, 14);
                }

                if (i < 4)
                {
                    string emoji = ChatConst.Reactions[i];
                    TextRenderer.DrawText(g, emoji, Theme.EmojiFont(12), br, Theme.EmojiTint(emoji),
                        TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                else
                {
                    Icons.Reply(g, new Rectangle(br.X + 6, br.Y + 5, 18, 18), i == hoverToolbarBtn ? Theme.Accent : Theme.Muted);
                }
            }
        }

        // ---------- اسکرول‌بار و دکمه رفتن به پایین ----------

        private Rectangle ThumbRect()
        {
            if (contentHeight <= Height) return Rectangle.Empty;
            int thumbH = Math.Max(36, (int)((float)Height * Height / contentHeight));
            float ratio = MaxScroll <= 0 ? 0 : scrollY / MaxScroll;
            int y = (int)(ratio * (Height - thumbH));
            return new Rectangle(Width - 9, y, 6, thumbH);
        }

        private void DrawScrollbar(Graphics g)
        {
            Rectangle t = ThumbRect();
            if (t.IsEmpty) return;
            UiUtil.FillRound(g, Color.FromArgb(draggingThumb ? 150 : 80, Theme.Muted), t, 3);
        }

        private Rectangle ToBottomRect()
        {
            return new Rectangle(Width - 58, Height - 58, 40, 40);
        }

        private void DrawToBottom(Graphics g)
        {
            if (AtBottom && newCount == 0) return;
            if (contentHeight <= Height) return;
            Rectangle r = ToBottomRect();

            UiUtil.FillRound(g, Color.FromArgb(40, 20, 25, 80), new RectangleF(r.X, r.Y + 2, r.Width, r.Height), 20);
            using (Brush fb = new SolidBrush(hoverToBottom ? Theme.Hover : Theme.Surface))
            using (Pen bp = new Pen(Theme.Border, 1f))
            {
                g.FillEllipse(fb, r);
                g.DrawEllipse(bp, r);
            }
            Icons.ChevronDown(g, new Rectangle(r.X + 9, r.Y + 9, 22, 22), Theme.Accent);

            if (newCount > 0)
            {
                Rectangle bd = new Rectangle(r.X - 4, r.Y - 6, 22, 20);
                UiUtil.FillRound(g, Theme.Accent, bd, 10);
                TextRenderer.DrawText(g, Fa.Digits(Math.Min(newCount, 99)), Theme.Font(8, FontStyle.Bold), bd, Color.White,
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        // ==================================================================================
        // ماوس
        // ==================================================================================

        private ViewItem ItemAt(int y)
        {
            float yy = y + scrollY;
            foreach (ViewItem it in items)
            {
                if (yy >= it.Y && yy < it.Y + it.Height) return it;
            }
            return null;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            scrollTarget = ClampScroll(scrollTarget - e.Delta * 0.8f);
            StartTimer();
            base.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // کشیدن اسکرول‌بار
            if (draggingThumb)
            {
                int track = Math.Max(1, Height - ThumbRect().Height);
                float delta = (e.Y - dragStartY) / (float)track * MaxScroll;
                scrollY = scrollTarget = ClampScroll(dragStartScroll + delta);
                Invalidate();
                return;
            }

            ViewItem hit = null;
            int tbBtn = -1;

            // اول نوار ابزار ردیف فعلی را بررسی می‌کنیم (چون روی حاشیه حباب شناور است)
            if (ToolbarAllowed(hoverItem))
            {
                Rectangle tb = ToolbarRect(hoverItem);
                if (tb.Contains(e.Location))
                {
                    hit = hoverItem;
                    tbBtn = Math.Max(0, Math.Min(4, (e.X - tb.X - 4) / 30));
                }
            }

            if (hit == null)
            {
                ViewItem it = ItemAt(e.Y);
                if (it != null && it.Kind == ItemKind.Message && BubbleView(it).Contains(e.Location)) hit = it;
            }

            bool toBottom = (!AtBottom || newCount > 0) && contentHeight > Height && ToBottomRect().Contains(e.Location);

            // مکان‌نما
            bool hand = tbBtn >= 0 || toBottom || ChipAt(hit, e.Location) != null || InReplyBlock(hit, e.Location);
            Cursor = hand ? Cursors.Hand : Cursors.Default;

            if (hit != hoverItem || tbBtn != hoverToolbarBtn || toBottom != hoverToBottom)
            {
                hoverItem = hit;
                hoverToolbarBtn = tbBtn;
                hoverToBottom = toBottom;
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hoverItem = null;
            hoverToolbarBtn = -1;
            hoverToBottom = false;
            Cursor = Cursors.Default;
            Invalidate();
            base.OnMouseLeave(e);
        }

        private ChipInfo ChipAt(ViewItem it, Point p)
        {
            if (it == null || it.Kind != ItemKind.Message || it.Msg.Deleted) return null;
            Rectangle bv = BubbleView(it);
            foreach (ChipInfo ci in it.Chips)
            {
                Rectangle r = new Rectangle(bv.X + ci.Rect.X, bv.Y + ci.Rect.Y, ci.Rect.Width, ci.Rect.Height);
                if (r.Contains(p)) return ci;
            }
            return null;
        }

        private bool InReplyBlock(ViewItem it, Point p)
        {
            if (it == null || it.Kind != ItemKind.Message || it.ReplyRect.IsEmpty) return false;
            Rectangle bv = BubbleView(it);
            Rectangle r = new Rectangle(bv.X + it.ReplyRect.X, bv.Y + it.ReplyRect.Y, it.ReplyRect.Width, it.ReplyRect.Height);
            return r.Contains(p);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            if (e.Button == MouseButtons.Left)
            {
                // دکمه رفتن به پایین
                if (hoverToBottom)
                {
                    ScrollToBottom();
                    return;
                }

                // اسکرول‌بار
                if (e.X >= Width - 16 && contentHeight > Height)
                {
                    Rectangle thumb = ThumbRect();
                    if (e.Y >= thumb.Y && e.Y <= thumb.Bottom)
                    {
                        draggingThumb = true;
                        dragStartY = e.Y;
                        dragStartScroll = scrollY;
                    }
                    else
                    {
                        scrollTarget = ClampScroll(scrollTarget + (e.Y < thumb.Y ? -Height : Height) * 0.9f);
                        StartTimer();
                    }
                    return;
                }

                // دکمه‌های نوار ابزار
                if (hoverItem != null && hoverToolbarBtn >= 0 && ToolbarAllowed(hoverItem))
                {
                    ChatMessageInfo m = hoverItem.Msg;
                    if (hoverToolbarBtn < 4)
                    {
                        if (ReactRequested != null) ReactRequested(m, ChatConst.Reactions[hoverToolbarBtn]);
                    }
                    else
                    {
                        if (ReplyRequested != null) ReplyRequested(m);
                    }
                    return;
                }

                // کلیک روی چیپ واکنش یا بلوک پاسخ
                ViewItem it = ItemAt(e.Y);
                ChipInfo chip = ChipAt(it, e.Location);
                if (chip != null)
                {
                    if (ReactRequested != null && it.Msg.Id > 0) ReactRequested(it.Msg, chip.Emoji);
                    return;
                }
                if (InReplyBlock(it, e.Location))
                {
                    ScrollToMessage(it.Msg.ReplyId);
                    return;
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            draggingThumb = false;
            if (e.Button == MouseButtons.Right)
            {
                ViewItem it = ItemAt(e.Y);
                if (it != null && it.Kind == ItemKind.Message && BubbleView(it).Contains(e.Location))
                {
                    ShowMenu(it.Msg, e.Location);
                }
            }
            base.OnMouseUp(e);
        }

        // ==================================================================================
        // منوی راست‌کلیک
        // ==================================================================================

        private void ShowMenu(ChatMessageInfo m, Point p)
        {
            if (m.Deleted) return;

            bool mine = IsMine(m);
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.RightToLeft = RightToLeft.Yes;
            menu.Renderer = new ThemedMenuRenderer();
            menu.Font = Theme.Font(10);
            menu.BackColor = Theme.Surface;
            menu.ShowImageMargin = false;

            if (m.Id > 0) AddMenuItem(menu, "پاسخ دادن", false, delegate { if (ReplyRequested != null) ReplyRequested(m); });
            AddMenuItem(menu, "کپی متن", false, delegate
            {
                try { Clipboard.SetText(m.Text); } catch { }
            });

            if (m.Id > 0 && mine)
            {
                AddMenuItem(menu, "ویرایش پیام", false, delegate { if (EditRequested != null) EditRequested(m); });
            }

            if (m.Id > 0 && AdminMode)
            {
                bool pinned = PinnedIds.Contains(m.Id);
                AddMenuItem(menu, pinned ? "برداشتن سنجاق" : "سنجاق کردن پیام", false, delegate { if (PinToggleRequested != null) PinToggleRequested(m); });
            }

            if (m.Id > 0 && (mine || AdminMode))
            {
                menu.Items.Add(new ToolStripSeparator());
                AddMenuItem(menu, mine ? "حذف پیام" : "حذف پیام (مدیر)", true, delegate { if (DeleteRequested != null) DeleteRequested(m); });
            }

            menu.Closed += delegate
            {
                try { BeginInvoke(new Action(menu.Dispose)); } catch { }
            };
            menu.Show(this, p);
        }

        private static void AddMenuItem(ContextMenuStrip menu, string text, bool danger, Action action)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.Padding = new Padding(8, 5, 8, 5);
            if (danger) item.Tag = "danger";
            item.Click += delegate { action(); };
            menu.Items.Add(item);
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
