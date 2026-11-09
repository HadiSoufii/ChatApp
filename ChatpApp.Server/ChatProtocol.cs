using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace ChatpApp.Protocol
{
    // مدل اصلی همه پیام‌های ردوبدل‌شده بین سرور و کلاینت است.
    // به جای ارسال متن خام، نوع پیام و داده‌های مربوط به آن به‌صورت ساختاریافته ارسال می‌شود.
    [DataContract]
    public sealed class ChatPacket
    {
        [DataMember(Order = 1)] public string Type { get; set; }
        [DataMember(Order = 2)] public string UserName { get; set; }
        [DataMember(Order = 3)] public string Avatar { get; set; }
        [DataMember(Order = 4)] public string Text { get; set; }
        [DataMember(Order = 5)] public string MessageId { get; set; }
        [DataMember(Order = 6)] public string ReplyToId { get; set; }
        [DataMember(Order = 7)] public string ReplyToText { get; set; }
        [DataMember(Order = 8)] public string ReplyToUser { get; set; }
        [DataMember(Order = 9)] public long TimestampTicks { get; set; }
        [DataMember(Order = 10)] public bool IsEdited { get; set; }
        [DataMember(Order = 11)] public bool IsDeleted { get; set; }
        [DataMember(Order = 12)] public bool IsServer { get; set; }
        [DataMember(Order = 13)] public int DeliveredCount { get; set; }
        [DataMember(Order = 14)] public string Reaction { get; set; }
        [DataMember(Order = 15)] public int ReactionCount { get; set; }
        [DataMember(Order = 16)] public bool IsPinned { get; set; }
        [DataMember(Order = 17)] public string IpAddress { get; set; }
        [DataMember(Order = 18)] public long ConnectedAtTicks { get; set; }
        [DataMember(Order = 19)] public UserInfo[] Users { get; set; }
        [DataMember(Order = 20)] public ChatMessageInfo[] History { get; set; }
        [DataMember(Order = 21)] public ReactionCounter[] Reactions { get; set; }
    }

    // اطلاعاتی که سرور برای نمایش فهرست کاربران آنلاین در اختیار کلاینت قرار می‌دهد.
    [DataContract]
    public sealed class UserInfo
    {
        [DataMember(Order = 1)] public string UserName { get; set; }
        [DataMember(Order = 2)] public string Avatar { get; set; }
        [DataMember(Order = 3)] public string IpAddress { get; set; }
        [DataMember(Order = 4)] public long ConnectedAtTicks { get; set; }
    }

    // مدل پیام برای نگهداری تاریخچه، پاسخ به پیام، ویرایش و حذف.
    [DataContract]
    public sealed class ChatMessageInfo
    {
        [DataMember(Order = 1)] public string MessageId { get; set; }
        [DataMember(Order = 2)] public string SenderName { get; set; }
        [DataMember(Order = 3)] public string SenderAvatar { get; set; }
        [DataMember(Order = 4)] public string Text { get; set; }
        [DataMember(Order = 5)] public long TimestampTicks { get; set; }
        [DataMember(Order = 6)] public string ReplyToId { get; set; }
        [DataMember(Order = 7)] public string ReplyToText { get; set; }
        [DataMember(Order = 8)] public string ReplyToUser { get; set; }
        [DataMember(Order = 9)] public bool IsEdited { get; set; }
        [DataMember(Order = 10)] public bool IsDeleted { get; set; }
        [DataMember(Order = 11)] public bool IsServer { get; set; }
        [DataMember(Order = 12)] public ReactionCounter[] Reactions { get; set; }
    }

    // تعداد واکنش هر ایموجی و کاربرانی که آن واکنش را ثبت کرده‌اند.
    [DataContract]
    public sealed class ReactionCounter
    {
        [DataMember(Order = 1)] public string Emoji { get; set; }
        [DataMember(Order = 2)] public int Count { get; set; }
        [DataMember(Order = 3)] public string[] Users { get; set; }
    }

    // این کلاس فریم‌بندی TCP را پیاده‌سازی می‌کند.
    // چون TCP یک جریان بایت است، ممکن است یک پیام در چند Receive برسد؛ بنابراین ابتدا طول پیام ارسال می‌شود.
    public static class FrameProtocol
    {
        private const int MaxPacketBytes = 5 * 1024 * 1024;

        public static void Send(NetworkStream stream, ChatPacket packet, object writeLock)
        {
            if (stream == null || packet == null) throw new ArgumentNullException();

            var serializer = new DataContractJsonSerializer(typeof(ChatPacket));
            byte[] payload;
            using (var memory = new MemoryStream())
            {
                serializer.WriteObject(memory, packet);
                payload = memory.ToArray();
            }

            if (payload.Length > MaxPacketBytes)
                throw new InvalidDataException("حجم پیام از حد مجاز بیشتر است.");

            // طول پیام به ترتیب بایت شبکه (Big Endian) نوشته می‌شود.
            byte[] header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(payload.Length));
            lock (writeLock ?? stream)
            {
                stream.Write(header, 0, header.Length);
                stream.Write(payload, 0, payload.Length);
                stream.Flush();
            }
        }

        public static ChatPacket Receive(NetworkStream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            // ابتدا چهار بایت طول فریم خوانده می‌شود؛ بازگشت null یعنی طرف مقابل اتصال را بسته است.
            byte[] header = ReadExact(stream, 4, true);
            if (header == null) return null;

            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(header, 0));
            if (length <= 0 || length > MaxPacketBytes)
                throw new InvalidDataException("ساختار یا اندازه پیام دریافتی معتبر نیست.");

            byte[] payload = ReadExact(stream, length, false);
            var serializer = new DataContractJsonSerializer(typeof(ChatPacket));
            using (var memory = new MemoryStream(payload))
                return (ChatPacket)serializer.ReadObject(memory);
        }

        private static byte[] ReadExact(NetworkStream stream, int length, bool allowCleanClose)
        {
            byte[] buffer = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int read = stream.Read(buffer, offset, length - offset);
                if (read == 0)
                {
                    if (allowCleanClose && offset == 0) return null;
                    throw new EndOfStreamException("ارتباط هنگام دریافت پیام قطع شد.");
                }
                offset += read;
            }
            return buffer;
        }
    }
}
