using Application.Core;
using Application.DTOs;
using MediatR;

namespace Application.Processes;
public class Kill
{
    public class Command : IRequest<Result<CommonProcessDto>>
    {
        public string ProcessName { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<CommonProcessDto>>
    {
        public async Task<Result<CommonProcessDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var result = await ProcessAgent.KillProcess(request.ProcessName);
            if (result.ProcessId == -1) return Result<CommonProcessDto>.Failure("There is no such process");
            return Result<CommonProcessDto>.Success(result);
        }
    }
}