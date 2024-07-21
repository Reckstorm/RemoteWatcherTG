using System.Text.Json;
using Domain;
using MediatR;

namespace Application.RProcesses
{
    public class Add
    {
        public class Command : IRequest<Result<Unit>>
        {
            public RProcess Process { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var rules = await RegistryAgent.GetRules();

                var list = new List<RProcess>();

                if (rules != null && !rules.Equals("")) list = JsonSerializer.Deserialize<List<RProcess>>(rules);

                if (list.Any(p => p.ProcessName == request.Process.ProcessName)) return Result<Unit>.Failure("Rule already exists");

                list.Add(request.Process);

                await RegistryAgent.SetRules(JsonSerializer.Serialize(list));

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}

