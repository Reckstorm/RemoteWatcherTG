using MediatR;

namespace Application.Logic
{
    public class StopUntilStartTime
    {
        public class Command : IRequest<Result<Unit>>
        { }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var blocker = Blocker.GetInstance();

                await blocker.Unblock();

                if (!blocker.unblock) return Result<Unit>.Failure("Failed to unblock");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}