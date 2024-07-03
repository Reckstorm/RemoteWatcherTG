using Application.Core;
using Application.DTOs;
using MediatR;
namespace Application.Processes
{
    public class List
    {
        public class Query : IRequest<Result<List<CommonProcessDto>>>
        {

        }

        public class Handler : IRequestHandler<Query, Result<List<CommonProcessDto>>>
        {
            public async Task<Result<List<CommonProcessDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var processes = await ProcessAgent.GetProcesses();
                if (processes == null) return Result<List<CommonProcessDto>>.Failure("Failed to get processes");
                return Result<List<CommonProcessDto>>.Success(processes);
            }
        }
    }
}
