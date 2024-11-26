using MediatR;

namespace Application.Logic;

public class Stop
{
    public class Command : IRequest<Result<Unit>>
    { }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var blocker = Blocker.GetInstance();

            if (blocker == null) return Result<Unit>.Failure("Blocker is not running");

            if (!blocker.Running) return Result<Unit>.Failure("Blocker is not running");

            await blocker.StopBlock();

            if (blocker.Running) return Result<Unit>.Failure("Failed to stop blocker");

            return Result<Unit>.Success(Unit.Value);
        }
    }
}