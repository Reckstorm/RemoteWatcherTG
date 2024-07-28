using Application.Core;
using Application.DTOs;
using Domain;
using MediatR;

namespace Application.Processes;

public class Details
{
    public class Query: IRequest<Result<CommonDto>>
    {
        public string ProcessName { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<CommonDto>>
    {
        public async Task<Result<CommonDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var process = await ProcessAgent.GetProcessDetails(request.ProcessName);
            if (process == null) return Result<CommonDto>.Failure("There is no such process");
            return Result<CommonDto>.Success(process);
        }
    }
}