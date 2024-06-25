using System.Text.Json;
using Domain;
// using FluentValidation;
using MediatR;

namespace Application.RProcesses;

public class Edit
{
    public class Command : IRequest<Result<Unit>>
    {
        public string ProcessName { get; set; }
        public RProcess Process { get; set; }
    }

    // public class CommandValidator : AbstractValidator<Command>
    // {
    //     public CommandValidator()
    //     {
    //         RuleFor(x => x.ProcessName).NotEmpty();
    //         RuleFor(x => x.Process).SetValidator(new RProcessValidator());
    //     }
    // }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var rules = await RegistryAgent.GetRules();

            var list = new List<RProcess>();

            if (rules != null || rules != string.Empty) list = JsonSerializer.Deserialize<List<RProcess>>(rules);

            var item = list.FirstOrDefault(p => p.ProcessName == request.Process.ProcessName);

            if (item == null) return Result<Unit>.Failure("Rule does not exist");

            list.Remove(item);

            list.Add(request.Process);

            await RegistryAgent.SetRules(JsonSerializer.Serialize(list));

            return Result<Unit>.Success(Unit.Value);
        }
    }
}