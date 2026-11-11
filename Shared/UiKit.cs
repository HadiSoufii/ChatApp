using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WinTimer = System.Windows.Forms.Timer;

// کنترل‌های سفارشی و ابزارهای رسم مشترک بین سرور و کلاینت
namespace ChatpApp.Shared
{
    /// <summary>
    /// کنترلی که یک رنگ سطح (پس‌زمینه) دارد؛ کنترل‌های داخلش گوشه‌های گردشان را با همین رنگ پر می‌کنند
    /// </summary>
    public interface ISurface
    {
        Color SurfaceColor { get; }
    }

    /// <summary>
    /// توابع کمکی رسم
    /// </summary>
    public static class UiUtil
    {
        // ساخت مسیر مستطیل با گوشه‌های گرد (هر گوشه شعاع جدا)
        public static GraphicsPath RoundRect(RectangleF r, float tl, float tr, float br, float bl)
        {
            float max = Math.Max(1f, Math.Min(r.Width, r.Height) / 2f);
            tl = Math.Max(0.5f, Math.Min(tl, max));
            tr = Math.Max(0.5f, Math.Min(tr, max));
            br = Math.Max(0.5f, Math.Min(br, max));
            bl = Math.Max(0.5f, Math.Min(bl, max));

            GraphicsPath p = new GraphicsPath();
            p.AddArc(r.Left, r.Top, tl * 2, tl * 2, 180, 90);
            p.AddArc(r.Right - tr * 2, r.Top, tr * 2, tr * 2, 270, 90);
            p.AddArc(r.Right - br * 2, r.Bottom - br * 2, br * 2, br * 2, 0, 90);
            p.AddArc(r.Left, r.Bottom - bl * 2, bl * 2, bl * 2, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static GraphicsPath RoundRect(RectangleF r, float radius)
        {
            return RoundRect(r, radius, radius, radius, radius);
        }

        // پر کردن یک مستطیل گرد با رنگ ساده
        public static void FillRound(Graphics g, Color c, RectangleF r, float radius)
        {
            using (GraphicsPath p = RoundRect(r, radius))
            using (Brush b = new SolidBrush(c))
            {
                g.FillPath(b, p);
            }
        }

        // رنگ پس‌زمینه پدر کنترل (برای پر کردن گوشه‌های گرد)
        public static Color ParentBack(Control c)
        {
            Control p = c.Parent;
            while (p != null)
            {
                ISurface s = p as ISurface;
                if (s != null) return s.SurfaceColor;
                if (p.BackColor.A == 255 && p.BackColor != Color.Transparent) return p.BackColor;
                p = p.Parent;
            }
            return Theme.Bg;
        }

        // پرچم‌های رسم متن با TextRenderer
        public static TextFormatFlags Flags(bool rtl, bool wrap, bool center, bool vcenter, bool ellipsis)
        {
            TextFormatFlags f = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
            f |= wrap ? TextFormatFlags.WordBreak : TextFormatFlags.SingleLine;
            if (rtl)
            {
                f |= TextFormatFlags.RightToLeft;
                if (!center) f |= TextFormatFlags.Right;
            }
            if (center) f |= TextFormatFlags.HorizontalCenter;
            if (vcenter) f |= TextFormatFlags.VerticalCenter;
            if (ellipsis) f |= TextFormatFlags.EndEllipsis;
            return f;
        }

        // اندازه یک متن تک‌خطی
        public static Size MeasureLine(string text, Font font)
        {
            if (string.IsNullOrEmpty(text)) return Size.Empty;
            return TextRenderer.MeasureText(text, font, new Size(int.MaxValue / 4, int.MaxValue / 4),
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        }

        // رسم متن تک‌خطی با خودکار تشخیص جهت
        public static void DrawLine(Graphics g, string text, Font font, Color color, Rectangle r, bool center)
        {
            if (string.IsNullOrEmpty(text)) return;
            bool rtl = Fa.IsRtl(text);
            TextRenderer.DrawText(g, text, font, r, color, Flags(rtl, false, center, true, true));
        }
    }

    /// <summary>
    /// آیکون‌های برداری که با کد کشیده می‌شوند (بدون نیاز به فایل تصویر)
    /// </summary>
    public static class Icons
    {
        private static PointF P(Rectangle r, float x, float y)
        {
            return new PointF(r.X + r.Width * x, r.Y + r.Height * y);
        }

        private static Pen MakePen(Color c, float w)
        {
            Pen p = new Pen(c, w);
            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.Round;
            p.LineJoin = LineJoin.Round;
            return p;
        }

        // آیکون ارسال (فلش رو به چپ چون جهت برنامه راست‌به‌چپ است)
        public static void Send(Graphics g, Rectangle r, Color c)
        {
            PointF[] pts = { P(r, 0.06f, 0.5f), P(r, 0.94f, 0.08f), P(r, 0.72f, 0.5f), P(r, 0.94f, 0.92f) };
            using (Brush b = new SolidBrush(c)) g.FillPolygon(b, pts);
        }

        // آیکون ایموجی (صورتک)
        public static void Smile(Graphics g, Rectangle r, Color c)
        {
            using (Pen p = MakePen(c, 1.7f))
            using (Brush b = new SolidBrush(c))
            {
                g.DrawEllipse(p, r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2);
                g.DrawArc(p, r.X + r.Width * 0.26f, r.Y + r.Height * 0.3f, r.Width * 0.48f, r.Height * 0.42f, 20, 140);
                g.FillEllipse(b, r.X + r.Width * 0.32f, r.Y + r.Height * 0.32f, r.Width * 0.1f, r.Height * 0.12f);
                g.FillEllipse(b, r.X + r.Width * 0.58f, r.Y + r.Height * 0.32f, r.Width * 0.1f, r.Height * 0.12f);
            }
        }

        // آیکون پاسخ (فلش برگشتی)
        public static void Reply(Graphics g, Rectangle r, Color c)
        {
            using (Pen p = MakePen(c, 1.9f))
            {
                g.DrawLines(p, new PointF[] { P(r, 0.42f, 0.18f), P(r, 0.16f, 0.46f), P(r, 0.42f, 0.74f) });
                g.DrawBezier(p, P(r, 0.16f, 0.46f), P(r, 0.7f, 0.38f), P(r, 0.88f, 0.5f), P(r, 0.84f, 0.84f));
            }
        }

        // آیکون بستن (ضربدر)
        public static void Close(Graphics g, Rectangle r, Color c)
        {
            using (Pen p = MakePen(c, 1.9f))
            {
                g.DrawLine(p, P(r, 0.22f, 0.22f), P(r, 0.78f, 0.78f));
                g.DrawLine(p, P(r, 0.78f, 0.22f), P(r, 0.22f, 0.78f));
            }
        }

        // آیکون سنجاق
        public static void Pin(Graphics g, Rectangle r, Color c)
        {
            using (Pen p = MakePen(c, 1.8f))
            using (Brush b = new SolidBrush(c))
            {
                g.FillEllipse(b, r.X + r.Width * 0.3f, r.Y + r.Height * 0.08f, r.Width * 0.4f, r.Height * 0.4f);
                g.DrawLine(p, P(r, 0.2f, 0.5f), P(r, 0.8f, 0.5f));
                g.DrawLine(p, P(r, 0.5f, 0.5f), P(r, 0.5f, 0.92f));
            }
        }

        // آیکون بلندگو (روشن)
        public static void SpeakerOn(Graphics g, Rectangle r, Color c)
        {
            Speaker(g, r, c, true);
        }

        // آیکون بلندگو (خاموش)
        public static void SpeakerOff(Graphics g, Rectangle r, Color c)
        {
            Speaker(g, r, c, false);
        }

        private static void Speaker(Graphics g, Rectangle r, Color c, bool on)
        {
            PointF[] pts = { P(r, 0.08f, 0.38f), P(r, 0.28f, 0.38f), P(r, 0.5f, 0.16f), P(r, 0.5f, 0.84f), P(r, 0.28f, 0.62f), P(r, 0.08f, 0.62f) };
            using (Brush b = new SolidBrush(c)) g.FillPolygon(b, pts);
            using (Pen p = MakePen(c, 1.7f))
            {
                if (on)
                {
                    g.DrawArc(p, r.X + r.Width * 0.38f, r.Y + r.Height * 0.3f, r.Width * 0.38f, r.Height * 0.4f, -50, 100);
                    g.DrawArc(p, r.X + r.Width * 0.3f, r.Y + r.Height * 0.14f, r.Width * 0.6f, r.Height * 0.72f, -50, 100);
                }
                else
                {
                    g.DrawLine(p, P(r, 0.66f, 0.36f), P(r, 0.92f, 0.64f));
                    g.DrawLine(p, P(r, 0.92f, 0.36f), P(r, 0.66f, 0.64f));
                }
            }
        }

        // آیکون فلش پایین
        public static void ChevronDown(Graphics g, Rectangle r, Color c)
        {
            using (Pen p = MakePen(c, 2.2f))
            {
                g.DrawLines(p, new PointF[] { P(r, 0.22f, 0.36f), P(r, 0.5f, 0.64f), P(r, 0.78f, 0.36f) });
            }
        }

        // آیکون بازگشت (فلش به راست، چون در راست‌به‌چپ «جلو» به سمت چپ است و «بازگشت» به سمت راست)
        public static void Back(Graphics g, Rectangle r, Color c)
        {
            using (Pen p = MakePen(c, 2f))
            {
                g.DrawLine(p, P(r, 0.15f, 0.5f), P(r, 0.85f, 0.5f));
                g.DrawLines(p, new PointF[] { P(r, 0.55f, 0.2f), P(r, 0.85f, 0.5f), P(r, 0.55f, 0.8f) });
            }
        }

        // آیکون قطع اتصال (دو خط کج شبیه دوشاخه)
        public static void Unplug(Graphics g, Rectangle r, Color c)
        {
            using (Pen p = MakePen(c, 1.9f))
            {
                g.DrawLine(p, P(r, 0.5f, 0.1f), P(r, 0.5f, 0.5f));
                g.DrawArc(p, r.X + r.Width * 0.18f, r.Y + r.Height * 0.2f, r.Width * 0.64f, r.Height * 0.64f, -60, 300);
            }
        }

        // آیکون تیک ساده
        public static void Check(Graphics g, Rectangle r, Color c)
        {
            using (Pen p = MakePen(c, 2.2f))
            {
                g.DrawLines(p, new PointF[] { P(r, 0.2f, 0.52f), P(r, 0.42f, 0.74f), P(r, 0.82f, 0.28f) });
            }
        }

        /// <summary>
        /// رسم تیک‌های وضعیت پیام: ۱ تیک = ارسال شد ، ۲ تیک = به سرور رسید ، ۳ تیک = خوانده شد
        /// </summary>
        public static void Ticks(Graphics g, float x, float y, int count, Color c)
        {
            SmoothingMode old = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen p = MakePen(c, 1.5f))
            {
                for (int i = 0; i < count; i++)
                {
                    float ox = x + i * 4.5f;
                    g.DrawLines(p, new PointF[] { new PointF(ox, y + 5.5f), new PointF(ox + 3.2f, y + 8.8f), new PointF(ox + 9f, y + 1.8f) });
                }
            }
            g.SmoothingMode = old;
        }
    }

    /// <summary>
    /// آواتارهای برداری: ۳ آواتار دخترانه (۰ تا ۲) و ۳ آواتار پسرانه (۳ تا ۵) و یک آواتار مخصوص سرور (۱-)
    /// همه با کد کشیده می‌شوند و در هر اندازه‌ای شفاف و تمیز هستند
    /// </summary>
    public static class AvatarPainter
    {
        private static readonly Color[] BgTop = {
            Color.FromArgb(249, 168, 212), Color.FromArgb(94, 234, 212), Color.FromArgb(253, 224, 71),
            Color.FromArgb(147, 197, 253), Color.FromArgb(110, 231, 183), Color.FromArgb(253, 186, 116) };
        private static readonly Color[] BgBottom = {
            Color.FromArgb(192, 132, 252), Color.FromArgb(59, 130, 246), Color.FromArgb(244, 63, 94),
            Color.FromArgb(99, 102, 241), Color.FromArgb(16, 185, 129), Color.FromArgb(239, 68, 68) };
        private static readonly Color[] Skin = {
            Color.FromArgb(255, 219, 190), Color.FromArgb(238, 190, 150), Color.FromArgb(255, 228, 205),
            Color.FromArgb(255, 219, 190), Color.FromArgb(224, 172, 132), Color.FromArgb(255, 228, 205) };
        private static readonly Color[] Hair = {
            Color.FromArgb(76, 44, 32), Color.FromArgb(201, 92, 44), Color.FromArgb(32, 32, 42),
            Color.FromArgb(58, 38, 30), Color.FromArgb(24, 24, 30), Color.FromArgb(96, 62, 40) };
        private static readonly Color[] Shirt = {
            Color.FromArgb(190, 24, 93), Color.FromArgb(15, 118, 110), Color.FromArgb(180, 83, 9),
            Color.FromArgb(30, 64, 175), Color.FromArgb(4, 120, 87), Color.FromArgb(154, 52, 18) };

        /// <summary>
        /// کشیدن آواتار داخل مستطیل r به شکل دایره
        /// </summary>
        public static void Draw(Graphics g, int id, RectangleF r)
        {
            GraphicsState st = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(r.X, r.Y);
            g.ScaleTransform(r.Width / 100f, r.Height / 100f);

            using (GraphicsPath clip = new GraphicsPath())
            {
                clip.AddEllipse(0, 0, 100, 100);
                g.SetClip(clip, CombineMode.Intersect);
            }

            if (id < 0) DrawServer(g);
            else DrawPerson(g, id % ChatConst.AvatarCount);

            g.Restore(st);
        }

        private static void FillE(Graphics g, Color c, float x, float y, float w, float h)
        {
            using (Brush b = new SolidBrush(c)) g.FillEllipse(b, x, y, w, h);
        }

        private static void FillPie(Graphics g, Color c, float x, float y, float w, float h, float start, float sweep)
        {
            using (Brush b = new SolidBrush(c)) g.FillPie(b, x, y, w, h, start, sweep);
        }

        private static void FillR(Graphics g, Color c, float x, float y, float w, float h)
        {
            using (Brush b = new SolidBrush(c)) g.FillRectangle(b, x, y, w, h);
        }

        private static void FillRR(Graphics g, Color c, float x, float y, float w, float h, float rad)
        {
            UiUtil.FillRound(g, c, new RectangleF(x, y, w, h), rad);
        }

        private static void FillPoly(Graphics g, Color c, PointF[] pts)
        {
            using (Brush b = new SolidBrush(c)) g.FillPolygon(b, pts);
        }

        private static void DrawPerson(Graphics g, int i)
        {
            // پس‌زمینه گرادیان
            using (LinearGradientBrush bg = new LinearGradientBrush(new RectangleF(0, 0, 100, 100), BgTop[i], BgBottom[i], 90f))
            {
                g.FillRectangle(bg, 0, 0, 100, 100);
            }

            Color skin = Skin[i];
            Color hair = Hair[i];

            // موهای پشت سر (دختران)
            if (i == 0) FillRR(g, hair, 23, 22, 54, 66, 22);          // مو بلند
            if (i == 2) FillE(g, hair, 21, 22, 58, 54);               // مو کوتاه (بابِ) 
            if (i == 1) FillE(g, hair, 39, 7, 22, 22);                // بان (گوجه) بالای سر

            // بدن (لباس) و گردن
            FillE(g, Shirt[i], 12, 76, 76, 64);
            FillR(g, Color.FromArgb(235, skin.R - 18 < 0 ? 0 : skin.R - 18, skin.G - 20 < 0 ? 0 : skin.G - 20, skin.B - 20 < 0 ? 0 : skin.B - 20), 43, 60, 14, 22);

            // سر
            FillE(g, skin, 30, 24, 40, 46);

            // موهای جلوی سر
            if (i == 0 || i == 1 || i == 2) FillPie(g, hair, 27, 19, 46, 42, 180, 180);
            if (i == 3) { FillPie(g, hair, 27, 19, 46, 40, 180, 180); FillR(g, hair, 29, 36, 4, 9); FillR(g, hair, 67, 36, 4, 9); }
            if (i == 4)
            {
                PointF[] spikes = { new PointF(29, 38), new PointF(31, 19), new PointF(39, 29), new PointF(43, 10), new PointF(50, 27),
                                    new PointF(57, 10), new PointF(61, 29), new PointF(69, 19), new PointF(71, 38) };
                FillPoly(g, hair, spikes);
            }
            if (i == 5)
            {
                FillR(g, hair, 29, 36, 4, 9); FillR(g, hair, 67, 36, 4, 9);
                FillPie(g, Color.FromArgb(37, 99, 235), 26, 16, 48, 44, 180, 180);
                FillRR(g, Color.FromArgb(29, 78, 216), 24, 35, 52, 7, 3);
            }

            // اکسسوری دخترانه: گیره قرمز برای بان و پاپیون برای مو کوتاه
            if (i == 1) FillE(g, Color.FromArgb(239, 68, 68), 45, 25, 10, 6);
            if (i == 2)
            {
                Color bow = Color.FromArgb(244, 63, 94);
                FillPoly(g, bow, new PointF[] { new PointF(68, 24), new PointF(57, 16), new PointF(57, 32) });
                FillPoly(g, bow, new PointF[] { new PointF(68, 24), new PointF(79, 16), new PointF(79, 32) });
                FillE(g, Color.FromArgb(190, 18, 60), 65, 21, 6, 6);
            }

            // چشم‌ها
            Color eye = Color.FromArgb(40, 32, 44);
            FillE(g, eye, 39, 46, 5, 6.5f);
            FillE(g, eye, 56, 46, 5, 6.5f);
            FillE(g, Color.White, 40.6f, 47, 1.8f, 1.8f);
            FillE(g, Color.White, 57.6f, 47, 1.8f, 1.8f);

            // عینک (پسر شماره ۴)
            if (i == 4)
            {
                using (Pen gp = new Pen(Color.FromArgb(30, 30, 40), 1.8f))
                {
                    g.DrawEllipse(gp, 35, 43, 14, 13);
                    g.DrawEllipse(gp, 51, 43, 14, 13);
                    g.DrawLine(gp, 49, 48, 51, 48);
                }
            }

            // لبخند
            using (Pen sp = new Pen(Color.FromArgb(170, 60, 60), 2.2f))
            {
                sp.StartCap = LineCap.Round;
                sp.EndCap = LineCap.Round;
                g.DrawArc(sp, 41, 51, 18, 12, 15, 150);
            }

            // گونه‌های صورتی دخترانه
            if (i < 3)
            {
                FillE(g, Color.FromArgb(110, 255, 110, 150), 32, 55, 8, 5);
                FillE(g, Color.FromArgb(110, 255, 110, 150), 60, 55, 8, 5);
            }
        }

        // آواتار مخصوص سرور: سه کشوی سرور روی پس‌زمینه سبز-آبی
        private static void DrawServer(Graphics g)
        {
            using (LinearGradientBrush bg = new LinearGradientBrush(new RectangleF(0, 0, 100, 100), Color.FromArgb(45, 212, 191), Color.FromArgb(37, 99, 235), 60f))
            {
                g.FillRectangle(bg, 0, 0, 100, 100);
            }

            float y = 24;
            for (int k = 0; k < 3; k++)
            {
                FillRR(g, Color.FromArgb(240, 255, 255, 255), 22, y, 56, 15, 6);
                FillE(g, k == 0 ? Color.FromArgb(34, 197, 94) : Color.FromArgb(37, 99, 235), 29, y + 5, 5, 5);
                FillR(g, Color.FromArgb(150, 100, 116, 139), 40, y + 6.5f, 30, 2);
                y += 20;
            }
        }
    }

    /// <summary>
    /// ابزارهای سیستم‌عامل: چشمک زدن آیکون در نوار وظیفه
    /// </summary>
    public static class NativeHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        private const uint FLASHW_STOP = 0;
        private const uint FLASHW_ALL = 3;
        private const uint FLASHW_TIMERNOFG = 12;

        // چشمک زدن دکمه برنامه در نوار وظیفه تا وقتی کاربر پنجره را فعال کند
        public static void FlashTaskbar(Form form)
        {
            try
            {
                FLASHWINFO info = new FLASHWINFO();
                info.cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO));
                info.hwnd = form.Handle;
                info.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
                info.uCount = uint.MaxValue;
                info.dwTimeout = 0;
                FlashWindowEx(ref info);
            }
            catch { }
        }

