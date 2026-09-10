using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileInfoTests : BaseProfileTest
{
    [Test, Order(1)]
    public async Task ProfileInfo_ShouldRenderDisabledInputs_Initially()
    {
        await LoginAsync("username", "#Sifra123");

        await Page.GotoAsync($"{ApiUrl}/profile#info");

        var nameInput = Page.Locator("input[name='name']");
        var emailInput = Page.Locator("input[name='email']");
        var editButton = Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" });

        await Expect(nameInput).ToHaveValueAsync("Name");
        await Expect(nameInput).ToBeDisabledAsync();

        await Expect(emailInput).ToHaveValueAsync("username@email.com");
        await Expect(emailInput).ToBeDisabledAsync();

        await Expect(editButton).ToBeVisibleAsync();

        await LogoutAsync();
    }

    [Test, Order(2)]
    public async Task ProfileInfo_ShouldEnableInputs_WhenEditButtonClicked()
    {
        await LoginAsync("username", "#Sifra123");

        await Page.GotoAsync($"{ApiUrl}/profile#info");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }).ClickAsync();

        var nameInput = Page.Locator("input[name='name']");
        var saveButton = Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" });
        var cancelButton = Page.GetByRole(AriaRole.Button, new() { Name = "Cancel" });

        await Expect(nameInput).ToBeEnabledAsync();
        await Expect(saveButton).ToBeVisibleAsync();
        await Expect(cancelButton).ToBeVisibleAsync();

        await LogoutAsync();
    }

    [Test, Order(3)]
    public async Task ProfileInfo_ShouldShowValidationErrors_WhenInputsAreInvalid()
    {
        await LoginAsync("username", "#Sifra123");

        await Page.GotoAsync($"{ApiUrl}/profile#info");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }).ClickAsync();

        await Page.Locator("input[name='email']").FillAsync("invalid-email");
        await Page.Locator("input[name='username']").FillAsync("ab");
        await Page.Locator("input[name='phoneNumber']").FillAsync("12345");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

        await Expect(Page.GetByText("Invalid email")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Username must be at least 3 characters")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Invalid phone number (format: +381...)")).ToBeVisibleAsync();

        await LogoutAsync();
    }

    [Test, Order(4)]
    public async Task ProfileInfo_ShouldResetFormAndDisableInputs_WhenCancelClicked()
    {
        await LoginAsync("username", "#Sifra123");

        await Page.GotoAsync($"{ApiUrl}/profile#info");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }).ClickAsync();

        var firstNameInput = Page.Locator("input[name='name']");
        await firstNameInput.FillAsync("NameUpdated");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();

        await Expect(firstNameInput).ToHaveValueAsync("Name");
        await Expect(firstNameInput).ToBeDisabledAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }))
            .ToBeVisibleAsync();

        await LogoutAsync();
    }

    [Test, Order(5)]
    public async Task ProfileInfo_ShouldSubmitFormSuccessfully_WhenDataIsValid()
    {
        await LoginAsync("username", "#Sifra123");

        await Page.GotoAsync($"{ApiUrl}/profile#info");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }).ClickAsync();

        await Page.Locator("input[name='name']").FillAsync("NameUpdated");
        await Page.Locator("input[name='username']").FillAsync("username_updated");
        await Page.Locator("input[name='email']").FillAsync("username_updated@gmail.com");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

        await Expect(Page.GetByText("Profile updated successfully!")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }))
            .ToBeVisibleAsync();
        await Expect(Page.Locator("input[name='name']")).ToBeDisabledAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }).ClickAsync();

        await Page.Locator("input[name='name']").FillAsync("Name");
        await Page.Locator("input[name='username']").FillAsync("username");
        await Page.Locator("input[name='email']").FillAsync("username@email.com");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

        await Expect(Page.GetByText("Profile updated successfully!").Nth(1)).ToBeVisibleAsync();

        await LogoutAsync();
    }

    [Test, Order(6)]
    public async Task ProfileInfo_ShouldShowErrorToast_WhenApiFails()
    {
        await LoginAsync("username", "#Sifra123");

        await Page.GotoAsync($"{ApiUrl}/profile#info");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }).ClickAsync();

        await Page.Locator("input[name='username']").FillAsync("username1");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

        await Expect(Page.GetByText("Username already exists")).ToBeVisibleAsync();

        await LogoutAsync();
    }
}
