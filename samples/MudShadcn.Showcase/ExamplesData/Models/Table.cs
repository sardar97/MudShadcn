// Ported from MudBlazor.Examples.Data (MudBlazor 9.10.0, MIT). See THIRD-PARTY-NOTICES.md.
#nullable enable

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MudBlazor.Examples.Data.Models;

public class Table
{
    [JsonPropertyName("table")]
    public IReadOnlyCollection<ElementGroup>? ElementGroups { get; set; }
}
