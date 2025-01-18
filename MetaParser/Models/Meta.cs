using System.Collections.Generic;
using System.Linq;

namespace MetaParser.Models;

public sealed class Meta
{
    public Meta()
    {
    }

    public List<Rule> Rules { get; } = [];

    public Dictionary<string, IEnumerable<Rule>> States => Rules.GroupBy(r => r.State).ToDictionary(g => g.Key, g => g.AsEnumerable());
}
