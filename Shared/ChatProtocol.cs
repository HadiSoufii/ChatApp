using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;

// این فایل بین پروژه سرور و کلاینت مشترک است (در csproj هر دو پروژه به صورت Link اضافه شده)
// شامل: پروتکل شبکه، مدل پیام، و توابع کمکی فارسی
namespace ChatpApp.Shared
{
    /// <summary>
    /// ثابت‌های مشترک برنامه
    /// </summary>
    public static class ChatConst
    {
        // نام نمایشی سرور در چت
        public const string ServerName = "سرور";

        // بیشترین طول یک پیام
        public const int MaxTextLength = 1000;

        // تعداد آواتارهای قابل انتخاب (۳ دخترانه + ۳ پسرانه)
        public const int AvatarCount = 6;

        // ایموجی‌های واکنش به پیام: قلب، خنده، لایک، آتش
        public static readonly string[] Reactions = { "\u2764", "\U0001F602", "\U0001F44D", "\U0001F525" };
    }

    /// <summary>
    /// یک بسته (Packet) شبکه: نوع بسته + چند آرگومان متنی
    /// شکل ارسال روی شبکه:  TYPE|arg1|arg2|...  و در آخر یک خط جدید (\n)
    /// آرگومان‌ها قبل از ارسال escape می‌شوند، پس متن کاربر (حتی فارسی و چند خطی) هیچ‌وقت ساختار بسته را خراب نمی‌کند
    /// </summary>
    public class Packet
    {
        public string Type;
        public string[] Args;

        public Packet(string type, params string[] args)
        {
            Type = type;
            Args = args ?? new string[0];
        }

        // گرفتن آرگومان شماره i (اگر وجود نداشت رشته خالی)
        public string Arg(int i)
        {
            return (i >= 0 && i < Args.Length) ? Args[i] : "";
        }

        // گرفتن آرگومان به صورت عدد long
        public long ArgLong(int i)
        {
            long v;
            return long.TryParse(Arg(i), NumberStyles.Integer, CultureInfo.InvariantCulture, out v) ? v : 0;
        }

        // گرفتن آرگومان به صورت عدد int
        public int ArgInt(int i)
        {
            return (int)ArgLong(i);
        }

        // تبدیل بسته به یک خط متن برای ارسال
        public string Serialize()
        {
            StringBuilder sb = new StringBuilder(Type);
            foreach (string a in Args)
            {
                sb.Append('|');
                sb.Append(Escape(a));
            }
            return sb.ToString();
        }

        // ساختن بسته از یک خط دریافتی
        public static Packet Parse(string line)
        {
            string[] parts = line.Split('|');
            string[] args = new string[parts.Length - 1];
            for (int i = 1; i < parts.Length; i++)
            {
                args[i - 1] = Unescape(parts[i]);
            }
            return new Packet(parts[0], args);
        }

        private const string HexChars = "0123456789ABCDEF";

        // escape کردن: کاراکترهای % و | و کاراکترهای کنترلی (مثل خط جدید) به شکل %XX نوشته می‌شوند
        private static string Escape(string s)
        {
            byte[] src = Encoding.UTF8.GetBytes(s ?? "");
            MemoryStream ms = new MemoryStream(src.Length + 16);
            foreach (byte b in src)
            {
                if (b == (byte)'%' || b == (byte)'|' || b < 32 || b == 127)
                {
                    ms.WriteByte((byte)'%');
                    ms.WriteByte((byte)HexChars[b >> 4]);
                    ms.WriteByte((byte)HexChars[b & 15]);
                }
                else
                {
                    ms.WriteByte(b);
                }
            }
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static int HexVal(byte c)
        {
            if (c >= (byte)'0' && c <= (byte)'9') return c - (byte)'0';
            if (c >= (byte)'A' && c <= (byte)'F') return c - (byte)'A' + 10;
            if (c >= (byte)'a' && c <= (byte)'f') return c - (byte)'a' + 10;
            return -1;
        }

        // برعکس Escape
        private static string Unescape(string s)
        {
            if (s.IndexOf('%') < 0)
            {
                return s;
            }

            byte[] src = Encoding.UTF8.GetBytes(s);
            MemoryStream ms = new MemoryStream(src.Length);
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == (byte)'%' && i + 2 < src.Length)
                {
                    int hi = HexVal(src[i + 1]);
                    int lo = HexVal(src[i + 2]);
                    if (hi >= 0 && lo >= 0)
                    {
                        ms.WriteByte((byte)(hi * 16 + lo));
                        i += 2;
                        continue;
                    }
                }
                ms.WriteByte(src[i]);
            }
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }

