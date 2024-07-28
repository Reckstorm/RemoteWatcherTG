using System.Text.Json;
using Domain;
using MediatR;

namespace Application.Rules
{
    public class Delete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string ProcessName { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var rules = await RegistryAgent.GetRules();

                if (rules == string.Empty) return Result<Unit>.Failure("There is nothing to delete");

                var list = JsonSerializer.Deserialize<List<Rule>>(rules);

                var item = list.FirstOrDefault(p => p.ProcessName == request.ProcessName);

                var res = list.Remove(item);

                if (!res) return Result<Unit>.Failure("There is nothing to delete");

                await RegistryAgent.SetRules(JsonSerializer.Serialize(list));

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}