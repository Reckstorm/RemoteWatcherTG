using System.Text.Json;
using MediatR;

namespace Application.Logic
{
    public class StartNormalBlocking
    {
        public class Command : IRequest<Result<Unit>>
        { }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var rules = await RegistryAgent.GetRules();

                var list = new List<Domain.Rule>();

                if (rules != null && !rules.Equals("")) list = JsonSerializer.Deserialize<List<Domain.Rule>>(rules);

                list.ForEach(r =>
                {
                    r.UnblockedUntilStart = false;
                });

                await RegistryAgent.SetRules(JsonSerializer.Serialize(list));

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}