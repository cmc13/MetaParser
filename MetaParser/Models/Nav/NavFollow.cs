using System.Runtime.Serialization;

namespace MetaParser.Models
{
    public class NavFollow : ISerializable
    {
        public int TargetId { get; set; }

        public string TargetName { get; set; }

        public NavFollow() { }

        protected NavFollow(SerializationInfo info, StreamingContext context)
        {
            TargetId = info.GetInt32(nameof(TargetId));
            TargetName = info.GetString(nameof(TargetName));
        }

        public override string ToString() => $"Follow: {TargetName} ({TargetId})";

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(TargetId), TargetId);
            info.AddValue(nameof(TargetName), TargetName, typeof(string));
        }
    }
}