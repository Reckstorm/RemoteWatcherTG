using System.Text.Json;
using Domain;
using MediatR;

namespace Application.Rules;

public class Edit
{
    public class Command : IRequest<Result<Rule>>
    {
        public string ProcessName { get; set; }
        public Rule Process { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Rule>>
    {
        public async Task<Result<Rule>> Handle(Command request, CancellationToken cancellationToken)
        {
            var rules = await RegistryAgent.GetRules();

            var list = new List<Rule>();

            if (rules != null || rules != string.Empty) list = JsonSerializer.Deserialize<List<Rule>>(rules);

            var item = list.FirstOrDefault(p => p.ProcessName == request.ProcessName);

            if (item == null) return Result<Rule>.Failure("Rule does not exist");

            list.Remove(item);

            list.Add(request.Process);

            await RegistryAgent.SetRules(JsonSerializer.Serialize(list));

            return Result<Rule>.Success(request.Process);
        }
    }
}