using Application.DTOs;

namespace Application.DTOComparers
{
    public class CommonDtoEqualityComparer : EqualityComparer<CommonDto>
    {
        public override bool Equals(CommonDto x, CommonDto y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            return x.ProcessName == y.ProcessName;
        }

        public override int GetHashCode(CommonDto obj) => obj.ProcessName.GetHashCode();
    }
}