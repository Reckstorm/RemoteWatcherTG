using Domain;

namespace TGBot.Services
{
    public class UserRequestHandling
    {
        public RProcess RProcessesBeingEdited { get; set; }
        public long ChatId { get; set; }
        public int EditStep { get; set; }
    }
}