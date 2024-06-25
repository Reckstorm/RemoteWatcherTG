using System.Text.Json;
using Domain;
using MediatR;

namespace Application.RProcesses;

public class Details
{
    public class Query: IRequest<Result<RProcess>>
    {
        public string ProcessName { get; set;}
    }

    public class Handler : IRequestHandler<Query, Result<RProcess>>
    {
        public async Task<Result<RProcess>> Handle(Query request, CancellationToken cancellationToken)
        {
            var rules = await RegistryAgent.GetRules();

            var list = new List<RProcess>();

            if (rules != null || rules != string.Empty) list = JsonSerializer.Deserialize<List<RProcess>>(rules);

            var item = list.FirstOrDefault(p => p.ProcessName == request.ProcessName);

            if (item == null) return Result<RProcess>.Failure("There is no such process");

            return Result<RProcess>.Success(item);
        }
    }
}