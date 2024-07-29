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

            if (string.IsNullOrEmpty(rules)) return Result<Unit>.Failure("Failed to run blocker");

            var blocker = Blocker.GetInstance(JsonSerializer.Deserialize<List<Rule>>(rules));

            if (blocker.running) return Result<Unit>.Failure("Blocker is already running");

            blocker.RunBlock();
            
            if (!blocker.running) return Result<Unit>.Failure("Failed to run blocker");

            return Result<Unit>.Success(Unit.Value);
        }
    }
}