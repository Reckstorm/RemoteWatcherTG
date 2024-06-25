using System.Text.RegularExpressions;
using Domain;
// using FluentValidation;

namespace Application.RProcesses;
// {
//     public class RProcessValidator : AbstractValidator<RProcess>
//     {
//         public RProcessValidator()
//         {
//             RuleFor(x => x.ProcessName).NotEmpty();
//             RuleFor(x => x.BlockStartTime).GreaterThanOrEqualTo(TimeOnly.Parse("00:00:00")).LessThanOrEqualTo(TimeOnly.Parse("23:59:59"));
//             RuleFor(x => x.BlockEndTime).LessThanOrEqualTo(TimeOnly.Parse("23:59:59")).GreaterThanOrEqualTo(TimeOnly.Parse("00:00:00"));
//         }  
//     }
// }