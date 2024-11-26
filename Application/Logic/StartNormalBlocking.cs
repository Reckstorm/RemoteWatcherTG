using System.Text.Json;
using MediatR;

namespace Application.Logic
{
    public class StartNormalBlocking
    {
        public class Command : IRequest<Result<Unit>>
        { }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var blocker = Blocker.GetInstance();

                await blocker.Block();

                if (blocker.Unblocker.Unblock) return Result<Unit>.Failure("Failed to block");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}