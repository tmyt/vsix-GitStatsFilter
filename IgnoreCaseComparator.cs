using System.Collections.Generic;
using System.Globalization;

namespace BranchFilter
{
    internal class IgnoreCaseComparator : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return string.Compare(x, y, true, CultureInfo.InstalledUICulture) == 0;
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}