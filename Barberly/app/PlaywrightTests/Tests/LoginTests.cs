using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests;

[TestFixture]
public class LoginTests : PageTest
{
    private const string APIUrl = "https://localhost:5174";

    [SetUp]
    public async Task SetUp()
    {
        await Page.GotoAsync($"{APIUrl}/login");
    }

    [Test]
    public async Task Login_ShouldRenderAllCoreElements()
    {
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Barberly" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Welcome back!" }))
            .ToBeVisibleAsync();

        await Expect(Page.GetByPlaceholder("Enter your username")).ToBeVisibleAsync();
        await Expect(Page.GetByPlaceholder("••••••••")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Login" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Go home" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Register here" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Forgot password?" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_EmptySubmit_ShouldShowValidationErrors()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        await Expect(Page.GetByText("Username is required.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Password must be at least 8 characters.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_WeakPassword_ShouldShowRegexValidationError()
    {
        await Page.GetByPlaceholder("Enter your username").FillAsync("dusan123");
        await Page.GetByPlaceholder("••••••••").FillAsync("weakpass");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        await Expect(Page.GetByText("Password must include at least one uppercase letter."))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_TogglePasswordVisibility_ShouldChangeInputType()
    {
        var passwordInput = Page.GetByPlaceholder("••••••••");

        await Expect(passwordInput).ToHaveAttributeAsync("type", "password");

        var toggleButton = Page.Locator("button[type='button']");
        await toggleButton.ClickAsync();

        await Expect(passwordInput).ToHaveAttributeAsync("type", "text");

        await toggleButton.ClickAsync();
        await Expect(passwordInput).ToHaveAttributeAsync("type", "password");
    }

    [Test]
    public async Task Login_ClickGoHome_ShouldNavigateToHomePage()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Go home" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{APIUrl}/");
    }

    [Test]
    public async Task Login_ClickRegisterHere_ShouldNavigateToRegisterPage()
    {
        await Page.GetByRole(AriaRole.Link, new() { Name = "Register here" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{APIUrl}/register");
    }

    [Test]
    public async Task Login_SuccessfulAuth_ShouldRedirectToProfile()
    {
        await Page.RouteAsync(
            "**/api/**",
            async route =>
            {
                var url = route.Request.Url;
                var method = route.Request.Method;

                if (url.Contains("/api/Auth/Login") && method == "POST")
                {
                    await route.FulfillAsync(
                        new RouteFulfillOptions
                        {
                            Status = 200,
                            ContentType = "application/json",
                            Body = "{\"userId\": \"user-123\", \"roles\": [\"Barber\"]}",
                        }
                    );
                    return;
                }

                if (
                    url.Contains("/api/Auth/Me")
                    || url.Contains("/api/Auth/User")
                    || url.Contains("/api/User")
                )
                {
                    await route.FulfillAsync(
                        new RouteFulfillOptions
                        {
                            Status = 200,
                            ContentType = "application/json",
                            Body =
                                "{\"id\": \"user-123\", \"userName\": \"dusan123\", \"email\": \"dusan@test.com\", \"firstName\": \"Dusan\", \"lastName\": \"Maksimovic\", \"phoneNumber\": \"123456\", \"birthDate\": \"2000-01-01\", \"salonId\": \"salon-1\", \"role\": \"Barber\"}",
                        }
                    );
                    return;
                }

                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = "[]",
                    }
                );
            }
        );

        await Page.GetByPlaceholder("Enter your username").FillAsync("dusan123");
        await Page.GetByPlaceholder("••••••••").FillAsync("Password123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{APIUrl}/profile");
    }

    [Test]
    public async Task Login_FailedAuth_ShouldShowToastError()
    {
        await Page.RouteAsync(
            "**/api/Auth/Login",
            async route =>
            {
                await route.FulfillAsync(new RouteFulfillOptions { Status = 401 });
            }
        );

        await Page.GetByPlaceholder("Enter your username").FillAsync("dusan123");
        await Page.GetByPlaceholder("••••••••").FillAsync("WrongPassword123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        await Expect(Page.GetByText("Error while logging!")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_MobileViewport_ShouldHideSideImage()
    {
        await Page.SetViewportSizeAsync(375, 667);

        var sideImage = Page.GetByAltText("barber chair");
        await Expect(sideImage).Not.ToBeVisibleAsync();

        await Expect(Page.GetByPlaceholder("Enter your username")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_DesktopViewport_ShouldDisplaySideImage()
    {
        await Page.SetViewportSizeAsync(1280, 800);

        var sideImage = Page.GetByAltText("barber chair");
        await Expect(sideImage).ToBeVisibleAsync();
    }
}
