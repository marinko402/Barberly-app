using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests;

[TestFixture]
public class SalonTests : PageTest
{
    private const string ApiUrl = "https://localhost:5174";

    [SetUp]
    public async Task SetUp() { }

    private async Task NavigateToSalonPageAsync()
    {
        await Page.GotoAsync($"{ApiUrl}/barbers");
        var card = Page.Locator("div.group").Filter(new() { HasText = "Antic Group" });

        await card.GetByRole(AriaRole.Button, new() { Name = "View Salon" }).ClickAsync();
    }

    [Test]
    public async Task Salon_ShouldDisplayLoadingState_WhenSalonDataIsFetching()
    {
        await Page.GotoAsync($"{ApiUrl}/barbers");
        var card = Page.Locator("div.group").Filter(new() { HasText = "Antic Group" });

        await card.GetByRole(AriaRole.Button, new() { Name = "View Salon" }).ClickAsync();
        await Expect(Page.GetByText("Loading salon experience...")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldRenderSalonInfoAndBarbersList()
    {
        await NavigateToSalonPageAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Antic Group" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Dusanova 90")).ToBeVisibleAsync();

        await Expect(Page.GetByText("Ime Prezime")).ToBeVisibleAsync();
        await Expect(Page.GetByText("@username1")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Name Surname")).ToBeVisibleAsync();
        await Expect(Page.GetByText("@username", new() { Exact = true })).ToBeVisibleAsync();

        await Expect(Page.GetByText("Please select a barber from the left side"))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldDisplayBarberSlots_WhenBarberIsSelected()
    {
        await NavigateToSalonPageAsync();

        await Page.GetByText("Ime Prezime").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Ime's Schedule" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("09:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("10:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("11:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("12:00")).ToBeHiddenAsync();
    }

    [Test]
    public async Task Salon_ShouldDisplayEmptyState_WhenNoSlotsAvailable()
    {
        await NavigateToSalonPageAsync();
        await Page.GetByText("Name Surname").ClickAsync();

        await Expect(Page.GetByText("All slots are taken or none are created for this day."))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldAllowSelectingSlot_AndFillingBookingForm()
    {
        await NavigateToSalonPageAsync();
        await Page.GetByText("Ime Prezime").ClickAsync();

        await Page.GetByText("10:00").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Complete Booking" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("10:00 (30 mins)")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "First Name" }).FillAsync("Petar");

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Last Name" }).FillAsync("Petrovic");

        await Page.GetByPlaceholder("email@example.com").FillAsync("petar@gmail.com");
        await Page.GetByPlaceholder("+381 6X XXX XXXX").FillAsync("+381641234567");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Back to Slots" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Ime's Schedule" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldSubmitBookingSuccessfully()
    {
        await NavigateToSalonPageAsync();
        await Page.GetByText("Ime Prezime").ClickAsync();
        await Page.GetByText("10:00").ClickAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "First Name" }).FillAsync("Petar");

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Last Name" }).FillAsync("Petrovic");
        await Page.GetByPlaceholder("email@example.com").FillAsync("petar@gmail.com");
        await Page.GetByPlaceholder("+381 6X XXX XXXX").FillAsync("+381641234567");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm Appointment" }).ClickAsync();

        await Expect(Page.GetByText("Appointment successfully booked!")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldRenderCorrectly_OnMobileViewport()
    {
        await Page.SetViewportSizeAsync(390, 844);

        await NavigateToSalonPageAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Antic Group" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Ime Prezime")).ToBeVisibleAsync();

        await Page.GetByText("Ime Prezime").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Ime's Schedule" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("09:00")).ToBeVisibleAsync();

        await Page.GetByText("09:00").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Confirm Appointment" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldFetchNewSlots_WhenDateIsChanged()
    {
        await NavigateToSalonPageAsync();

        await Page.GetByText("Ime Prezime").ClickAsync();

        var tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

        await Page.Locator("input[type='date']").FillAsync(tomorrow);

        await Expect(Page.GetByText("All slots are taken or none are created for this day."))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldResetSelectedSlot_WhenDifferentBarberIsSelected()
    {
        await NavigateToSalonPageAsync();

        await Page.GetByText("Ime Prezime").ClickAsync();
        await Page.GetByText("13:00").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Complete Booking" }))
            .ToBeVisibleAsync();

        await Page.GetByText("Name Surname").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Name's Schedule" }))
            .ToBeVisibleAsync();

        await Expect(Page.GetByText("All slots are taken or none are created for this day."))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Salon_ShouldPreventSubmit_WhenRequiredFieldsAreEmpty()
    {
        await NavigateToSalonPageAsync();
        await Page.GetByText("Ime Prezime").ClickAsync();
        await Page.GetByText("13:00").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm Appointment" }).ClickAsync();

        var firstNameInput = Page.GetByRole(AriaRole.Textbox, new() { Name = "First Name" });
        await Expect(firstNameInput).ToHaveJSPropertyAsync("validity.valid", false);

        var validationMessage = await firstNameInput.EvaluateAsync<string>(
            "el => el.validationMessage"
        );
        Assert.That(validationMessage, Is.Not.Empty);
    }
}
