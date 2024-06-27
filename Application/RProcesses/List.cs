using System.Text.Json;
using Domain;
using MediatR;
namespace Application.RProcesses
{
    public class List
    {
        public class Query : IRequest<Result<List<RProcess>>>
        {

        }

        public class Handler : IRequestHandler<Query, Result<List<RProcess>>>
        {
            public async Task<Result<List<RProcess>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var rules = await RegistryAgent.GetRules();

                if (string.IsNullOrEmpty(rules)) return null;

                return Result<List<RProcess>>.Success(JsonSerializer.Deserialize<List<RProcess>>(rules));
            }
        }
    }
}