        // توقف چشمک زدن
        public static void StopFlash(Form form)
        {
            try
            {
                FLASHWINFO info = new FLASHWINFO();
                info.cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO));
                info.hwnd = form.Handle;
                info.dwFlags = FLASHW_STOP;
                info.uCount = 0;
                info.dwTimeout = 0;
                FlashWindowEx(ref info);
            }
            catch { }
        }
    }

    /// <summary>
    /// ظاهر منوی راست‌کلیک هماهنگ با تم
    /// </summary>
    public class ThemedMenuRenderer : ToolStripProfessionalRenderer
    {
        private class Colors : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground { get { return Theme.Surface; } }
            public override Color ImageMarginGradientBegin { get { return Theme.Surface; } }
            public override Color ImageMarginGradientMiddle { get { return Theme.Surface; } }
            public override Color ImageMarginGradientEnd { get { return Theme.Surface; } }
            public override Color MenuItemSelected { get { return Theme.Hover; } }
            public override Color MenuItemBorder { get { return Theme.Hover; } }
            public override Color MenuBorder { get { return Theme.Border; } }
            public override Color SeparatorDark { get { return Theme.Border; } }
            public override Color SeparatorLight { get { return Theme.Border; } }
            public override Color MenuItemSelectedGradientBegin { get { return Theme.Hover; } }
            public override Color MenuItemSelectedGradientEnd { get { return Theme.Hover; } }
        }

        public ThemedMenuRenderer() : base(new Colors())
        {
            RoundedEdges = false;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            string tag = e.Item.Tag as string;
            e.TextColor = (tag == "danger") ? Theme.Danger : Theme.Text;
            base.OnRenderItemText(e);
        }
    }

    // =====================================================================================
    // کنترل‌های پایه
    // =====================================================================================

    public enum SurfaceRole { Bg, Surface, Surface2 }

    /// <summary>
    /// پنل ساده که رنگش خودکار با تم عوض می‌شود
    /// </summary>
    public class SurfacePanel : Panel, ISurface
    {
        private SurfaceRole role;

        // سازنده بدون پارامتر برای دیزاینر (طراح فرم)
        public SurfacePanel() : this(SurfaceRole.Bg) { }

        public SurfacePanel(SurfaceRole role)
        {
            this.role = role;
            DoubleBuffered = true;
            Theme.Changed += OnThemeChanged;
            BackColor = SurfaceColor;
        }

        // نقش رنگ پنل (پس‌زمینه / سطح / سطح دوم)
        [DefaultValue(SurfaceRole.Bg)]
        public SurfaceRole Role
        {
            get { return role; }
            set { role = value; BackColor = SurfaceColor; Invalidate(); }
        }

        // رنگ پس‌زمینه از تم گرفته می‌شود، پس در دیزاینر ذخیره نمی‌شود
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        public Color SurfaceColor
        {
            get
            {
                if (role == SurfaceRole.Surface) return Theme.Surface;
                if (role == SurfaceRole.Surface2) return Theme.Surface2;
                return Theme.Bg;
            }
        }

        private void OnThemeChanged(object s, EventArgs e)
        {
            BackColor = SurfaceColor;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Theme.Changed -= OnThemeChanged;
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// کارت با گوشه‌های گرد، سایه ملایم و حاشیه
    /// </summary>
    public class RoundedPanel : Panel, ISurface
    {
        public int Radius = 18;

        public RoundedPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Theme.Changed += OnThemeChanged;
            BackColor = Theme.Surface;
        }

        public Color SurfaceColor { get { return Theme.Surface; } }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        private void OnThemeChanged(object s, EventArgs e)
        {
            BackColor = Theme.Surface;
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(UiUtil.ParentBack(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2.5f);
            if (!Theme.IsDark)
            {
                // سایه ملایم زیر کارت
                UiUtil.FillRound(g, Color.FromArgb(14, 40, 50, 120), new RectangleF(r.X + 1, r.Y + 2.5f, r.Width, r.Height), Radius);
            }
            using (GraphicsPath p = UiUtil.RoundRect(r, Radius))
            using (Brush b = new SolidBrush(Theme.Surface))
            using (Pen pen = new Pen(Theme.Border, 1f))
            {
                g.FillPath(b, p);
                g.DrawPath(pen, p);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Theme.Changed -= OnThemeChanged;
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// پنل هدر با پس‌زمینه گرادیان و چند دایره تزئینی
    /// </summary>
    public class GradientPanel : Panel
    {
        public GradientPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Theme.Changed += OnThemeChanged;
        }

        private void OnThemeChanged(object s, EventArgs e) { Invalidate(true); }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (Width <= 0 || Height <= 0) return;

            Color a = Theme.IsDark ? Theme.Mix(Theme.Accent, Color.Black, 0.35f) : Theme.Accent;
            Color b = Theme.IsDark ? Theme.Mix(Theme.AccentAlt, Color.Black, 0.35f) : Theme.AccentAlt;
            using (LinearGradientBrush br = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), a, b, 0f))
            {
                g.FillRectangle(br, 0, 0, Width, Height);
            }

            // دایره‌های تزئینی نیمه‌شفاف
            using (Brush c1 = new SolidBrush(Color.FromArgb(22, 255, 255, 255)))
            {
                g.FillEllipse(c1, Width * 0.55f, -Height * 0.9f, Height * 2f, Height * 2f);
                g.FillEllipse(c1, Width * 0.12f, Height * 0.2f, Height * 1.4f, Height * 1.4f);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Theme.Changed -= OnThemeChanged;
            base.Dispose(disposing);
        }
    }

    public enum LabelRole { Text, Muted, Danger, OnAccent, Accent, Success }

    /// <summary>
    /// لیبل فارسی (راست‌به‌چپ) که رنگش خودکار با تم عوض می‌شود
    /// </summary>
    public class ThemedLabel : Label
    {
        private LabelRole role;

        // سازنده بدون پارامتر برای دیزاینر (طراح فرم)
        public ThemedLabel() : this(LabelRole.Text) { }

        public ThemedLabel(LabelRole role)
        {
            this.role = role;
            AutoSize = false;
            BackColor = Color.Transparent;
            RightToLeft = RightToLeft.Yes;
            TextAlign = ContentAlignment.MiddleRight;
            Font = Theme.Font(10);
            UseMnemonic = false;
            Theme.Changed += OnThemeChanged;
            ForeColor = RoleColor();
        }

        // نقش رنگ متن (متن اصلی، کم‌رنگ، قرمز، سفید روی هدر ...)؛ رنگ نهایی از تم گرفته می‌شود
        [DefaultValue(LabelRole.Text)]
        public LabelRole Role
        {
            get { return role; }
            set { role = value; ForeColor = RoleColor(); }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        private Color RoleColor()
        {
            switch (role)
            {
                case LabelRole.Muted: return Theme.Muted;
                case LabelRole.Danger: return Theme.Danger;
                case LabelRole.OnAccent: return Color.White;
                case LabelRole.Accent: return Theme.Accent;
                case LabelRole.Success: return Theme.Success;
                default: return Theme.Text;
            }
        }

        private void OnThemeChanged(object s, EventArgs e)
        {
            ForeColor = RoleColor();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Theme.Changed -= OnThemeChanged;
            base.Dispose(disposing);
        }
    }

    public enum BtnKind { Primary, Secondary, Danger, Success, Ghost }

    /// <summary>
    /// دکمه مدرن با گرادیان، انیمیشن هاور، آیکون و حالت «در حال انجام» (اسپینر)
    /// </summary>
    public class ModernButton : Control
    {
        private BtnKind kind = BtnKind.Primary;
        private int radius = 12;

        // نوع دکمه (اصلی، ثانویه، خطر، موفقیت، شفاف)
        [DefaultValue(BtnKind.Primary)]
        public BtnKind Kind
        {
            get { return kind; }
            set { kind = value; Invalidate(); }
        }

        // شعاع گوشه‌ها
        [DefaultValue(12)]
        public int Radius
        {
            get { return radius; }
            set { radius = value; Invalidate(); }
        }

        // تابع رسم آیکون (در کد تنظیم می‌شود)
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Action<Graphics, Rectangle, Color> IconPainter { get; set; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        private float hover, hoverTarget, spin;
        private bool down, busy, transparent;
        private readonly WinTimer timer = new WinTimer();

        public ModernButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, false);
            Cursor = Cursors.Hand;
            Font = Theme.Font(10, FontStyle.Bold);
            Size = new Size(120, 40);
            timer.Interval = 16;
            timer.Tick += OnTick;
            Theme.Changed += OnThemeChanged;
        }

        // پس‌زمینه شفاف (برای دکمه‌هایی که روی هدر گرادیان قرار می‌گیرند)
        [DefaultValue(false)]
        public bool TransparentBg
        {
            get { return transparent; }
            set { transparent = value; BackColor = value ? Color.Transparent : SystemColors.Control; Invalidate(); }
        }

        // حالت در حال انجام: اسپینر چرخان نشان می‌دهد
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Busy
        {
            get { return busy; }
            set { busy = value; StartTimer(); Invalidate(); }
        }

        private void StartTimer()
        {
            if (!timer.Enabled) timer.Start();
        }

        private void OnTick(object s, EventArgs e)
        {
            bool again = false;
            if (Math.Abs(hover - hoverTarget) > 0.02f)
            {
                hover += (hoverTarget - hover) * 0.3f;
                again = true;
            }
            else
            {
                hover = hoverTarget;
            }

            if (busy)
            {
                spin = (spin + 12f) % 360f;
                again = true;
            }

            Invalidate();
            if (!again) timer.Stop();
        }

        private void OnThemeChanged(object s, EventArgs e) { Invalidate(); }

        protected override void OnMouseEnter(EventArgs e) { hoverTarget = 1f; StartTimer(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hoverTarget = 0f; down = false; StartTimer(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { down = true; Invalidate(); } base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (!transparent) g.Clear(UiUtil.ParentBack(this));
            if (Width < 4 || Height < 4) return;

            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
            if (down && Enabled) r.Inflate(-1.5f, -1.5f);

            Color fore = Color.White;
            using (GraphicsPath path = UiUtil.RoundRect(r, Radius))
            {
                if (!Enabled)
                {
                    using (Brush b = new SolidBrush(Theme.Surface2)) g.FillPath(b, path);
                    fore = Theme.Muted;
                }
                else if (Kind == BtnKind.Primary)
                {
                    Color c1 = Theme.Mix(Theme.Accent, Color.White, hover * 0.2f);
                    Color c2 = Theme.Mix(Theme.AccentAlt, Color.White, hover * 0.2f);
                    using (LinearGradientBrush b = new LinearGradientBrush(r, c1, c2, 0f)) g.FillPath(b, path);
                }
                else if (Kind == BtnKind.Danger || Kind == BtnKind.Success)
                {
                    Color baseColor = Kind == BtnKind.Danger ? Theme.Danger : Theme.Success;
                    using (Brush b = new SolidBrush(Theme.Mix(baseColor, Color.White, hover * 0.18f))) g.FillPath(b, path);
                }
                else if (Kind == BtnKind.Secondary)
                {
                    using (Brush b = new SolidBrush(Theme.Mix(Theme.Surface2, Theme.Hover, hover))) g.FillPath(b, path);
                    using (Pen p = new Pen(Theme.Mix(Theme.Border, Theme.Accent, hover * 0.6f), 1f)) g.DrawPath(p, path);
                    fore = Theme.Text;
                }
                else
                {
                    // Ghost: فقط هنگام هاور یک پس‌زمینه کمرنگ دارد
                    if (hover > 0.02f)
                    {
                        using (Brush b = new SolidBrush(Color.FromArgb((int)(55 * hover), transparent ? Color.White : Theme.Accent))) g.FillPath(b, path);
                    }
                    fore = transparent ? Color.White : Theme.Text;
                }
            }

            // آیکون و متن
            bool rtl = Fa.IsRtl(Text);
            int iconSize = 18, gap = 8;
            Size ts = UiUtil.MeasureLine(Text, Font);
            bool hasIcon = busy || IconPainter != null;
            int total = ts.Width + (hasIcon ? iconSize + (ts.Width > 0 ? gap : 0) : 0);
            int x0 = (Width - total) / 2;
            Rectangle iconRect, textRect;
            if (rtl)
            {
                textRect = new Rectangle(x0, 0, ts.Width, Height);
                iconRect = new Rectangle(x0 + ts.Width + (ts.Width > 0 ? gap : 0), (Height - iconSize) / 2, iconSize, iconSize);
            }
            else
            {
                iconRect = new Rectangle(x0, (Height - iconSize) / 2, iconSize, iconSize);
                textRect = new Rectangle(x0 + (hasIcon ? iconSize + gap : 0), 0, ts.Width, Height);
            }

            if (busy)
            {
                using (Pen sp = new Pen(fore, 2.4f))
                {
                    sp.StartCap = LineCap.Round;
                    sp.EndCap = LineCap.Round;
                    g.DrawArc(sp, iconRect.X + 1, iconRect.Y + 1, iconRect.Width - 2, iconRect.Height - 2, spin, 270f);
                }
            }
            else if (IconPainter != null)
            {
                IconPainter(g, iconRect, fore);
            }

            if (ts.Width > 0)
            {
                TextFormatFlags f = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine
                    | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                if (rtl) f |= TextFormatFlags.RightToLeft;
                TextRenderer.DrawText(g, Text, Font, textRect, fore, f);
            }
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
    /// کادر ورودی متن با گوشه گرد، حاشیه رنگی هنگام فوکوس و متن راهنما (Placeholder)
    /// داخلش یک TextBox معمولی دارد که از طریق Inner در دسترس است
    /// </summary>
    public class ModernTextBox : Control, ISurface
    {
        public readonly TextBox Inner;
        private readonly Label hint;
        private bool focused;
        private bool rtl = true;
        private string hintText = "";
        public int Radius = 12;

        public ModernTextBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            Inner = new TextBox();
            Inner.BorderStyle = BorderStyle.None;
            Inner.Font = Theme.Font(11);
            Inner.RightToLeft = RightToLeft.Yes;       // پیش‌فرض راست‌به‌چپ (فارسی)
            Inner.Enter += delegate { focused = true; Invalidate(); };
            Inner.Leave += delegate { focused = false; Invalidate(); };
            Inner.TextChanged += delegate { UpdateHint(); OnTextChanged(EventArgs.Empty); };

            hint = new Label();
            hint.AutoSize = false;
            hint.UseMnemonic = false;
            hint.Cursor = Cursors.IBeam;
            hint.Font = Theme.Font(10.5f);
            hint.Click += delegate { Inner.Focus(); };

            Controls.Add(Inner);
            Controls.Add(hint);
            hint.BringToFront();

            Size = new Size(240, 44);
            Theme.Changed += OnThemeChanged;
            ApplyTheme();
            LayoutInner();
        }

        public Color SurfaceColor { get { return Theme.InputBg; } }

        [DefaultValue("")]
        public override string Text
        {
            get { return Inner.Text; }
            set { Inner.Text = value; }
        }

        // متن راهنما
        [DefaultValue("")]
        public string Hint
        {
            get { return hintText; }
            set { hintText = value; hint.Text = value; UpdateHint(); }
        }

        // چندخطی بودن
        [DefaultValue(false)]
        public bool Multiline
        {
            get { return Inner.Multiline; }
            set { Inner.Multiline = value; Inner.WordWrap = true; LayoutInner(); }
        }

        // راست‌به‌چپ بودن (برای متن فارسی)؛ برای آی‌پی و پورت false شود
        [DefaultValue(true)]
        public bool Rtl
        {
            get { return rtl; }
            set { rtl = value; Inner.RightToLeft = value ? RightToLeft.Yes : RightToLeft.No; LayoutInner(); }
        }

        public void FocusInner() { Inner.Focus(); }

        private void UpdateHint()
        {
            hint.Visible = Inner.Text.Length == 0 && hintText.Length > 0;
        }

        private void LayoutInner()
        {
            if (Inner == null || hint == null) return;
            int padX = 14;
            if (Inner.Multiline)
            {
                Inner.SetBounds(padX, 10, Math.Max(10, Width - 2 * padX), Math.Max(10, Height - 20));
                hint.SetBounds(padX, 10, Math.Max(10, Width - 2 * padX), 24);
                hint.TextAlign = rtl ? ContentAlignment.TopRight : ContentAlignment.TopLeft;
            }
            else
            {
                Inner.Width = Math.Max(10, Width - 2 * padX);
                Inner.Left = padX;
                Inner.Top = Math.Max(0, (Height - Inner.Height) / 2);
                hint.SetBounds(padX, Inner.Top, Inner.Width, Inner.Height);
                hint.TextAlign = rtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            }
        }

        private void ApplyTheme()
        {
            Inner.BackColor = Theme.InputBg;
            Inner.ForeColor = Theme.Text;
            hint.BackColor = Theme.InputBg;
            hint.ForeColor = Theme.Muted;
        }

        private void OnThemeChanged(object s, EventArgs e) { ApplyTheme(); Invalidate(); }

        protected override void OnResize(EventArgs e) { base.OnResize(e); LayoutInner(); }
        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); LayoutInner(); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Inner.Enabled = Enabled; }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Inner.Focus(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(UiUtil.ParentBack(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF r = new RectangleF(0.75f, 0.75f, Width - 2f, Height - 2f);
            using (GraphicsPath p = UiUtil.RoundRect(r, Radius))
            using (Brush b = new SolidBrush(Theme.InputBg))
            using (Pen pen = new Pen(focused ? Theme.Accent : Theme.Border, focused ? 1.8f : 1f))
            {
                g.FillPath(b, p);
                g.DrawPath(pen, p);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Theme.Changed -= OnThemeChanged;
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// سوئیچ متحرک خورشید / ماه برای تغییر تم
    /// </summary>
    public class ThemeToggle : Control
    {
        private float t;
        private readonly WinTimer timer = new WinTimer();
        private bool transparent;

        public ThemeToggle()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, false);
            Size = new Size(60, 30);
            Cursor = Cursors.Hand;
            t = Theme.IsDark ? 1f : 0f;
            timer.Interval = 16;
            timer.Tick += OnTick;
            Theme.Changed += OnThemeChanged;
        }

        [DefaultValue(false)]
        public bool TransparentBg
        {
            get { return transparent; }
            set { transparent = value; BackColor = value ? Color.Transparent : SystemColors.Control; Invalidate(); }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        private void OnThemeChanged(object s, EventArgs e) { if (!timer.Enabled) timer.Start(); }

        private void OnTick(object s, EventArgs e)
        {
            float target = Theme.IsDark ? 1f : 0f;
            if (Math.Abs(t - target) < 0.03f) { t = target; timer.Stop(); }
            else t += (target - t) * 0.25f;
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Theme.Toggle();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (!transparent) g.Clear(UiUtil.ParentBack(this));

            RectangleF track = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
            Color day1 = Color.FromArgb(251, 191, 36), day2 = Color.FromArgb(249, 115, 22);
            Color night1 = Color.FromArgb(67, 56, 202), night2 = Color.FromArgb(30, 27, 75);
            using (GraphicsPath p = UiUtil.RoundRect(track, track.Height / 2f))
            using (LinearGradientBrush b = new LinearGradientBrush(track, Theme.Mix(day1, night1, t), Theme.Mix(day2, night2, t), 0f))
            {
                g.FillPath(b, p);
            }

            float d = Height - 7f;
            float x = 3.5f + t * (Width - d - 7f);
            RectangleF knob = new RectangleF(x, 3.5f, d, d);
            using (Brush kb = new SolidBrush(Color.White)) g.FillEllipse(kb, knob);

            float cx = knob.X + d / 2f, cy = knob.Y + d / 2f;
            if (t < 0.5f)
            {
                // خورشید
                using (Brush sb = new SolidBrush(Color.FromArgb(245, 158, 11))) g.FillEllipse(sb, cx - 4.5f, cy - 4.5f, 9, 9);
                using (Pen rp = new Pen(Color.FromArgb(245, 158, 11), 1.5f))
                {
                    for (int i = 0; i < 8; i++)
                    {
                        double a = i * Math.PI / 4;
                        g.DrawLine(rp, cx + (float)Math.Cos(a) * 6.5f, cy + (float)Math.Sin(a) * 6.5f, cx + (float)Math.Cos(a) * 8.3f, cy + (float)Math.Sin(a) * 8.3f);
                    }
                }
            }
            else
            {
                // ماه
                using (Brush mb = new SolidBrush(Color.FromArgb(67, 56, 202))) g.FillEllipse(mb, cx - 6, cy - 6, 12, 12);
                using (Brush cut = new SolidBrush(Color.White)) g.FillEllipse(cut, cx - 1.5f, cy - 7.5f, 10, 10);
            }
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
    /// نشانگر وضعیت (قرص کوچک با نقطه رنگی و متن)؛ در حالت Pulsing نقطه می‌تپد
    /// </summary>
    public class StatusPill : Control
    {
        private string text = "";
        private Color dot = Color.Gray;
        private bool pulsing;
        private readonly WinTimer timer = new WinTimer();

        public StatusPill()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(130, 32);
            timer.Interval = 40;
            timer.Tick += delegate { Invalidate(); };
        }

        // تعیین وضعیت: متن، رنگ نقطه و تپش
        public void SetState(string newText, Color dotColor, bool pulse)
        {
            text = newText;
            dot = dotColor;
            pulsing = pulse;
            if (pulse) timer.Start(); else timer.Stop();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
            UiUtil.FillRound(g, Color.FromArgb(48, 255, 255, 255), r, r.Height / 2f);

            // نقطه رنگی (سمت راست چون متن فارسی است)
            float cx = Width - 18f, cy = Height / 2f;
            if (pulsing)
            {
                double ph = (Environment.TickCount % 1400) / 1400.0;
                float rad = 5f + (float)(ph * 8);
                int alpha = (int)(110 * (1 - ph));
                using (Brush pb = new SolidBrush(Color.FromArgb(alpha, dot))) g.FillEllipse(pb, cx - rad, cy - rad, rad * 2, rad * 2);
            }
            using (Brush db = new SolidBrush(dot)) g.FillEllipse(db, cx - 5f, cy - 5f, 10, 10);

            TextRenderer.DrawText(g, text, Theme.Font(9.5f, FontStyle.Bold), new Rectangle(8, 0, Width - 34, Height), Color.White,
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.RightToLeft);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { timer.Stop(); timer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// کارت آماری کوچک (عنوان + مقدار) روی هدر
    /// </summary>
    public class StatChip : Control
    {
        private string title = "";
        private string value = "";

        public StatChip()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(130, 48);
        }

        public string Title { get { return title; } set { title = value; Invalidate(); } }
        public string Value { get { return value; } set { this.value = value; Invalidate(); } }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            UiUtil.FillRound(g, Color.FromArgb(40, 255, 255, 255), new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f), 12);
            TextRenderer.DrawText(g, value, Theme.Font(13, FontStyle.Bold), new Rectangle(0, 4, Width, 24), Color.White,
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);
            TextRenderer.DrawText(g, title, Theme.Font(8.5f), new Rectangle(0, 27, Width, 16), Color.FromArgb(215, 255, 255, 255),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);
        }
    }

    /// <summary>
    /// انتخاب‌گر آواتار: ۳ آواتار دخترانه و ۳ آواتار پسرانه با انیمیشن بزرگ شدن هنگام هاور و حلقه انتخاب
    /// </summary>
    public class AvatarPicker : Control, ISurface
    {
        public int Selected = 0;
        public event EventHandler SelectedChanged;

        private readonly float[] scale = new float[ChatConst.AvatarCount];
        private int hover = -1;
        private readonly WinTimer timer = new WinTimer();
        private const int Size1 = 54;
        private const int Gap = 14;

        public AvatarPicker()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Default;
            Size = new Size(440, 108);
            for (int i = 0; i < scale.Length; i++) scale[i] = 1f;
            timer.Interval = 16;
            timer.Tick += OnTick;
            Theme.Changed += OnThemeChanged;
        }

        public Color SurfaceColor { get { return UiUtil.ParentBack(this); } }

        private void OnThemeChanged(object s, EventArgs e) { Invalidate(); }

        // مستطیل هر آواتار: دخترها (۰ تا ۲) سمت راست، پسرها (۳ تا ۵) سمت چپ
        private Rectangle GetRect(int i)
        {
            int groupW = 3 * Size1 + 2 * Gap;
            int mid = 40;
            int startX = (Width - (groupW * 2 + mid)) / 2;
            int x;
            if (i < 3)
            {
                int gx = startX + groupW + mid;
                x = gx + (2 - i) * (Size1 + Gap);
            }
            else
            {
                x = startX + (5 - i) * (Size1 + Gap);
            }
            return new Rectangle(x, 30, Size1, Size1);
        }

        private int HitTest(Point p)
        {
            for (int i = 0; i < ChatConst.AvatarCount; i++)
            {
                if (GetRect(i).Contains(p)) return i;
            }
            return -1;
        }

        private void OnTick(object s, EventArgs e)
        {
            bool again = false;
            for (int i = 0; i < scale.Length; i++)
            {
                float target = (i == hover) ? 1.14f : (i == Selected ? 1.07f : 1f);
                if (Math.Abs(scale[i] - target) > 0.004f)
                {
                    scale[i] += (target - scale[i]) * 0.3f;
                    again = true;
                }
                else scale[i] = target;
            }
            Invalidate();
            if (!again) timer.Stop();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int h = HitTest(e.Location);
            if (h != hover)
            {
                hover = h;
                Cursor = h >= 0 ? Cursors.Hand : Cursors.Default;
                timer.Start();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hover = -1;
            timer.Start();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            int h = HitTest(e.Location);
            if (h >= 0 && h != Selected)
            {
                Selected = h;
                timer.Start();
                if (SelectedChanged != null) SelectedChanged(this, EventArgs.Empty);
            }
            base.OnMouseDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(UiUtil.ParentBack(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // عنوان گروه‌ها
            Rectangle girls = new Rectangle(GetRect(2).X, 4, GetRect(0).Right - GetRect(2).X, 20);
            Rectangle boys = new Rectangle(GetRect(5).X, 4, GetRect(3).Right - GetRect(5).X, 20);
            TextFormatFlags f = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft;
            TextRenderer.DrawText(g, "دخترانه", Theme.Font(9, FontStyle.Bold), girls, Theme.Muted, f);
            TextRenderer.DrawText(g, "پسرانه", Theme.Font(9, FontStyle.Bold), boys, Theme.Muted, f);

            for (int i = 0; i < ChatConst.AvatarCount; i++)
            {
                Rectangle rc = GetRect(i);
                float s = scale[i];
                float w = rc.Width * s;
                RectangleF ar = new RectangleF(rc.X + (rc.Width - w) / 2f, rc.Y + (rc.Height - w) / 2f, w, w);

                // حلقه انتخاب
                if (i == Selected)
                {
                    RectangleF ring = ar;
                    ring.Inflate(5, 5);
                    using (Pen rp = new Pen(Theme.Accent, 3f)) g.DrawEllipse(rp, ring);
                }
                else if (i == hover)
                {
                    RectangleF ring = ar;
                    ring.Inflate(4, 4);
                    using (Pen rp = new Pen(Theme.Border, 2f)) g.DrawEllipse(rp, ring);
                }

                AvatarPainter.Draw(g, i, ar);

                // نشان تیک روی آواتار انتخاب شده
                if (i == Selected)
                {
                    RectangleF badge = new RectangleF(ar.Right - 15, ar.Bottom - 15, 18, 18);
                    using (Brush bb = new SolidBrush(Theme.Accent)) g.FillEllipse(bb, badge);
                    using (Pen wp = new Pen(Theme.Surface, 1.6f)) g.DrawEllipse(wp, badge);
                    Icons.Check(g, Rectangle.Round(badge), Color.White);
                }
            }
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
