using System.Text.Json;
using Application.DTOs;
using Domain;
using MediatR;

namespace Application.Rules;

public class Details
{
    public class Query: IRequest<Result<CommonDto>>
    {
        public string ProcessName { get; set;}
    }

    public class Handler : IRequestHandler<Query, Result<CommonDto>>
    {
        public async Task<Result<CommonDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var rules = await RegistryAgent.GetRules();

            var list = new List<CommonDto>();

            if (rules != null || rules != string.Empty) 
            {
                var temp = JsonSerializer.Deserialize<List<Rule>>(rules);
                foreach (var rule in temp)
                {
                    list.Add(new CommonDto { ProcessName = rule.ProcessName, StartTime = rule.BlockStartTime, EndTime = rule.BlockEndTime });
                }
            }

            var item = list.FirstOrDefault(p => p.ProcessName == request.ProcessName);

            if (item == null) return Result<CommonDto>.Failure("There is no such process");

            return Result<CommonDto>.Success(item);
        }
    }
}