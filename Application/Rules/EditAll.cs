using System.Text.Json;
using Application.DTOs;
using Domain;
using MediatR;

namespace Application.Rules
{
    public class EditAll
    {
        public class Command : IRequest<Result<Unit>>
        {
            public RuleDto Boundaries { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var rules = await RegistryAgent.GetRules();

                var list = new List<Rule>();

                if (rules != null && !rules.Equals("")) list = JsonSerializer.Deserialize<List<Rule>>(rules);

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