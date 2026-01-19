using System;

namespace SmartHomeSystem
{
    public class Activity
    {
        public string ActivityId { get; private set; }
        public User User { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; private set; }
        public object RelatedObject { get; set; }

        public Activity(string activityId, User user, string action, object relatedObject = null)
        {
            ActivityId = activityId;
            User = user;
            Action = action;
            Timestamp = DateTime.Now;
            RelatedObject = relatedObject;
        }

        public void LogActivity()
        {
            Console.WriteLine($"[ACT] {Timestamp:HH:mm:ss} - {User?.Username}: {Action}");
        }

        public string Serialize()
        {
            return $"{ActivityId}|{User?.UserId}|{Action}|{Timestamp}|" +
                   $"{(RelatedObject != null ? RelatedObject.ToString() : "NULL")}";
        }

        public override string ToString()
        {
            return $"{Timestamp:HH:mm:ss} {User?.Username} - {Action}";
        }
    }
}