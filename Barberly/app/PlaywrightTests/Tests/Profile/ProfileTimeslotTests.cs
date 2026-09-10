using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[TestFixture]
public class ProfileTimeslotTests : BaseProfileTest
{
    [SetUp]
    public async Task SetUp()
    {
        await LoginAsync("user_50d04554", "Password123!");
    }

    [Test, Order(1)]
    public async Task Timeslot_ShouldDisplayFormAndScheduleListCorrectly()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Expect(Page.GetByText("Quick Slot Generator")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Create Timeslot" }))
            .ToBeVisibleAsync();

        await Expect(Page.GetByText("Active Schedule")).ToBeVisibleAsync();
        await Expect(Page.GetByText("2 Slots")).ToBeVisibleAsync();

        await Expect(Page.GetByText("09:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("30 mins")).ToBeVisibleAsync();
        await Expect(Page.Locator("span").GetByText("Open", new() { Exact = true }))
            .ToBeAttachedAsync();

        await Expect(Page.GetByText("10:00")).ToBeVisibleAsync();
        await Expect(Page.GetByText("45 mins")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Booked")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Petar Petrovic")).ToBeVisibleAsync();
    }

    [Test, Order(2)]
    public async Task Timeslot_ShouldDisplayEmptyState_WhenNoSlotsExist()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        var tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

        await Page.Locator("input[type='date']").FillAsync(tomorrow);

        await Expect(Page.GetByText("0 Slots")).ToBeVisibleAsync();
        await Expect(
                Page.GetByText(
                    "No timeslots generated for this date yet. Use the quick builder above!"
                )
            )
            .ToBeVisibleAsync();
    }

    [Test, Order(3)]
    public async Task Timeslot_ShouldCreateNewTimeslotSuccessfully()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        var timeInput = Page.Locator("input[type='time']");
        await timeInput.FillAsync("14:30");

        var durationSelect = Page.Locator("select");
        await durationSelect.SelectOptionAsync("45");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Timeslot" }).ClickAsync();

        await Expect(Page.GetByText("Timeslot successfully created!")).ToBeVisibleAsync();
    }

    [Test, Order(4)]
    public async Task Timeslot_ShouldDisplayCustomError_WhenCreateTimeslotFails()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create Timeslot" }).ClickAsync();

        await Expect(Page.GetByText("The timeslot overlaps with an existing one."))
            .ToBeVisibleAsync();
    }

    [Test, Order(5)]
    public async Task Timeslot_ShouldEditAndSaveSlotSuccessfully()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Page.GetByTitle("Edit timeslot").Nth(0).ClickAsync();

        await Expect(Page.GetByText("Modify Selected Slot")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Cancel Edit")).ToBeVisibleAsync();

        var durationSelect = Page.Locator("select");
        await durationSelect.SelectOptionAsync("45");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

        await Expect(Page.GetByText("Timeslot successfully updated!")).ToBeVisibleAsync();
    }

    [Test, Order(6)]
    public async Task Timeslot_ShouldCancelEditMode()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Page.GetByTitle("Edit timeslot").Nth(0).ClickAsync();
        await Expect(Page.GetByText("Modify Selected Slot")).ToBeVisibleAsync();

        await Page.GetByText("Cancel Edit").ClickAsync();
        await Expect(Page.GetByText("Quick Slot Generator")).ToBeVisibleAsync();
    }

    [Test, Order(7)]
    public async Task Timeslot_ShouldDeleteTimeslotSuccessfully()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Page.GetByTitle("Delete timeslot").Nth(0).ClickAsync();

        await Expect(Page.GetByText("Timeslot deleted.")).ToBeVisibleAsync();
    }

    [Test, Order(8)]
    public async Task Timeslot_ShouldCancelBookingSuccessfully()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Page.GetByTitle("Cancel booking & free up slot").ClickAsync();

        await Expect(Page.GetByText("Booking cancelled. Timeslot is now available!"))
            .ToBeVisibleAsync();
    }

    [Test, Order(9)]
    public async Task Timeslot_ShouldRenderCorrectly_OnMobileViewport()
    {
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync($"{ApiUrl}/profile#timeslots");

        await Expect(Page.GetByText("Quick Slot Generator")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Create Timeslot" }))
            .ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Combobox)).ToBeVisibleAsync();

        await Expect(Page.GetByText("Active Schedule")).ToBeVisibleAsync();
        await Expect(Page.GetByText("10:00")).ToBeVisibleAsync();

        await Expect(Page.Locator("span").GetByText("Open", new() { Exact = true }).Nth(0))
            .ToBeHiddenAsync();
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
