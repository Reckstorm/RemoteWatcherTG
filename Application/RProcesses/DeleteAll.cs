using MediatR;

namespace Application.RProcesses
{
    public class DeleteAll
    {
        public class Command : IRequest<Result<Unit>>
        { }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var rules = await RegistryAgent.GetRules();

                if (rules == string.Empty) return Result<Unit>.Failure("There is nothing to delete");

                await RegistryAgent.SetRules("");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}