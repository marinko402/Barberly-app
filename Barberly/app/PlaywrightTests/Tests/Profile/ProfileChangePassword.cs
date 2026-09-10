using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileChangePasswordTests : BaseProfileTest
{
    [SetUp]
    public async Task SetUp()
    {
        await LoginAsync("username", "#Sifra123");
    }

    [Test]
    public async Task ChangePassword_ShouldShowValidationError_WhenCurrentPasswordIsEmpty()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#security");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Verify Password" }).ClickAsync();

        await Expect(Page.GetByText("Current password is required")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ChangePassword_ShouldShowErrorToast_WhenVerifyPasswordFails()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#security");

        await Page.GetByPlaceholder("Enter your current password to verify identity")
            .FillAsync("WrongPass123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Verify Password" }).ClickAsync();

        await Expect(Page.GetByText("Incorrect password")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ChangePassword_ShouldMoveToStep2_WhenVerifyPasswordSucceeds()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#security");

        await Page.GetByPlaceholder("Enter your current password to verify identity")
            .FillAsync("#Sifra123");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Verify Password" }).ClickAsync();

        await Expect(Page.GetByText("Identity verified! Enter your new password."))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("New Password", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Confirm New Password", new() { Exact = true }))
            .ToBeVisibleAsync();

        await Page.GotoAsync($"{ApiUrl}/profile");
    }

    [Test]
    public async Task ChangePassword_ShouldShowValidationErrors_WhenNewPasswordIsWeak()
    {
        await Helper_NavigateToStep2Async();

        await Page.GetByPlaceholder("e.g., ••••••••••••").FillAsync("weak");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Update Password" }).ClickAsync();

        await Expect(Page.GetByText("Password must be at least 8 characters.")).ToBeVisibleAsync();

        await Page.GotoAsync($"{ApiUrl}/profile");
    }

    [Test]
    public async Task ChangePassword_ShouldShowValidationError_WhenPasswordsDoNotMatch()
    {
        await Helper_NavigateToStep2Async();

        await Page.GetByPlaceholder("e.g., ••••••••••••").FillAsync("NewPassword123!");
        await Page.GetByPlaceholder("Repeat your new password").FillAsync("DifferentPassword123!");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Update Password" }).ClickAsync();

        await Expect(Page.GetByText("Passwords do not match")).ToBeVisibleAsync();

        await Page.GotoAsync($"{ApiUrl}/profile");
    }

    [Test]
    public async Task ChangePassword_ShouldShowErrorToast_WhenNewPasswordIsSameAsOldPassword()
    {
        await Helper_NavigateToStep2Async();

        await Page.GetByPlaceholder("e.g., ••••••••••••").FillAsync("#Sifra123");
        await Page.GetByPlaceholder("Repeat your new password").FillAsync("#Sifra123");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Update Password" }).ClickAsync();

        await Expect(Page.GetByText("New password cannot be the same as the old password!"))
            .ToBeVisibleAsync();

        await Page.GotoAsync($"{ApiUrl}/profile");
    }

    [Test]
    public async Task ChangePassword_ShouldResetToStep1_WhenCancelClickedInStep2()
    {
        await Helper_NavigateToStep2Async();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();

        await Expect(Page.GetByPlaceholder("Enter your current password to verify identity"))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Verify Password" }))
            .ToBeVisibleAsync();

        await Page.GotoAsync($"{ApiUrl}/profile");
    }

    [Test]
    public async Task ChangePassword_ShouldSubmitSuccessfully_WhenDataIsValid()
    {
        await Helper_NavigateToStep2Async();

        await Page.GetByPlaceholder("e.g., ••••••••••••").FillAsync("NewPassword123!");
        await Page.GetByPlaceholder("Repeat your new password").FillAsync("NewPassword123!");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Update Password" }).ClickAsync();

        await Expect(Page.GetByText("Password changed successfully!")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Enter your current password to verify identity"))
            .ToBeVisibleAsync();

        await Page.GotoAsync($"{ApiUrl}/profile#security");

        await Page.GetByPlaceholder("Enter your current password to verify identity")
            .FillAsync("NewPassword123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Verify Password" }).ClickAsync();

        await Page.GetByPlaceholder("e.g., ••••••••••••").WaitForAsync();
        await Page.GetByPlaceholder("e.g., ••••••••••••").FillAsync("#Sifra123");
        await Page.GetByPlaceholder("Repeat your new password").FillAsync("#Sifra123");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Update Password" }).ClickAsync();

        await Expect(Page.GetByText("Password changed successfully!")).ToBeVisibleAsync();

        await Page.GotoAsync($"{ApiUrl}/profile");
    }

    private async Task Helper_NavigateToStep2Async()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#security");

        await Page.GetByPlaceholder("Enter your current password to verify identity")
            .FillAsync("#Sifra123");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Verify Password" }).ClickAsync();

        await Page.GetByPlaceholder("e.g., ••••••••••••").WaitForAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await LogoutAsync();
    }
}
