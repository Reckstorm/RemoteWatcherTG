using System.Text.Json;
using Domain;
// using FluentValidation;
using MediatR;

namespace Application.RProcesses
{
    public class EditAll
    {
        public class Command : IRequest<Result<Unit>>
        {
            public RProcessDTO Boundaries { get; set; }
        }

        // public class CommandValidator : AbstractValidator<Command>
        // {
        //     public CommandValidator()
        //     {
        //     RuleFor(x => x.Boundaries.StartTime).GreaterThanOrEqualTo(TimeOnly.Parse("00:00:00")).LessThanOrEqualTo(TimeOnly.Parse("23:59:59"));
        //     RuleFor(x => x.Boundaries.EndTime).LessThanOrEqualTo(TimeOnly.Parse("23:59:59")).GreaterThanOrEqualTo(TimeOnly.Parse("00:00:00"));
        //     }
        // }

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