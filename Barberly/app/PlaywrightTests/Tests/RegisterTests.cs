using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests.Tests;

[TestFixture]
public class RegisterTests : PageTest
{
    private const string BaseUrl = "https://localhost:5174";

    [SetUp]
    public async Task Setup()
    {
        await Page.RouteAsync(
            "**/api/Auth/Me",
            async route =>
            {
                await route.FulfillAsync(new RouteFulfillOptions { Status = 401 });
            }
        );

        await Page.GotoAsync($"{BaseUrl}/register");
    }


    [Test]
    public async Task Register_PageRender_ShouldDisplayAllFormFieldsAndButtons()
    {
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Barberly" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Grow Your Barber Shop" }))
            .ToBeVisibleAsync();

        await Expect(Page.GetByPlaceholder("First name")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Last name")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Username")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Email address")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Phone number")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Password", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("Confirm password")).ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Register" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Login here" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Go home" }))
            .ToBeVisibleAsync();
    }


    [Test]
    public async Task Register_EmptySubmit_ShouldShowAllRequiredValidationErrors()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

        await Expect(Page.GetByText("First name is required.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Last name is required.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Username must be at least 3 characters.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Invalid email address.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Invalid phone number.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Birth date is required.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Password must be at least 8 characters.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_InvalidPasswordComplexity_ShouldShowRegexValidationErrors()
    {
        var passwordInput = Page.GetByPlaceholder("Password", new() { Exact = true });

        await passwordInput.FillAsync("abc");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByText("Password must be at least 8 characters.")).ToBeVisibleAsync();

        await passwordInput.FillAsync("password");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByText("Must include at least one uppercase letter."))
            .ToBeVisibleAsync();

        await passwordInput.FillAsync("Password");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByText("Must include at least one number.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_PasswordMismatch_ShouldShowConfirmPasswordError()
    {
        await Page.GetByPlaceholder("Password", new() { Exact = true }).FillAsync("Password123!");
        await Page.GetByPlaceholder("Confirm password").FillAsync("DifferentPassword123!");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

        await Expect(Page.GetByText("Passwords do not match.")).ToBeVisibleAsync();
    }


    [Test]
    public async Task Register_TogglePasswordVisibility_ShouldSwitchInputType()
    {
        var passwordInput = Page.GetByPlaceholder("Password", new() { Exact = true });
        await Expect(passwordInput).ToHaveAttributeAsync("type", "password");

        var toggleButton = Page.Locator("form button[type='button']").First;
        await toggleButton.ClickAsync();

        await Expect(passwordInput).ToHaveAttributeAsync("type", "text");
    }


    [Test]
    public async Task Register_SuccessfulAuth_ShouldShowSuccessToastAndRedirectToLogin()
    {
        await Page.RouteAsync(
            "**/api/Auth/Register",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = "{\"message\": \"User registered successfully\"}",
                    }
                );
            }
        );

        await FillValidRegistrationForm();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

        await Expect(Page.GetByText("Registration successful!")).ToBeVisibleAsync();
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
    }

    [Test]
    public async Task Register_FailedApiCall_ShouldShowErrorToast()
    {
        await Page.RouteAsync(
            "**/api/Auth/Register",
            async route =>
            {
                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 400,
                        ContentType = "application/json",
                        Body = "{\"message\": \"Username already exists\"}",
                    }
                );
            }
        );

        await FillValidRegistrationForm();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

        await Expect(Page.GetByText("Registration failed!")).ToBeVisibleAsync();
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/register");
    }

    [Test]
    public async Task Register_ClickLoginHere_ShouldNavigateToLoginPage()
    {
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login here" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
    }

    [Test]
    public async Task Register_ClickGoHome_ShouldNavigateToHomePage()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Go home" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/");
    }

    private async Task FillValidRegistrationForm()
    {
        await Page.GetByPlaceholder("First name").FillAsync("Dusan");
        await Page.GetByPlaceholder("Last name").FillAsync("Maksimovic");
        await Page.GetByPlaceholder("Username").FillAsync("dusan123");
        await Page.GetByPlaceholder("Email address").FillAsync("dusan@example.com");
        await Page.GetByPlaceholder("Phone number").FillAsync("+381612345678");

        var dateInput = Page.Locator("input[type='date']");
        await dateInput.FillAsync("2000-01-01");

        await Page.GetByPlaceholder("Password", new() { Exact = true }).FillAsync("Password123!");
        await Page.GetByPlaceholder("Confirm password").FillAsync("Password123!");
    }

    [Test]
    public async Task Register_MobileLayout_ShouldAdaptViewAndHideDesktopImage()
    {
        await Page.SetViewportSizeAsync(375, 812);

        var formContainer = Page.Locator("form");
        await Expect(formContainer).ToBeVisibleAsync();

        var desktopImageContainer = Page.Locator("div.hidden.md\\:block");
        await Expect(desktopImageContainer).Not.ToBeVisibleAsync();

        var firstNameInput = Page.GetByPlaceholder("First name");
        var lastNameInput = Page.GetByPlaceholder("Last name");

        await Expect(firstNameInput).ToBeVisibleAsync();
        await Expect(lastNameInput).ToBeVisibleAsync();

        var goHomeButton = Page.GetByRole(AriaRole.Button, new() { Name = "Go home" });
        await Expect(goHomeButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_DesktopLayout_ShouldShowSideBySideLayoutAndImage()
    {
        await Page.SetViewportSizeAsync(1280, 800);

        var desktopImage = Page.GetByAltText("barber chair");
        await Expect(desktopImage).ToBeVisibleAsync();

        var desktopLogo = Page.GetByAltText("barberly logo");
        await Expect(desktopLogo).ToBeVisibleAsync();

        var cardContainer = Page.Locator("div.relative.w-full.max-w-4xl");
        await Expect(cardContainer).ToBeVisibleAsync();
    }
}
