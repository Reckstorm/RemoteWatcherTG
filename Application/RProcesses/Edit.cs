using System.Text.Json;
using Domain;
using MediatR;

namespace Application.RProcesses;

public class Edit
{
    public class Command : IRequest<Result<RProcess>>
    {
        public string ProcessName { get; set; }
        public RProcess Process { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<RProcess>>
    {
        public async Task<Result<RProcess>> Handle(Command request, CancellationToken cancellationToken)
        {
            var rules = await RegistryAgent.GetRules();

            var list = new List<RProcess>();

            if (rules != null || rules != string.Empty) list = JsonSerializer.Deserialize<List<RProcess>>(rules);

            var item = list.FirstOrDefault(p => p.ProcessName == request.ProcessName);

            if (item == null) return Result<RProcess>.Failure("Rule does not exist");

            list.Remove(item);

            list.Add(request.Process);

            await RegistryAgent.SetRules(JsonSerializer.Serialize(list));

            return Result<RProcess>.Success(request.Process);
        }
    }
}