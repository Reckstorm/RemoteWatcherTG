using Application.Core;
using MediatR;

namespace Application.Processes;

public class Details
{
    public class Query: IRequest<Result<ProcessDto>>
    {
        public string ProcessName { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ProcessDto>>
    {
        public async Task<Result<ProcessDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var process = await ProcessAgent.GetProcessDetails(request.ProcessName);
            if (process == null) return Result<ProcessDto>.Failure("There is no such process");
            return Result<ProcessDto>.Success(process);
        }
    }
}