using Domain;

namespace TGBot.Services
{
    public class UserRequest
    {
        public string RProcessName { get; set; } = null;
        public string ProcessName { get; set; } = null;
        public int Step { get; set; } = 0;
        public string BaseMenuSection { get; set; }
    }
}