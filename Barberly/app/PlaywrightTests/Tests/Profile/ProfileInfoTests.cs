using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests.Profile;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProfileInfoTests : PageTest
{
    private const string ApiUrl = "https://localhost:5174";

    [SetUp]
    public async Task SetUp()
    {
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

        var mockUser = new
        {
            id = "123",
            userName = "dusan",
            email = "dusan@gmail.com",
            firstName = "Dusan",
            lastName = "Maksimovic",
            phoneNumber = "+381601234567",
            birthDate = "2000-01-01",
            salonId = "salon-1",
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
    public async Task ProfileInfo_ShouldRenderDisabledInputs_Initially()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#info");

        var nameInput = Page.Locator("input[name='name']");
        var emailInput = Page.Locator("input[name='email']");
        var editButton = Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" });

        await Expect(nameInput).ToHaveValueAsync("Dusan");
        await Expect(nameInput).ToBeDisabledAsync();

        await Expect(emailInput).ToHaveValueAsync("dusan@gmail.com");
        await Expect(emailInput).ToBeDisabledAsync();

        await Expect(editButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task ProfileInfo_ShouldEnableInputs_WhenEditButtonClicked()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#info");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }).ClickAsync();

        var nameInput = Page.Locator("input[name='name']");
        var saveButton = Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" });
        var cancelButton = Page.GetByRole(AriaRole.Button, new() { Name = "Cancel" });

        await Expect(nameInput).ToBeEnabledAsync();
        await Expect(saveButton).ToBeVisibleAsync();
        await Expect(cancelButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task ProfileInfo_ShouldShowValidationErrors_WhenInputsAreInvalid()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#info");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }).ClickAsync();

        await Page.Locator("input[name='email']").FillAsync("invalid-email");
        await Page.Locator("input[name='username']").FillAsync("ab");
        await Page.Locator("input[name='phoneNumber']").FillAsync("12345");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

        await Expect(Page.GetByText("Invalid email")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Username must be at least 3 characters")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Invalid phone number (format: +381...)")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ProfileInfo_ShouldResetFormAndDisableInputs_WhenCancelClicked()
    {
        await Page.GotoAsync($"{ApiUrl}/profile#info");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }).ClickAsync();

        var firstNameInput = Page.Locator("input[name='name']");
        await firstNameInput.FillAsync("NovoIme");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();

        await Expect(firstNameInput).ToHaveValueAsync("Dusan");
        await Expect(firstNameInput).ToBeDisabledAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task ProfileInfo_ShouldSubmitFormSuccessfully_WhenDataIsValid()
    {
        await Page.RouteAsync(
            "**/api/**",
            async route =>
            {
                if (route.Request.Method == "PUT")
                {
                    await route.FulfillAsync(
                        new RouteFulfillOptions
                        {
                            Status = 200,
                            ContentType = "application/json",
                            Body = JsonSerializer.Serialize(
                                new
                                {
                                    id = "123",
                                    userName = "dusan_updated",
                                    email = "dusan_updated@gmail.com",
                                    firstName = "DusanUpdated",
                                    lastName = "Maksimovic",
                                    phoneNumber = "+381601234567",
                                    birthDate = "2000-01-01",
                                    salonId = "salon-1",
                                    role = "Barber",
                                }
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

        await Page.GotoAsync($"{ApiUrl}/profile#info");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }).ClickAsync();

        await Page.Locator("input[name='name']").FillAsync("DusanUpdated");
        await Page.Locator("input[name='username']").FillAsync("dusan_updated");
        await Page.Locator("input[name='email']").FillAsync("dusan_updated@gmail.com");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

        await Expect(Page.GetByText("Profile updated successfully!")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }))
            .ToBeVisibleAsync();
        await Expect(Page.Locator("input[name='name']")).ToBeDisabledAsync();
    }

    [Test]
    public async Task ProfileInfo_ShouldShowErrorToast_WhenApiFails()
    {
        await Page.RouteAsync(
            "**/api/**",
            async route =>
            {
                if (route.Request.Method == "PUT")
                {
                    await route.FulfillAsync(
                        new RouteFulfillOptions
                        {
                            Status = 400,
                            ContentType = "application/json",
                            Body = JsonSerializer.Serialize(
                                new { message = "Username already taken!" }
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

        await Page.GotoAsync($"{ApiUrl}/profile#info");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile" }).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

        await Expect(Page.GetByText("Username already taken!")).ToBeVisibleAsync();
    }
}
