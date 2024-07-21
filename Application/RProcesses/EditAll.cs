using System.Text.Json;
using Domain;
using MediatR;

namespace Application.RProcesses
{
    public class EditAll
    {
        public class Command : IRequest<Result<Unit>>
        {
            public RProcessDTO Boundaries { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var rules = await RegistryAgent.GetRules();

                var list = new List<RProcess>();

                if (rules != null && !rules.Equals("")) list = JsonSerializer.Deserialize<List<RProcess>>(rules);

                list.ForEach(r => {
                    r.BlockStartTime = request.Boundaries.StartTime;
                    r.BlockEndTime = request.Boundaries.EndTime;
                });

                await RegistryAgent.SetRules(JsonSerializer.Serialize(list));

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}