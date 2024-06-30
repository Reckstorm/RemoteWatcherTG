using Application.Core;
using MediatR;
namespace Application.Processes
{
    public class List
    {
        public class Query : IRequest<Result<List<ProcessDto>>>
        {

        }

        public class Handler : IRequestHandler<Query, Result<List<ProcessDto>>>
        {
            public async Task<Result<List<ProcessDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var processes = await ProcessAgent.GetProcesses();
                if (processes == null) return Result<List<ProcessDto>>.Failure("Failed to get processes");
                return Result<List<ProcessDto>>.Success(processes);
            }
        }
    }
}
