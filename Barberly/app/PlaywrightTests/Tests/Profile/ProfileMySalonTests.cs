using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[TestFixture]
public class ProfileMySalonTests : BaseProfileTest
{
    [SetUp]
    public async Task SetUp()
    {
        await LoginAsync("user_50d04554", "Password123!");
    }

    [Test, Order(1)]
    public async Task MySalon_ShouldDisplayRegistrationForm_WhenUserHasNoSalon()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Expect(Page.GetByText("Register New Salon")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Launch Salon" }))
            .ToBeVisibleAsync();
    }

    [Test, Order(2)]
    public async Task MySalon_ShouldShowValidationErrors_WhenRegisteringWithEmptyFields()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Launch Salon" }).ClickAsync();

        await Expect(Page.GetByText("Salon name must be at least 2 characters")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Address is required")).ToBeVisibleAsync();
        await Expect(Page.GetByText("City is required")).ToBeVisibleAsync();
    }

    [Test, Order(3)]
    public async Task MySalon_ShouldRegisterNewSalonSuccessfully()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByPlaceholder("e.g., The Gentleman's Club").FillAsync("Gentleman Barber");
        await Page.GetByPlaceholder("e.g., Knez Mihailova 21").FillAsync("Knez Mihailova 10");
        await Page.GetByPlaceholder("e.g., Belgrade").FillAsync("Belgrade");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Launch Salon" }).ClickAsync();

        await Expect(Page.GetByText("Salon registered successfully!")).ToBeVisibleAsync();
    }

    [Test, Order(4)]
    public async Task MySalon_ShouldDisplayExistingSalonDetailsAndTeam()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Expect(Page.GetByPlaceholder("e.g., The Gentleman's Club"))
            .ToHaveValueAsync("Gentleman Barber");
        await Expect(Page.GetByPlaceholder("e.g., Knez Mihailova 21"))
            .ToHaveValueAsync("Knez Mihailova 10");
        await Expect(Page.GetByPlaceholder("e.g., Belgrade")).ToHaveValueAsync("Belgrade");

        await Expect(Page.GetByText("Active Team (1)")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Dusan Maksimovic").Nth(2)).ToBeVisibleAsync();
        await Expect(Page.GetByText("@user_50d04554").Nth(1)).ToBeVisibleAsync();
    }

    [Test, Order(5)]
    public async Task MySalon_ShouldUpdateSalonDetailsSuccessfully()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit Salon" }).ClickAsync();

        await Page.GetByPlaceholder("e.g., The Gentleman's Club").FillAsync("Updated Salon Name");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

        await Expect(Page.GetByText("Salon updated successfully!")).ToBeVisibleAsync();
    }

    [Test, Order(6)]
    public async Task MySalon_ShouldAddBarberToTeamSuccessfully()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByPlaceholder("e.g., john.barber").FillAsync("username4");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add Barber" }).ClickAsync();

        await Expect(Page.GetByText("Barber successfully added to salon!")).ToBeVisibleAsync();
    }

    [Test, Order(7)]
    public async Task MySalon_ShouldRemoveBarberFromTeamViaModal()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByTitle("Remove from salon").ClickAsync();

        await Expect(Page.GetByText("Remove Team Member")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Remove", Exact = true }).ClickAsync();

        await Expect(Page.GetByText("Barber successfully removed from salon.")).ToBeVisibleAsync();
    }

    [Test, Order(8)]
    public async Task MySalon_ShouldDeleteSalonSuccessfully()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete Salon" }).ClickAsync();

        await Expect(Page.GetByText("Salon successfully deleted!")).ToBeVisibleAsync();
    }

    [Test, Order(9)]
    public async Task MySalon_ShouldHandleFullLifecycleAndModal_OnMobileViewport()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync($"{ApiUrl}/profile#salon");

        await Page.GetByPlaceholder("e.g., The Gentleman's Club").FillAsync("Gentleman Barber");
        await Page.GetByPlaceholder("e.g., Knez Mihailova 21").FillAsync("Knez Mihailova 10");
        await Page.GetByPlaceholder("e.g., Belgrade").FillAsync("Belgrade");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Launch Salon" }).ClickAsync();

        await Expect(Page.GetByText("Salon registered successfully!")).ToBeVisibleAsync();

        await Expect(Page.GetByPlaceholder("e.g., The Gentleman's Club")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Active Team (1)")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Add Team Member" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Edit Salon" }))
            .ToBeVisibleAsync();

        await Page.GetByPlaceholder("e.g., john.barber").FillAsync("username4");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add Barber" }).ClickAsync();
        await Expect(Page.GetByText("Barber successfully added to salon!")).ToBeVisibleAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        var logoutButton = Page.GetByRole(AriaRole.Button)
            .Filter(new() { Has = Page.Locator("svg.lucide-log-out") });

        await logoutButton.ClickAsync();

        await Expect(Page).Not.ToHaveURLAsync($"{ApiUrl}/profile");
    }
}
