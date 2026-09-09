using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests;

[TestFixture]
public class BarbersTests : PageTest
{
    private const string ApiUrl = "https://localhost:5174";

    [SetUp]
    public async Task SetUp()
    {
        await Page.GotoAsync($"{ApiUrl}/barbers");
    }

    [Test]
    public async Task Barbers_ShouldDisplayHeaderAndSalonCardsCorrectly()
    {
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Available Salons" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Search by name, city or address..."))
            .ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Antic Group" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Dusanova 90")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Nis").Nth(1)).ToBeVisibleAsync();
        await Expect(Page.GetByText("2 Staff")).ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Kvanto" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Dusanova 15")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Beograd")).ToBeVisibleAsync();
        await Expect(Page.GetByText("1 Staff").Nth(0)).ToBeVisibleAsync();
    }

    [Test]
    public async Task Barbers_ShouldFilterSalons_WhenSearching()
    {
        var searchInput = Page.GetByPlaceholder("Search by name, city or address...");
        await searchInput.FillAsync("Group");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Antic Group" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Kvanto" })).ToBeHiddenAsync();
    }

    [Test]
    public async Task Barbers_ShouldDisplayEmptyState_WhenNoSalonsMatchSearch()
    {
        var searchInput = Page.GetByPlaceholder("Search by name, city or address...");
        await searchInput.FillAsync("Something");

        await Expect(Page.GetByText("No salons found.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Try adjusting your search criteria.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Barbers_ShouldNavigateToSalonDetails_WhenViewSalonClicked()
    {
        var viewSalonButton = Page.GetByRole(AriaRole.Button, new() { Name = "View Salon" }).First;
        await viewSalonButton.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/Kvanto.*"));
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
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Antic Group" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Kvanto" }))
            .ToBeVisibleAsync();
    }
}