    /// <summary>
    /// یک اتصال TCP که بسته‌ها را به صورت «یک خط = یک بسته» می‌فرستد و می‌گیرد
    /// چون TCP جریان بایت است و مرز پیام‌ها را حفظ نمی‌کند (ممکنه چند پیام به هم بچسبند یا یک پیام نصفه برسد)
    /// این کلاس بایت‌ها را جمع می‌کند و فقط وقتی یک خط کامل رسید آن را به عنوان یک بسته تحویل می‌دهد
    /// </summary>
    public class ChatConnection
    {
        public readonly Socket Socket;

        // بافر دریافت
        private byte[] acc = new byte[8192];
        private int accLen = 0;

        // قفل ارسال: چند ترید ممکنه همزمان برای یک کاربر بسته بفرستند
        private readonly object sendLock = new object();

        private const int MaxLineBytes = 256 * 1024;

        public ChatConnection(Socket socket)
        {
            Socket = socket;
        }

        // آی پی و پورت طرف مقابل
        public string RemoteText
        {
            get
            {
                try { return Socket.RemoteEndPoint.ToString(); } catch { return "?"; }
            }
        }

        /// <summary>
        /// خواندن یک بسته کامل. اگر اتصال بسته شده باشد null برمی‌گرداند و در خطای شبکه Exception می‌دهد
        /// </summary>
        public Packet ReadPacket()
        {
            while (true)
            {
                // آیا یک خط کامل در بافر داریم؟
                int nl = Array.IndexOf(acc, (byte)10, 0, accLen);
                if (nl >= 0)
                {
                    string line = Encoding.UTF8.GetString(acc, 0, nl).TrimEnd('\r');

                    // خط مصرف شده را از بافر حذف می‌کنیم
                    Buffer.BlockCopy(acc, nl + 1, acc, 0, accLen - nl - 1);
                    accLen -= nl + 1;

                    if (line.Length == 0)
                    {
                        continue;
                    }
                    return Packet.Parse(line);
                }

                // بافر پر شده؟ بزرگ‌ترش می‌کنیم (تا یک حد مجاز)
                if (accLen >= acc.Length)
                {
                    if (acc.Length >= MaxLineBytes)
                    {
                        throw new IOException("line too long");
                    }
                    Array.Resize(ref acc, acc.Length * 2);
                }

                int n = Socket.Receive(acc, accLen, acc.Length - accLen, SocketFlags.None);
                if (n <= 0)
                {
                    return null;
                }
                accLen += n;
            }
        }

