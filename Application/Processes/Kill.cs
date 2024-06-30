using Application.Core;
using MediatR;

namespace Application.Processes;
public class Kill
{
    public class Command : IRequest<Result<ProcessDto>>
    {
        public string ProcessName { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<ProcessDto>>
    {
        public async Task<Result<ProcessDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var result = await ProcessAgent.KillProcess(request.ProcessName);
            if (result.ProcessId == -1) return Result<ProcessDto>.Failure("There is no such process");
            return Result<ProcessDto>.Success(result);
        }
    }
}