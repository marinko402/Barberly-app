using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileChangePasswordTests : PageTest
{
    private const string ApiUrl = "https://localhost:5174";

    [SetUp]
    public async Task SetUp()
    {
        await Context.AddCookiesAsync(
            new[]
            {
                new Cookie
                {
                    Name = "jwt",
                    Value = "mock-jwt-token-value",
                    Url = ApiUrl,
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteAttribute.Strict,
                },
            }
        );

        var mockUser = new
        {
            id = "123",
            userName = "dusan",
            email = "dusan@gmail.com",
            firstName = "Dusan",
            lastName = "Maksimovic",
            phoneNumber = "+381601234567",
            role = "Barber",
        };

        await Page.RouteAsync(
            "**/api/**",
            async route =>
            {
                if (route.Request.Method == "GET")
                {
                    await route.FulfillAsync(
                        new RouteFulfillOptions
                        {
                            Status = 200,
                            ContentType = "application/json",
                            Body = JsonSerializer.Serialize(mockUser),
                        }
                    );
                }
                else
                {
                    await route.ContinueAsync();
                }
            }
        );
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
        await Page.RouteAsync(
            "**/api/**",
            async route =>
            {
                if (route.Request.Method == "POST")
                {
                    await route.FulfillAsync(
                        new RouteFulfillOptions
                        {
                            Status = 400,
                            ContentType = "application/json",
                            Body = JsonSerializer.Serialize(
                                new { message = "Incorrect current password!" }
                            ),
                        }
                    );
                }
                else
                {
                    await route.FallbackAsync();
                }
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#security");

        await Page.GetByPlaceholder("Enter your current password to verify identity")
            .FillAsync("WrongPass123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Verify Password" }).ClickAsync();

        await Expect(Page.GetByText("Incorrect current password!")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ChangePassword_ShouldMoveToStep2_WhenVerifyPasswordSucceeds()
    {
        await Page.RouteAsync(
            "**/api/**",
            async route =>
            {
                if (route.Request.Method == "POST")
                {
                    await route.FulfillAsync(
                        new RouteFulfillOptions
                        {
                            Status = 200,
                            ContentType = "application/json",
                            Body = JsonSerializer.Serialize(new { success = true }),
                        }
                    );
                }
                else
                {
                    await route.FallbackAsync();
                }
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#security");

        await Page.GetByPlaceholder("Enter your current password to verify identity")
            .FillAsync("OldPassword123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Verify Password" }).ClickAsync();

        await Expect(Page.GetByText("Identity verified! Enter your new password."))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("New Password", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Confirm New Password", new() { Exact = true }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task ChangePassword_ShouldShowValidationErrors_WhenNewPasswordIsWeak()
    {
        await Helper_NavigateToStep2Async();

        await Page.GetByPlaceholder("e.g., ••••••••••••").FillAsync("weak");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Update Password" }).ClickAsync();

        await Expect(Page.GetByText("Password must be at least 8 characters.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ChangePassword_ShouldShowValidationError_WhenPasswordsDoNotMatch()
    {
        await Helper_NavigateToStep2Async();

        await Page.GetByPlaceholder("e.g., ••••••••••••").FillAsync("NewPassword123!");
        await Page.GetByPlaceholder("Repeat your new password").FillAsync("DifferentPassword123!");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Update Password" }).ClickAsync();

        await Expect(Page.GetByText("Passwords do not match")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ChangePassword_ShouldShowErrorToast_WhenNewPasswordIsSameAsOldPassword()
    {
        await Helper_NavigateToStep2Async();

        await Page.GetByPlaceholder("e.g., ••••••••••••").FillAsync("OldPassword123!");
        await Page.GetByPlaceholder("Repeat your new password").FillAsync("OldPassword123!");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Update Password" }).ClickAsync();

        await Expect(Page.GetByText("New password cannot be the same as the old password!"))
            .ToBeVisibleAsync();
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
    }

    [Test]
    public async Task ChangePassword_ShouldSubmitSuccessfully_WhenDataIsValid()
    {
        await Page.RouteAsync(
            "**/api/**",
            async route =>
            {
                if (route.Request.Method == "POST" || route.Request.Method == "PUT")
                {
                    await route.FulfillAsync(
                        new RouteFulfillOptions
                        {
                            Status = 200,
                            ContentType = "application/json",
                            Body = JsonSerializer.Serialize(new { success = true }),
                        }
                    );
                }
                else
                {
                    await route.FallbackAsync();
                }
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#security");

        await Page.GetByPlaceholder("Enter your current password to verify identity")
            .FillAsync("OldPassword123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Verify Password" }).ClickAsync();

        await Page.GetByPlaceholder("e.g., ••••••••••••").FillAsync("NewPassword123!");
        await Page.GetByPlaceholder("Repeat your new password").FillAsync("NewPassword123!");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Update Password" }).ClickAsync();

        await Expect(Page.GetByText("Password changed successfully!")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Enter your current password to verify identity"))
            .ToBeVisibleAsync();
    }

    private async Task Helper_NavigateToStep2Async()
    {
        await Page.RouteAsync(
            "**/api/**",
            async route =>
            {
                if (route.Request.Method == "POST")
                {
                    await route.FulfillAsync(
                        new RouteFulfillOptions
                        {
                            Status = 200,
                            ContentType = "application/json",
                            Body = JsonSerializer.Serialize(new { success = true }),
                        }
                    );
                }
                else
                {
                    await route.FallbackAsync();
                }
            }
        );

        await Page.GotoAsync($"{ApiUrl}/profile#security");

        await Page.GetByPlaceholder("Enter your current password to verify identity")
            .FillAsync("OldPassword123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Verify Password" }).ClickAsync();

        await Page.GetByPlaceholder("e.g., ••••••••••••").WaitForAsync();
    }
}
