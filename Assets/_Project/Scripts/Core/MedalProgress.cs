using System;
using System.Collections.Generic;

namespace BatallaDigestiva
{
    public sealed class MedalProgress
    {
        private readonly Dictionary<string, int> kills = new Dictionary<string, int>();
        public int Required { get; private set; } = 5;
        public void Reset(int required) { kills.Clear(); Required = Math.Max(1, required); }
        public int Kills(string id) => id != null && kills.TryGetValue(id, out var count) ? count : 0;
        public int Medals(string id) => Kills(id) / Required;
        public int TowardsNext(string id) => Kills(id) % Required;
        public bool Register(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            kills[id] = Kills(id) + 1;
            return TowardsNext(id) == 0;
        }
        public bool HasAll(IEnumerable<string> ids, int expected = 8)
        {
            var unique = new HashSet<string>(ids);
            if (unique.Count != expected) return false;
            foreach (var id in unique) if (Medals(id) == 0) return false;
            return true;
        }
    }
}
