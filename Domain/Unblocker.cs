using System.Text.Json.Serialization;

namespace Domain
{
    public class Unblocker
    {
        private object _locker { get; set; } = new object();

        private bool _unblock = false;
        public bool Unblock
        {
            get { lock (_locker) return _unblock; }
            set { lock (_locker) _unblock = value; }
        }

        private DateOnly _unblockDate;
        public DateOnly UnblockDate
        {
            get { lock (_locker) return _unblockDate; }
            set { lock (_locker) _unblockDate = value; }
        }
    }
}