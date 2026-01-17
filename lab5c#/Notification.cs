using System;

namespace SmartHomeSystem
{
    public class Notification
    {
        public string NotificationId { get; private set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; private set; }
        public bool IsRead { get; private set; }
        public object RelatedObject { get; set; }
        public string Sender { get; set; }

        public Notification(string notificationId, NotificationType type, string message,
                          object relatedObject = null, string sender = "Система")
        {
            NotificationId = notificationId;
            Type = type;
            Message = message;
            Timestamp = DateTime.Now;
            IsRead = false;
            RelatedObject = relatedObject;
            Sender = sender;
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }

        public void DisplayInfo()
        {
            string typeString = "";
            switch (Type)
            {
                case NotificationType.INFO: typeString = "[ИНФО]"; break;
                case NotificationType.WARNING: typeString = "[ПРЕДУПР]"; break;
                case NotificationType.ALERT: typeString = "[ТРЕВОГА]"; break;
            }

            string readStatus = IsRead ? "✓" : "✗";
            Console.WriteLine($"{readStatus} {Timestamp:HH:mm:ss} {typeString} {Message}");
        }

        public string Serialize()
        {
            return $"{NotificationId}|{(int)Type}|{Message}|{Timestamp}|{IsRead}|{Sender}";
        }

        public static Notification Deserialize(string data)
        {
            var parts = data.Split('|');
            if (parts.Length != 6) return null;

            return new Notification(
                parts[0],
                (NotificationType)int.Parse(parts[1]),
                parts[2]
            )
            {
                Timestamp = DateTime.Parse(parts[3]),
                IsRead = bool.Parse(parts[4]),
                Sender = parts[5]
            };
        }

        public override string ToString()
        {
            return $"{Timestamp:HH:mm:ss} - {Message}";
        }
    }
}