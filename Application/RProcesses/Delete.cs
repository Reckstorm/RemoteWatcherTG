using System.Text.Json;
using Domain;
// using FluentValidation;
using MediatR;

namespace Application.RProcesses
{
    public class Delete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string ProcessName { get; set; }
        }

        // public class CommandValidator: AbstractValidator<Command>
        // {
        //     public CommandValidator()
        //     {
        //         RuleFor(x => x.ProcessName).NotEmpty();
        //     }
        // }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var rules = await RegistryAgent.GetRules();

                if (rules == string.Empty) return Result<Unit>.Failure("There is nothing to delete");

                var list = JsonSerializer.Deserialize<List<RProcess>>(rules);

                var item = list.FirstOrDefault(p => p.ProcessName == request.ProcessName);

                var res = list.Remove(item);

                if (!res) return Result<Unit>.Failure("There is nothing to delete");

                await RegistryAgent.SetRules(JsonSerializer.Serialize(list));

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}