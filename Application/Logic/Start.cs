using System.Text.Json;
using Domain;
using MediatR;

namespace Application.Logic;

public class Start
{
    public class Command : IRequest<Result<Unit>>
    { }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var rules = await RegistryAgent.GetRules();
            var unblocker = await RegistryAgent.GetUnblocker();

            if (string.IsNullOrEmpty(rules)) return Result<Unit>.Failure("Failed to run blocker");
            if (string.IsNullOrEmpty(unblocker)) return Result<Unit>.Failure("Failed to run blocker");

            var blocker = Blocker.GetInstance(JsonSerializer.Deserialize<List<Rule>>(rules), JsonSerializer.Deserialize<Unblocker>(unblocker));

            if (blocker.Running) return Result<Unit>.Failure("Blocker is already running");

            blocker.RunBlock();
            
            if (!blocker.Running) return Result<Unit>.Failure("Failed to run blocker");

            return Result<Unit>.Success(Unit.Value);
        }
    }
}