        /// <summary>
        /// ارسال یک بسته. اگر موفق بود true برمی‌گرداند
        /// </summary>
        public bool Send(Packet p)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(p.Serialize() + "\n");
                lock (sendLock)
                {
                    int sent = 0;
                    while (sent < bytes.Length)
                    {
                        sent += Socket.Send(bytes, sent, bytes.Length - sent, SocketFlags.None);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        // بستن اتصال
        public void Close()
        {
            try { Socket.Shutdown(SocketShutdown.Both); } catch { }
            try { Socket.Close(); } catch { }
        }
    }

    /// <summary>
    /// اطلاعات یک واکنش (ایموجی) روی پیام و کاربرانی که آن را زده‌اند
    /// </summary>
    public class ReactionInfo
    {
        public string Emoji = "";
        public List<string> Users = new List<string>();
    }

    /// <summary>
    /// اطلاعات یک پیام چت (هم در سرور، هم در کلاینت استفاده می‌شود)
    /// </summary>
    public class ChatMessageInfo
    {
        public long Id;                    // شناسه یکتای پیام (سرور تعیین می‌کند؛ پیام‌های در حال ارسال شناسه منفی موقت دارند)
        public string Sender = "";         // نام فرستنده
        public int Avatar;                 // شماره آواتار (۰ تا ۵) و ۱- برای سرور
        public DateTime Time;              // زمان ارسال (به وقت محلی)
        public string Text = "";           // متن پیام
        public long ReplyId;               // اگر پاسخ به پیام دیگری است شناسه آن
        public string ReplyName = "";      // نام فرستنده پیام اصلی
        public string ReplyText = "";      // خلاصه متن پیام اصلی
        public bool Edited;                // ویرایش شده؟
        public bool Deleted;               // حذف شده؟
        public string Kind = "user";       // user = کاربر ، server = پیام سرور
        public List<ReactionInfo> Reactions = new List<ReactionInfo>();

        // وضعیت تحویل (فقط در کلاینت): ۰ = ارسال شد (۱ تیک) ، ۱ = به سرور رسید (۲ تیک) ، ۲ = خوانده شد (۳ تیک)
        public int Status;
        public bool ReadReported;          // (کلاینت) آیا «خوانده شد» این پیام به سرور گزارش شده؟
        public HashSet<string> Readers = new HashSet<string>();   // (سرور) کسانی که پیام را خوانده‌اند

        // یک کپی مستقل از پیام (برای ارسال به ترید رابط کاربری)
        public ChatMessageInfo Clone()
        {
            ChatMessageInfo c = (ChatMessageInfo)MemberwiseClone();
            c.Reactions = new List<ReactionInfo>();
            foreach (ReactionInfo r in Reactions)
            {
                ReactionInfo rc = new ReactionInfo();
                rc.Emoji = r.Emoji;
                rc.Users = new List<string>(r.Users);
                c.Reactions.Add(rc);
            }
            c.Readers = new HashSet<string>(Readers);
            return c;
        }
    }

    /// <summary>
    /// اطلاعات یک پیام سنجاق شده
    /// </summary>
    public class PinInfo
    {
        public long Id;
        public string Sender = "";
        public string Text = "";
    }

    /// <summary>
    /// توابع کمکی پروتکل: تبدیل پیام به بسته و برعکس، تمیز کردن نام کاربری و ...
    /// </summary>
    public static class ChatText
    {
        // تمیز کردن نام کاربری: حذف کاراکترهای کنترلی و تبدیل کاراکترهای رزرو شده پروتکل به _
        public static string CleanName(string raw)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char ch in (raw ?? ""))
            {
                if (char.IsControl(ch)) continue;
                if (ch == ',' || ch == ';' || ch == ':' || ch == '|' || ch == '%') sb.Append('_');
                else sb.Append(ch);
            }

            string name = sb.ToString().Trim();
            if (name.Length > 20) name = name.Substring(0, 20).Trim();
            return name.Length == 0 ? "کاربر" : name;
        }

        // محدود کردن متن پیام
        public static string LimitText(string text)
        {
            string t = (text ?? "").Trim();
            if (t.Length > ChatConst.MaxTextLength) t = t.Substring(0, ChatConst.MaxTextLength);
            return t;
        }

        // کوتاه کردن متن برای پیش‌نمایش (پاسخ، سنجاق)
        public static string Snippet(string text, int max)
        {
            string t = (text ?? "").Replace("\r", " ").Replace("\n", " ").Trim();
            return t.Length > max ? t.Substring(0, max) + "…" : t;
        }

        // رمزگذاری واکنش‌ها به شکل  emoji:user1,user2;emoji2:user3
        public static string EncodeReactions(List<ReactionInfo> list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ReactionInfo r in list)
            {
                if (r.Users.Count == 0) continue;
                if (sb.Length > 0) sb.Append(';');
                sb.Append(r.Emoji).Append(':').Append(string.Join(",", r.Users));
            }
            return sb.ToString();
        }

        // برعکس EncodeReactions
        public static List<ReactionInfo> DecodeReactions(string s)
        {
            List<ReactionInfo> list = new List<ReactionInfo>();
            if (string.IsNullOrEmpty(s)) return list;

            foreach (string part in s.Split(';'))
            {
                int c = part.IndexOf(':');
                if (c <= 0) continue;

                ReactionInfo r = new ReactionInfo();
                r.Emoji = part.Substring(0, c);
                string users = part.Substring(c + 1);
                if (users.Length > 0) r.Users.AddRange(users.Split(','));
                if (r.Users.Count > 0) list.Add(r);
            }
            return list;
        }

