using Domain;

namespace TGBot.Services
{
    public class UserRequest
    {
        public RProcess RProcess { get; set; } = null;
        public string Item { get; set; } = null;
        public List<string> Items { get; set; } = null;
        public int Step { get; set; } = 0;
        public string BaseMenuSection { get; set; }
    }
}