using System.Text.Json;
using Application.DTOs;
using Domain;
using MediatR;
namespace Application.RProcesses
{
    public class List
    {
        public class Query : IRequest<Result<List<CommonProcessDto>>>
        {

        }

        public class Handler : IRequestHandler<Query, Result<List<CommonProcessDto>>>
        {
            public async Task<Result<List<CommonProcessDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var rules = await RegistryAgent.GetRules();

                if (string.IsNullOrEmpty(rules)) return null;

                return Result<List<CommonProcessDto>>.Success(JsonSerializer.Deserialize<List<CommonProcessDto>>(rules));
            }
        }
    }
}
