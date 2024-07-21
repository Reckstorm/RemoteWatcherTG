using Application.DTOs;
using Application.RProcesses;
using Domain;

namespace TGBot.Services
{
    public class UserRequest
    {
        public string Item { get; set; } = null;
        public RProcessDTO Boundaries { get; set; } = new RProcessDTO();
        public List<CommonProcessDto> Items { get; set; } = new List<CommonProcessDto>();
        public string BaseMenuSection { get; set; } = null;
        // public string ChainMenuSection { get; set; } = null;
    }
}