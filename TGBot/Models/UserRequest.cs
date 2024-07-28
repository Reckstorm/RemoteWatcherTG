using Application.DTOs;

namespace TGBot.Models
{
    public class UserRequest
    {
        public RuleDto Boundaries { get; set; } = new RuleDto();
        public List<CommonDto> Items { get; set; } = new List<CommonDto>();
        public string Menu { get; set; } = string.Empty;
        public string SubMenu { get; set; } = string.Empty;
        public string Item { get; set; } = string.Empty;
        public string ItemMenu { get; set; } = string.Empty;
    }
}