using MediatR;

namespace Application.Logic
{
    public class Status
    {
        public class Query : IRequest<Result<bool>>
        { }

        public class Handler : IRequestHandler<Query, Result<bool>>
        {
            public async Task<Result<bool>> Handle(Query request, CancellationToken cancellationToken)
            {
                var blocker = Blocker.GetInstance();

                if (blocker == null) return Result<bool>.Failure("Failed to check status");

                return Result<bool>.Success(blocker.running);
            }
        }
    }
}