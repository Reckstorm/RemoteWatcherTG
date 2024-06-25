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
            var blocker = Blocker.GetInstance(JsonSerializer.Deserialize<List<RProcess>>(await RegistryAgent.GetRules()));

            if (blocker.running) return Result<Unit>.Failure("Blocker is already running");

            blocker.RunBlock();
            
            if (!blocker.running) return Result<Unit>.Failure("Failed to run blocker");

            return Result<Unit>.Success(Unit.Value);
        }
    }
}