using System.Collections.Generic;
using System.Linq;

namespace BatallaDigestiva
{
    public sealed class QuestionRotation
    {
        public readonly HashSet<string> Used = new HashSet<string>();
        public string Last;
        public string Choose(IEnumerable<string> ids, int randomValue)
        {
            var all = ids.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            if (all.Count == 0) return null;
            var candidates = all.Where(id => !Used.Contains(id)).ToList();
            if (candidates.Count == 0)
            {
                Used.Clear();
                candidates = all.Count > 1 ? all.Where(id => id != Last).ToList() : all;
            }
            string selected = candidates[(int)((uint)randomValue % (uint)candidates.Count)];
            Used.Add(selected); Last = selected;
            return selected;
        }
    }
}
