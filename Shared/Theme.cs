using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ChatpApp.Shared
{
    public enum ThemeMode { Light, Dark }

    /// <summary>
    /// سیستم تم برنامه (روشن / تیره)
    /// همه کنترل‌های سفارشی رنگ‌هایشان را از اینجا می‌گیرند و با رویداد Changed خودشان را دوباره می‌کشند
    /// انتخاب کاربر در یک فایل کوچک ذخیره می‌شود تا دفعه بعد همان تم باز شود
    /// </summary>
    public static class Theme
    {
        public static ThemeMode Mode { get; private set; }

        // رویدادی که با عوض شدن تم برای همه کنترل‌ها اجرا می‌شود
        public static event EventHandler Changed;

        public static bool IsDark { get { return Mode == ThemeMode.Dark; } }

        // ---------- رنگ‌های اصلی ----------
        public static Color Bg;                // پس‌زمینه فرم
        public static Color Surface;           // کارت‌ها و پنل‌ها
        public static Color Surface2;          // سطح دوم (هاور، فیلد)
        public static Color InputBg;           // پس‌زمینه کادر ورودی
        public static Color Border;            // حاشیه‌ها
        public static Color Text;              // متن اصلی
        public static Color Muted;             // متن کم‌رنگ
        public static Color Accent;            // رنگ اصلی (ایندیگو)
        public static Color AccentAlt;         // رنگ دوم گرادیان (بنفش)
        public static Color Danger;            // قرمز
        public static Color Success;           // سبز
        public static Color Warning;           // زرد
        public static Color Hover;             // رنگ هاور

        // ---------- رنگ‌های چت ----------
        public static Color ChatBgTop;
        public static Color ChatBgBottom;
        public static Color BubbleOther;       // پیام دیگران
        public static Color BubbleServerA;     // پیام سرور (شروع گرادیان)
        public static Color BubbleServerB;     // پیام سرور (پایان گرادیان)
        public static Color ServerText;        // متن پیام سرور
        public static Color SystemChip;        // پیام‌های سیستمی
        public static Color SystemText;

        static Theme()
        {
            Mode = Load();
            ApplyPalette();
        }

        // مسیر فایل ذخیره تم
        private static string SettingsPath
        {
            get
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChatpApp");
                return Path.Combine(dir, "theme.txt");
            }
        }

        private static ThemeMode Load()
        {
            try
            {
                if (File.Exists(SettingsPath) && File.ReadAllText(SettingsPath).Trim() == "dark")
                {
                    return ThemeMode.Dark;
                }
            }
            catch { }
            return ThemeMode.Light;
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
                File.WriteAllText(SettingsPath, Mode == ThemeMode.Dark ? "dark" : "light");
            }
            catch { }
        }

        // تعیین تم
        public static void Set(ThemeMode mode)
        {
            if (mode == Mode) return;
            Mode = mode;
            ApplyPalette();
            Save();
            if (Changed != null) Changed(null, EventArgs.Empty);
        }

        // جابه‌جایی بین روشن و تیره
        public static void Toggle()
        {
            Set(Mode == ThemeMode.Dark ? ThemeMode.Light : ThemeMode.Dark);
        }

        // مقداردهی رنگ‌ها بر اساس تم فعلی
        private static void ApplyPalette()
        {
            Danger = Color.FromArgb(239, 68, 68);
            Success = Color.FromArgb(34, 197, 94);
            Warning = Color.FromArgb(245, 158, 11);

            if (Mode == ThemeMode.Dark)
            {
                Bg = Color.FromArgb(14, 16, 26);
                Surface = Color.FromArgb(24, 27, 41);
                Surface2 = Color.FromArgb(34, 38, 58);
                InputBg = Color.FromArgb(19, 22, 34);
                Border = Color.FromArgb(46, 51, 74);
                Text = Color.FromArgb(232, 235, 247);
                Muted = Color.FromArgb(139, 147, 172);
                Accent = Color.FromArgb(99, 102, 241);
                AccentAlt = Color.FromArgb(168, 85, 247);
                Hover = Color.FromArgb(40, 45, 68);

                ChatBgTop = Color.FromArgb(17, 19, 31);
                ChatBgBottom = Color.FromArgb(12, 14, 23);
                BubbleOther = Color.FromArgb(38, 43, 64);
                BubbleServerA = Color.FromArgb(120, 83, 18);
                BubbleServerB = Color.FromArgb(146, 64, 14);
                ServerText = Color.FromArgb(255, 243, 214);
                SystemChip = Color.FromArgb(34, 38, 58);
                SystemText = Color.FromArgb(160, 168, 192);
            }
            else
            {
                Bg = Color.FromArgb(240, 242, 249);
                Surface = Color.FromArgb(255, 255, 255);
                Surface2 = Color.FromArgb(238, 241, 250);
                InputBg = Color.FromArgb(246, 248, 253);
                Border = Color.FromArgb(222, 227, 240);
                Text = Color.FromArgb(24, 28, 48);
                Muted = Color.FromArgb(107, 116, 142);
                Accent = Color.FromArgb(79, 70, 229);
                AccentAlt = Color.FromArgb(147, 51, 234);
                Hover = Color.FromArgb(232, 236, 249);

                ChatBgTop = Color.FromArgb(244, 246, 252);
                ChatBgBottom = Color.FromArgb(233, 237, 249);
                BubbleOther = Color.FromArgb(255, 255, 255);
                BubbleServerA = Color.FromArgb(254, 240, 190);
                BubbleServerB = Color.FromArgb(253, 224, 171);
                ServerText = Color.FromArgb(87, 53, 8);
                SystemChip = Color.FromArgb(222, 227, 244);
                SystemText = Color.FromArgb(96, 106, 135);
            }
        }

        // ---------- فونت‌ها ----------
        // فونت Segoe UI حروف فارسی را پشتیبانی می‌کند. فونت‌ها کش می‌شوند و نباید Dispose شوند
        private static readonly Dictionary<string, Font> fonts = new Dictionary<string, Font>();

        public static Font Font(float size)
        {
            return Font(size, FontStyle.Regular);
        }

        public static Font Font(float size, FontStyle style)
        {
            string key = size.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" + (int)style;
            lock (fonts)
            {
                Font f;
                if (!fonts.TryGetValue(key, out f))
                {
                    f = new Font("Segoe UI", size, style, GraphicsUnit.Point);
                    fonts[key] = f;
                }
                return f;
            }
        }

        // فونت ایموجی
        public static Font EmojiFont(float size)
        {
            string key = "emoji" + size.ToString(System.Globalization.CultureInfo.InvariantCulture);
            lock (fonts)
            {
                Font f;
                if (!fonts.TryGetValue(key, out f))
                {
                    f = new Font("Segoe UI Emoji", size, FontStyle.Regular, GraphicsUnit.Point);
                    fonts[key] = f;
                }
                return f;
            }
        }

        // ---------- توابع کمکی رنگ ----------
        public static Color WithAlpha(Color c, int alpha)
        {
            return Color.FromArgb(Math.Max(0, Math.Min(255, alpha)), c);
        }

        // ترکیب دو رنگ (t بین ۰ تا ۱)
        public static Color Mix(Color a, Color b, float t)
        {
            if (t < 0) t = 0;
            if (t > 1) t = 1;
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        // رنگ تینت هر ایموجی (چون ایموجی‌ها تک‌رنگ کشیده می‌شوند، به هرکدام یک رنگ مناسب می‌دهیم)
        public static Color EmojiTint(string emoji)
        {
            if (emoji == "\u2764") return Color.FromArgb(239, 68, 68);
            if (emoji == "\U0001F602") return Color.FromArgb(245, 158, 11);
            if (emoji == "\U0001F44D") return Color.FromArgb(59, 130, 246);
            if (emoji == "\U0001F525") return Color.FromArgb(249, 115, 22);
            return Accent;
        }
    }
}
