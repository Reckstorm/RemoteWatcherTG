using MediatR;
namespace Application.RProcesses
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
                return Result<string>.Success(await RegistryAgent.GetRules());
            }
        }
    }
}
