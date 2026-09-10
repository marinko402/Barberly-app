using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileContentTests : BaseProfileTest
{
    [Test]
    public async Task ProfileContent_ShouldRenderHeaderWithUserGreetingAndRole()
    {
        await LoginAsync("username", "#Sifra123");
        await Page.GotoAsync($"{ApiUrl}/profile");

        await Expect(Page.GetByText("Welcome back, Name")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Barber", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Here's what's happening with your shop today."))
            .ToBeVisibleAsync();

        await LogoutAsync();
    }

    [Test]
    public async Task ProfileContent_ShouldShowInvalidSectionError_WhenSectionDoesNotExist()
    {
        await LoginAsync("username", "#Sifra123");
        await Page.GotoAsync($"{ApiUrl}/profile#invalid-section");

        await Expect(Page.GetByText("Invalid section").First).ToBeVisibleAsync();

        await LogoutAsync();
    }

    [Test]
    public async Task ProfileContent_ShouldShowLockedState_WhenBarberHasNoSalon_OnTimeslots()
    {
        await LoginAsync("user_50d04554", "Password123!");

        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Expect(Page.GetByText("Feature Temporarily Locked")).ToBeVisibleAsync();
        await Expect(
                Page.GetByText(
                    "You cannot manage your timeslots until you are actively registered under or owning a salon workspace."
                )
            )
            .ToBeVisibleAsync();

        var setupButton = Page.GetByRole(AriaRole.Link, new() { Name = "Set up My Salon" });
        await Expect(setupButton).ToBeVisibleAsync();

        await LogoutAsync();
    }

    [Test]
    public async Task ProfileContent_ShouldShowLockedState_WhenBarberHasNoSalon_OnBookings()
    {
        await LoginAsync("user_50d04554", "Password123!");

        await Page.GotoAsync($"{ApiUrl}/profile#bookings");

        await Expect(Page.GetByText("Feature Temporarily Locked")).ToBeVisibleAsync();
        await Expect(
                Page.GetByText(
                    "You cannot manage your bookings until you are actively registered under or owning a salon workspace."
                )
            )
            .ToBeVisibleAsync();

        await LogoutAsync();
    }

    [Test]
    public async Task ProfileContent_ShouldNavigateToSalonTab_WhenClickingSetUpSalonButton()
    {
        await LoginAsync("user_50d04554", "Password123!");
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Set up My Salon" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{ApiUrl}/profile#salon");

        await LogoutAsync();
    }
}
