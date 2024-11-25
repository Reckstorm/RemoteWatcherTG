using System.Data;
using System.Text.Json;
using Application.DTOs;
using MediatR;

namespace Application.Logic
{
    public class Status
    {
        public class Query : IRequest<Result<StatusDto>>
        { }

        public class Handler : IRequestHandler<Query, Result<StatusDto>>
        {
            public async Task<Result<StatusDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var blocker = Blocker.GetInstance();

                StatusDto status = new StatusDto();

                if (blocker == null) return Result<StatusDto>.Failure("Failed to check status");

                status.LogicStatus = blocker.running; 
                status.StoppedUntilStartTimeStatus = blocker.unblock;

                return Result<StatusDto>.Success(status);
            }
        }
    }
}