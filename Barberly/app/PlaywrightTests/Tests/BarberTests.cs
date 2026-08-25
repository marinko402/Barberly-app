using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests;

[TestFixture]
public class BarbersTests : PageTest
{
    private const string ApiUrl = "https://localhost:5174";

    private readonly object[] sampleSalons =
    [
        new
        {
            salonId = "salon-1",
            name = "Gentlemen Barber",
            city = "Nis",
            address = "Bulevar Nemanjica 12",
            barbers = new[] { new { id = "b1" }, new { id = "b2" } },
        },
        new
        {
            salonId = "salon-2",
            name = "Urban Cut",
            city = "Beograd",
            address = "Knez Mihailova 5",
            barbers = new[] { new { id = "b3" } },
        },
    ];

    [SetUp]
    public async Task SetUp()
    {
        await SetupBaseMocks(sampleSalons);
    }

    private async Task SetupBaseMocks(object scheduleData, int statusCode = 200)
    {
        await Page.UnrouteAllAsync();

        await Context.AddCookiesAsync([
            new Cookie
            {
                Name = "jwt",
                Value = "mock-jwt-token-value",
                Url = ApiUrl,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteAttribute.Strict,
            },
        ]);

        await Page.RouteAsync(
            "**/Salon/GetAllSalons*",
            async route =>
            {
                if (statusCode == 200)
                {
                    await route.FulfillAsync(
                        new RouteFulfillOptions
                        {
                            Status = 200,
                            ContentType = "application/json",
                            Body = JsonSerializer.Serialize(scheduleData),
                        }
                    );
                }
                else
                {
                    await route.FulfillAsync(
                        new RouteFulfillOptions
                        {
                            Status = statusCode,
                            ContentType = "application/json",
                            Body = JsonSerializer.Serialize(
                                new { message = "Error fetching salons" }
                            ),
                        }
                    );
                }
            }
        );
    }

    [Test]
    public async Task Barbers_ShouldDisplayHeaderAndSalonCardsCorrectly()
    {
        await Page.GotoAsync($"{ApiUrl}/barbers");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Available Salons" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Search by name, city or address..."))
            .ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Gentlemen Barber" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Bulevar Nemanjica 12")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Nis")).ToBeVisibleAsync();
        await Expect(Page.GetByText("2 Staff")).ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Urban Cut" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Knez Mihailova 5")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Beograd")).ToBeVisibleAsync();
        await Expect(Page.GetByText("1 Staff")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Barbers_ShouldFilterSalons_WhenSearching()
    {
        await Page.GotoAsync($"{ApiUrl}/barbers");

        var searchInput = Page.GetByPlaceholder("Search by name, city or address...");
        await searchInput.FillAsync("Urban");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Urban Cut" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Gentlemen Barber" }))
            .ToBeHiddenAsync();
    }

    [Test]
    public async Task Barbers_ShouldDisplayEmptyState_WhenNoSalonsMatchSearch()
    {
        await Page.GotoAsync($"{ApiUrl}/barbers");

        var searchInput = Page.GetByPlaceholder("Search by name, city or address...");
        await searchInput.FillAsync("Nepostojeci Salon");

        await Expect(Page.GetByText("No salons found.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Try adjusting your search criteria.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Barbers_ShouldDisplayErrorState_WhenApiFails()
    {
        await SetupBaseMocks(Array.Empty<object>(), statusCode: 500);
        await Page.GotoAsync($"{ApiUrl}/barbers");

        await Expect(Page.GetByText("Failed to load salons."))
            .ToBeVisibleAsync(new() { Timeout = 15000 });
        await Expect(Page.GetByText("Please check your internet connection and try again later."))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Barbers_ShouldNavigateToSalonDetails_WhenViewSalonClicked()
    {
        await Page.GotoAsync($"{ApiUrl}/barbers");

        var viewSalonButton = Page.GetByRole(AriaRole.Button, new() { Name = "View Salon" }).First;
        await viewSalonButton.ClickAsync();

        await Expect(Page)
            .ToHaveURLAsync(
                new System.Text.RegularExpressions.Regex(
                    ".*/salon/Gentlemen%20Barber.*|.*/salon/Gentlemen Barber.*"
                )
            );
    }

    [Test]
    public async Task Barbers_ShouldRenderCorrectly_OnMobileViewport()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync($"{ApiUrl}/barbers");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Available Salons" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Search by name, city or address..."))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Gentlemen Barber" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Urban Cut" }))
            .ToBeVisibleAsync();
    }
}
