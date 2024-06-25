using System.Text.Json;
using Application.Core;
using MediatR;
namespace Application.Processes
{
    public class List
    {
        public class Query : IRequest<Result<string>>
        {

        }

        public class Handler : IRequestHandler<Query, Result<string>>
        {
            public async Task<Result<string>> Handle(Query request, CancellationToken cancellationToken)
            {
                return Result<string>.Success(JsonSerializer.Serialize(await ProcessAgent.GetProcesses()));
            }
        }
    }
}
