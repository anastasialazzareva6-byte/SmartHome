using System;

namespace SmartHomeSystem
{
    public class Notification
    {
        public string NotificationId { get; private set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public Device RelatedDevice { get; set; }
        public AutomationScenario RelatedScenario { get; set; }
        public DateTime Timestamp { get; private set; }
        public bool IsRead { get; private set; }

        public Notification(string notificationId, NotificationType type, string message,
                          Device relatedDevice = null, AutomationScenario relatedScenario = null)
        {
            NotificationId = notificationId;
            Type = type;
            Message = message;
            RelatedDevice = relatedDevice;
            RelatedScenario = relatedScenario;
            Timestamp = DateTime.Now;
            IsRead = false;
        }

        public void MarkAsRead()
        {
            IsRead = true;
            Console.WriteLine($"[NOTIF] Уведомление {NotificationId} отмечено как прочитанное");
        }

        public void Send()
        {
            Console.WriteLine($"\n[УВЕДОМЛЕНИЕ] {Timestamp:HH:mm:ss}");
            Console.WriteLine($"Тип: {GetTypeString()}");
            Console.WriteLine($"Сообщение: {Message}");

            if (RelatedDevice != null)
                Console.WriteLine($"Устройство: {RelatedDevice.Name}");

            if (RelatedScenario != null)
                Console.WriteLine($"Сценарий: {RelatedScenario.Name}");
        }

        private string GetTypeString()
        {
            switch (Type)
            {
                case NotificationType.INFO: return "ИНФО";
                case NotificationType.WARNING: return "ПРЕДУПРЕЖДЕНИЕ";
                case NotificationType.ALERT: return "ТРЕВОГА";
                default: return "НЕИЗВЕСТНО";
            }
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"[{Timestamp:HH:mm:ss}] {(IsRead ? "✓" : "✗")} " +
                $"{GetTypeString()}: {Message}");
        }

        public string Serialize()
        {
            return $"{NotificationId}|{(int)Type}|{Message}|" +
                   $"{(RelatedDevice != null ? RelatedDevice.DeviceId : "NULL")}|" +
                   $"{(RelatedScenario != null ? RelatedScenario.ScenarioId : "NULL")}|" +
                   $"{Timestamp}|{IsRead}";
        }

        public static Notification Deserialize(string data)
        {
            var parts = data.Split('|');
            if (parts.Length != 7) return null;

            return new Notification(
                parts[0],
                (NotificationType)int.Parse(parts[1]),
                parts[2]
            )
            {
                Timestamp = DateTime.Parse(parts[5]),
                IsRead = bool.Parse(parts[6])
            };
        }

        public override string ToString()
        {
            return $"{GetTypeString()}: {Message}";
        }
    }
}