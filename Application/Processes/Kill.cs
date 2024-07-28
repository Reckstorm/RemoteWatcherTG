using Application.Core;
using Application.DTOs;
using MediatR;

namespace Application.Processes;
public class Kill
{
    public class Command : IRequest<Result<CommonDto>>
    {
        public string ProcessName { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<CommonDto>>
    {
        public async Task<Result<CommonDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var result = await ProcessAgent.KillProcess(request.ProcessName);
            if (result.ProcessId == -1) return Result<CommonDto>.Failure("There is no such process");
            return Result<CommonDto>.Success(result);
        }
    }
}