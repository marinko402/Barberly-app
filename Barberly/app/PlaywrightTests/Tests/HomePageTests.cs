using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class HomePageTest : PageTest
{
    private const string api = "https://localhost:5174";

    [SetUp]
    public async Task SetUp()
    {
        await Page.RouteAsync(
            "**/Me",
            async route =>
            {
                await route.FulfillAsync(
                    new()
                    {
                        Status = 401,
                        ContentType = "application/json",
                        Body = "{\"message\": \"Unauthenticated\"}",
                    }
                );
            }
        );

        await Page.RouteAsync(
            "**/Salon/GetSalonsCount",
            async route =>
            {
                await route.FulfillAsync(
                    new()
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = "15",
                    }
                );
            }
        );

        await Page.RouteAsync(
            "**/Booking/GetTotalBookingsCount",
            async route =>
            {
                await route.FulfillAsync(
                    new()
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = "1250",
                    }
                );
            }
        );

        await Page.RouteAsync(
            "**/Salon/GetTopSalons",
            async route =>
            {
                var topSalons = new[]
                {
                    new
                    {
                        salonId = 1,
                        name = "Barber Shop Elite",
                        address = "Bulevar Nemanjica 10",
                        city = "Nis",
                        staffCount = 4,
                        totalBookings = 450,
                    },
                    new
                    {
                        salonId = 2,
                        name = "Gentleman Cut",
                        address = "Vozda Karadjordja 5",
                        city = "Nis",
                        staffCount = 3,
                        totalBookings = 320,
                    },
                    new
                    {
                        salonId = 3,
                        name = "Classic Barbers",
                        address = "Pobedina 12",
                        city = "Nis",
                        staffCount = 2,
                        totalBookings = 180,
                    },
                };

                await route.FulfillAsync(
                    new()
                    {
                        Status = 200,
                        ContentType = "application/json",
                        Body = JsonSerializer.Serialize(topSalons),
                    }
                );
            }
        );

        await Page.GotoAsync($"{api}/");
    }

    [Test]
    public async Task ShouldLoadPageAndRenderHomeContainer()
    {
        await Expect(Page.GetByText("FIND AND BOOK THE PERFECT CUT")).ToBeVisibleAsync();

        await Expect(Page.GetByText("Find Master Barbers")).ToBeVisibleAsync();

        await Expect(Page.GetByText("About Barberly")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ShouldAutomaticallyScrollToAboutUsWhenHashPresent()
    {
        await Page.GotoAsync($"{api}/#about-us");

        var aboutSection = Page.Locator("#about-us");
        await Expect(aboutSection).ToBeVisibleAsync();
    }

    [Test]
    public async Task ShouldLoadGracefullyWhenHashDoesNotMatch()
    {
        await Page.GotoAsync($"{api}/#non-existent-section");

        await Expect(Page.GetByText("FIND AND BOOK THE PERFECT CUT")).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomeStart_ShouldRenderHeroHeadingsAndSubtitles()
    {
        await Expect(
                Page.GetByRole(AriaRole.Heading, new() { Name = "FIND AND BOOK THE PERFECT CUT" })
            )
            .ToBeVisibleAsync();

        await Expect(Page.GetByText("Search. Book. Look Good.")).ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "GROW YOUR BARBER SHOP" }))
            .ToBeVisibleAsync();

        await Expect(Page.GetByText("Showcase your skill. Manage bookings.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomeStart_ShouldRenderLogoImageWithCorrectAltText()
    {
        var logo = Page.GetByRole(AriaRole.Img, new() { Name = "Barberly Logo" });
        await Expect(logo).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomeStart_ShouldRenderDescriptionParagraph()
    {
        await Expect(Page.GetByText("Barberly connects clients with top barber shops"))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task HomeStart_ShouldNavigateToBarbersWhenClickingFindBarber()
    {
        var findBarberButton = Page.GetByRole(AriaRole.Button, new() { Name = "Find Barber" });
        await Expect(findBarberButton).ToBeVisibleAsync();
        await findBarberButton.ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{api}/barbers");
    }

    [Test]
    public async Task HomeStart_ShouldNavigateToRegisterWhenClickingJoinAsBarber()
    {
        var joinBarberButton = Page.GetByRole(AriaRole.Button, new() { Name = "Join as Barber" });
        await Expect(joinBarberButton).ToBeVisibleAsync();
        await joinBarberButton.ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{api}/register");
    }

    [Test]
    public async Task HomeStartAndAboutUs_MobileView_ShouldHideDesktopSpecificElements()
    {
        await Page.SetViewportSizeAsync(375, 667);

        var centerBadge = Page.Locator("#about-us .max-sm\\:hidden");
        await Expect(centerBadge).ToBeHiddenAsync();

        var mobileHashtag = Page.GetByText("#LookGoodFeelFresh").Last;
        await Expect(mobileHashtag).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomeFindBarber_ShouldRenderHeadingAndCounters()
    {
        await Expect(
                Page.GetByRole(AriaRole.Heading, new() { Name = "Find Master Barbers Near You" })
            )
            .ToBeVisibleAsync();

        await Expect(
                Page.GetByText(
                    "Discover local top-rated shops, compare pricing, and schedule your next fresh cut instantly."
                )
            )
            .ToBeVisibleAsync();

        await Expect(Page.GetByText("15+")).ToBeVisibleAsync();
        await Expect(Page.GetByText("1,250+")).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomeFindBarber_ShouldRenderTopSalonsCards()
    {
        await Expect(Page.GetByText("Barber Shop Elite")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Gentleman Cut")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Classic Barbers")).ToBeVisibleAsync();

        await Expect(Page.GetByText("Bulevar Nemanjica 10, Nis")).ToBeVisibleAsync();
        await Expect(Page.GetByText("4 Staff")).ToBeVisibleAsync();
        await Expect(Page.GetByText("450 Bookings")).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomeFindBarber_ClickBrowseAllShopsShouldNavigateToBarbersPage()
    {
        var browseButton = Page.GetByRole(
            AriaRole.Button,
            new() { Name = "Browse All Available Shops" }
        );
        await Expect(browseButton).ToBeVisibleAsync();

        await browseButton.ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{api}/barbers");
    }

    [Test]
    public async Task HomeFindBarber_SliderKeyboardNavigationShouldChangeActiveCard()
    {
        var sliderContainer = Page.Locator(".slider-container");
        await sliderContainer.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowRight");

        var secondCard = Page.Locator(".barber-card").Nth(1);
        await Expect(secondCard)
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("scale-100"));
    }

    [Test]
    public async Task HomeFindBarber_ClickingPaginationDot_ShouldChangeActiveCard()
    {
        var dots = Page.Locator("button.rounded-full");

        await dots.Nth(1).ClickAsync();

        var secondCard = Page.Locator(".barber-card").Nth(1);
        await Expect(secondCard)
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("scale-100"));
    }

    [Test]
    public async Task HomeFindBarber_KeyboardArrowLeft_ShouldNavigateToLastCard()
    {
        var sliderContainer = Page.Locator(".slider-container");
        await sliderContainer.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowLeft");

        var lastCard = Page.Locator(".barber-card").Nth(2);
        await Expect(lastCard)
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("scale-100"));
    }

    [Test]
    public async Task AboutUs_ShouldRenderHeadingsAndBadge()
    {
        await Expect(Page.GetByText("Who We Are")).ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "About Barberly" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task AboutUs_ShouldRenderDescriptionParagraphs()
    {
        await Expect(Page.GetByText("We believe in the power of a great cut")).ToBeVisibleAsync();
        await Expect(Page.GetByText("We carefully curate and gather the best local shops"))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task AboutUs_ShouldRenderStatCountersAndHashtag()
    {
        var aboutSection = Page.Locator("#about-us");

        await Expect(aboutSection.GetByText("100%")).ToBeVisibleAsync();
        await Expect(aboutSection.GetByText("Verified Barbers")).ToBeVisibleAsync();

        await Expect(aboutSection.GetByText("Easy")).ToBeVisibleAsync();

        await Expect(aboutSection.GetByText("Booking", new() { Exact = true })).ToBeVisibleAsync();

        await Expect(aboutSection.GetByText("#LookGoodFeelFresh").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task AboutUs_ShouldRenderImagesWithCorrectAltText()
    {
        var masterCutImg = Page.GetByRole(AriaRole.Img, new() { Name = "Master cut" });
        var barberToolsImg = Page.GetByRole(AriaRole.Img, new() { Name = "Barber tools" });

        await Expect(masterCutImg).ToBeVisibleAsync();
        await Expect(barberToolsImg).ToBeVisibleAsync();
    }
}
