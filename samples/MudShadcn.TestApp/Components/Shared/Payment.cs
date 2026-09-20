using MudBlazor;

namespace MudShadcn.TestApp.Components.Shared;

public sealed record Payment(string Id, string Customer, string Email, string Status, string Method, decimal Amount, DateTime Date)
{
    private static readonly string[] Names =
    [
        "Olivia Martin", "Jackson Lee", "Isabella Nguyen", "William Kim", "Sofia Davis", "Liam Johnson",
        "Emma Wilson", "Noah Brown", "Ava Garcia", "Lucas Miller", "Mia Anderson", "Ethan Thomas",
    ];

    private static readonly string[] Statuses = ["Paid", "Pending", "Processing", "Failed"];
    private static readonly string[] Methods = ["Credit card", "PayPal", "Bank transfer"];

    public static IReadOnlyList<Payment> Sample { get; } = Enumerable.Range(0, 48).Select(i =>
    {
        var name = Names[i % Names.Length];
        return new Payment(
            $"INV{i + 1:000}",
            name,
            name.ToLowerInvariant().Replace(' ', '.') + "@example.com",
            Statuses[(i * 7) % Statuses.Length],
            Methods[(i * 5) % Methods.Length],
            Math.Round(50m + (i * 137.31m % 900m), 2),
            new DateTime(2026, 9, 1).AddDays(-i * 3));
    }).ToList();

    public static Color StatusColor(string status) => status switch
    {
        "Paid" => Color.Success,
        "Failed" => Color.Error,
        "Processing" => Color.Info,
        _ => Color.Default,
    };
}
