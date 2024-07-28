using System.Text.Json;
using Application.DTOs;
using Domain;
using MediatR;
namespace Application.Rules
{
    public class List
    {
        public class Query : IRequest<Result<List<CommonDto>>>
        {

        }

        public class Handler : IRequestHandler<Query, Result<List<CommonDto>>>
        {
            public async Task<Result<List<CommonDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var rules = await RegistryAgent.GetRules();

                if (string.IsNullOrEmpty(rules)) return null;

                var list = new List<CommonDto>();

                var temp = JsonSerializer.Deserialize<List<Rule>>(rules);
                foreach (var rule in temp)
                {
                    list.Add(new CommonDto { ProcessName = rule.ProcessName, StartTime = rule.BlockStartTime, EndTime = rule.BlockEndTime });
                }

                return Result<List<CommonDto>>.Success(list);
            }
        }
    }
}
