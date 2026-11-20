using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Vek.Examples.Data.Models;

public class Table
{
    [JsonPropertyName("table")]
    public IReadOnlyCollection<ElementGroup>? ElementGroups { get; set; }
}
