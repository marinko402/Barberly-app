using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests;

public class BaseProfileTest : PageTest
{
    protected const string ApiUrl = "https://localhost:5174";

    protected async Task LoginAsync(string username, string password)
    {
        await Page.GotoAsync($"{ApiUrl}/login");

        await Page.GetByPlaceholder("Enter your username").FillAsync(username);
        await Page.GetByPlaceholder("••••••••").FillAsync(password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{ApiUrl}/profile");
    }

    protected async Task LogoutAsync()
    {
        await Page.GotoAsync($"{ApiUrl}/profile");

        var sidebar = Page.Locator("aside");
        await sidebar.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

        await Expect(Page).Not.ToHaveURLAsync($"{ApiUrl}/profile");
    }
}
