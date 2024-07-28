using Application.Core;
using Application.DTOs;
using MediatR;
namespace Application.Processes
{
    public class List
    {
        public class Query : IRequest<Result<List<CommonDto>>>
        {

        }

        public class Handler : IRequestHandler<Query, Result<List<CommonDto>>>
        {
            public async Task<Result<List<CommonDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var processes = await ProcessAgent.GetProcesses();
                if (processes == null) return Result<List<CommonDto>>.Failure("Failed to get processes");
                return Result<List<CommonDto>>.Success(processes);
            }
        }
    }
}
