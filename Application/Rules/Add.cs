using System.Text.Json;
using Domain;
using MediatR;

namespace Application.Rules
{
    public class Add
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Rule Process { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var rules = await RegistryAgent.GetRules();

                var list = new List<Rule>();

                if (rules != null && !rules.Equals("")) list = JsonSerializer.Deserialize<List<Rule>>(rules);

                if (list.Any(p => p.ProcessName == request.Process.ProcessName)) return Result<Unit>.Failure("Rule already exists");

                list.Add(request.Process);

                await RegistryAgent.SetRules(JsonSerializer.Serialize(list));

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}

