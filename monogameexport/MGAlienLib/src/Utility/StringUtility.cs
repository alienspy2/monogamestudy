
using System;

namespace MGAlienLib
{
    public static class StringUtility
    {
        public static bool IsMidMatch(this string str, string pattern, bool ignoreCase = true)
        {
            if (ignoreCase)
            {
                str = str.ToLowerInvariant();
                pattern = pattern.ToLowerInvariant();
            }

            string[] terms = pattern.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (terms.Length == 0) return true;

            bool matchesInOrder = true;
            int lastIndex = -1;

            foreach (string term in terms)
            {
                if (string.IsNullOrEmpty(term)) continue;

                int currentIndex = str.IndexOf(term, lastIndex + 1);

                if (currentIndex == -1 || (lastIndex >= 0 && currentIndex <= lastIndex))
                {
                    matchesInOrder = false;
                    break;
                }
                lastIndex = currentIndex;
            }

            return matchesInOrder;
        }
    }
}
