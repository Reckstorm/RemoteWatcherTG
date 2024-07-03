using Application.Core;
using Application.DTOs;
using Domain;
using MediatR;

namespace Application.Processes;

public class Details
{
    public class Query: IRequest<Result<CommonProcessDto>>
    {
        public string ProcessName { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<CommonProcessDto>>
    {
        public async Task<Result<CommonProcessDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var process = await ProcessAgent.GetProcessDetails(request.ProcessName);
            if (process == null) return Result<CommonProcessDto>.Failure("There is no such process");
            return Result<CommonProcessDto>.Success(process);
        }
    }
}