        // تبدیل پیام به بسته MSG
        public static Packet ToPacket(ChatMessageInfo m, bool history)
        {
            return new Packet("MSG",
                m.Id.ToString(CultureInfo.InvariantCulture),
                m.Sender,
                m.Avatar.ToString(CultureInfo.InvariantCulture),
                m.Time.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture),
                m.Text,
                m.ReplyId.ToString(CultureInfo.InvariantCulture),
                m.ReplyName,
                m.ReplyText,
                m.Edited ? "1" : "0",
                EncodeReactions(m.Reactions),
                m.Kind,
                history ? "1" : "0");
        }

        // تبدیل بسته MSG به پیام
        public static ChatMessageInfo FromPacket(Packet p)
        {
            ChatMessageInfo m = new ChatMessageInfo();
            m.Id = p.ArgLong(0);
            m.Sender = p.Arg(1);
            m.Avatar = p.ArgInt(2);
            m.Time = TicksToLocal(p.ArgLong(3));
            m.Text = p.Arg(4);
            m.ReplyId = p.ArgLong(5);
            m.ReplyName = p.Arg(6);
            m.ReplyText = p.Arg(7);
            m.Edited = p.Arg(8) == "1";
            m.Reactions = DecodeReactions(p.Arg(9));
            m.Kind = p.Arg(10) == "server" ? "server" : "user";
            return m;
        }

        // تبدیل Ticks (UTC) به زمان محلی
        public static DateTime TicksToLocal(long ticks)
        {
            try
            {
                return new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
            }
            catch
            {
                return DateTime.Now;
            }
        }
    }

    /// <summary>
    /// توابع کمکی زبان فارسی: اعداد فارسی، تشخیص جهت متن، ساعت و مدت زمان
    /// </summary>
    public static class Fa
    {
        // تبدیل اعداد انگلیسی به فارسی
        public static string Digits(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            StringBuilder sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                sb.Append((c >= '0' && c <= '9') ? (char)('\u06F0' + (c - '0')) : c);
            }
            return sb.ToString();
        }

        public static string Digits(int n)
        {
            return Digits(n.ToString(CultureInfo.InvariantCulture));
        }

        // ساعت به شکل ۱۴:۲۵
        public static string Time(DateTime t)
        {
            return Digits(t.ToString("HH:mm", CultureInfo.InvariantCulture));
        }

        // مدت زمان به شکل ۰۰:۰۵:۲۰
        public static string Duration(TimeSpan d)
        {
            if (d < TimeSpan.Zero) d = TimeSpan.Zero;
            int h = (int)d.TotalHours;
            return Digits(string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", h, d.Minutes, d.Seconds));
        }

        // آیا حرف فارسی/عربی است؟
        private static bool IsArabicScript(char c)
        {
            return (c >= '\u0590' && c <= '\u08FF') || (c >= '\uFB1D' && c <= '\uFDFF') || (c >= '\uFE70' && c <= '\uFEFF');
        }

        /// <summary>
        /// تشخیص جهت متن: اولین حرف معنی‌دار تعیین می‌کند. اگر حرفی نبود (مثلا فقط عدد یا ایموجی) راست‌به‌چپ در نظر گرفته می‌شود
        /// </summary>
        public static bool IsRtl(string text)
        {
            if (string.IsNullOrEmpty(text)) return true;
            foreach (char c in text)
            {
                if (IsArabicScript(c)) return true;
                if (char.IsLetter(c)) return false;
            }
            return true;
        }

        // رنگ ثابت برای هر نام کاربری (برای نمایش نام فرستنده)
        private static readonly Color[] NamePalette =
        {
            Color.FromArgb(220, 38, 38), Color.FromArgb(217, 119, 6), Color.FromArgb(5, 150, 105),
            Color.FromArgb(2, 132, 199), Color.FromArgb(124, 58, 237), Color.FromArgb(219, 39, 119),
            Color.FromArgb(13, 148, 136), Color.FromArgb(79, 70, 229)
        };

        public static Color NameColor(string name, bool dark)
        {
            int h = 7;
            foreach (char c in (name ?? "")) h = h * 31 + c;
            Color col = NamePalette[Math.Abs(h % NamePalette.Length)];
            if (!dark) return col;

            // در تم تیره رنگ را روشن‌تر می‌کنیم تا خوانا باشد
            return Color.FromArgb(Math.Min(255, col.R + 70), Math.Min(255, col.G + 70), Math.Min(255, col.B + 70));
        }
    }
}
