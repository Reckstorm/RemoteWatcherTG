using System.Text.Json;
using Application.DTOs;
using Domain;
using MediatR;

namespace Application.RProcesses;

public class Details
{
    public class Query: IRequest<Result<CommonProcessDto>>
    {
        public string ProcessName { get; set;}
    }

    public class Handler : IRequestHandler<Query, Result<CommonProcessDto>>
    {
        public async Task<Result<CommonProcessDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var rules = await RegistryAgent.GetRules();

            var list = new List<CommonProcessDto>();

            if (rules != null || rules != string.Empty) list = JsonSerializer.Deserialize<List<CommonProcessDto>>(rules);

            var item = list.FirstOrDefault(p => p.ProcessName == request.ProcessName);

            if (item == null) return Result<CommonProcessDto>.Failure("There is no such process");

            return Result<CommonProcessDto>.Success(item);
        }
    }
}