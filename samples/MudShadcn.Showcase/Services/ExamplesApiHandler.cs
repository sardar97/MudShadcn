#nullable enable

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MudBlazor.Examples.Data;

namespace MudShadcn.Showcase.Services;

/// <summary>
/// MudBlazor's examples load their sample data from a small web API that mudblazor.com hosts
/// (<c>webapi/periodictable</c>, <c>webapi/AmericanStates</c>). The showcase is a static site with no
/// server, so this handler answers those requests in the browser with the same data and the same
/// behaviour as MudBlazor's controllers. Every other request goes to the network.
/// </summary>
public sealed class ExamplesApiHandler : DelegatingHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly PeriodicTableService PeriodicTable = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? "";
        var index = path.IndexOf("/webapi/", StringComparison.OrdinalIgnoreCase);
        if (request.Method != HttpMethod.Get || index < 0)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var segments = path[(index + "/webapi/".Length)..]
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.UnescapeDataString)
            .ToArray();

        object? result = segments switch
        {
            ["periodictable"] => await PeriodicTable.GetElements(),
            ["periodictable", var search] => await PeriodicTable.GetElements(search),
            ["AmericanStates"] or ["americanstates"] => AmericanStates.GetStates(),
            ["AmericanStates" or "americanstates", "searchWithDelay" or "searchwithdelay", .. var rest] =>
                await SearchStatesWithDelayAsync(rest.FirstOrDefault() ?? "", cancellationToken),
            ["AmericanStates" or "americanstates", var search] => AmericanStates.GetStates(search),
            _ => null,
        };

        return result is null
            ? new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request }
            : new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request, Content = JsonContent.Create(result, options: JsonOptions) };
    }

    // Mirrors AmericanStatesController.SearchWithDelay: 40ms per state, honouring cancellation, so the
    // autocomplete examples that demonstrate cancelling a slow search behave as on mudblazor.com.
    private static async Task<List<string>> SearchStatesWithDelayAsync(string search, CancellationToken cancellationToken)
    {
        var input = search.Trim();
        var result = new List<string>();
        foreach (var state in AmericanStates.GetStates())
        {
            if (input.Length == 0 || state.Contains(input, StringComparison.InvariantCultureIgnoreCase))
            {
                result.Add(state);
            }

            await Task.Delay(40, cancellationToken);
        }

        return result;
    }
}
