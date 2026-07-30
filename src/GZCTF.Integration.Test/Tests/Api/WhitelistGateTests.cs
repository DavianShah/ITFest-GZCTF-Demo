using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GZCTF.Integration.Test.Base;
using GZCTF.Models;
using GZCTF.Models.Data;
using GZCTF.Models.Request.Account;
using GZCTF.Models.Request.Admin;
using GZCTF.Models.Request.Edit;
using GZCTF.Models.Request.Game;
using GZCTF.Repositories.Interface;
using GZCTF.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GZCTF.Integration.Test.Tests.Api;

/// <summary>
/// Integration tests for Whitelist Gate (Game.WhitelistOnly + Participation.WhitelistSource)
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public class WhitelistGateTests(GZCTFApplicationFactory factory)
{
    private async Task SetWhitelistOnly(int gameId, bool enabled)
    {
        using var scope = factory.Services.CreateScope();
        var gameRepo = scope.ServiceProvider.GetRequiredService<IGameRepository>();
        var entity = await gameRepo.GetGameById(gameId);
        Assert.NotNull(entity);
        entity.WhitelistOnly = enabled;
        await gameRepo.SaveAsync();
    }

    /// <summary>
    /// The real onboarding flow is public only through its random token, provisions one
    /// team into multiple games, and leaves the native team invite-code flow usable.
    /// </summary>
    [Fact]
    public async Task CaptainOnboarding_ProvisionsMultipleGames_AndNativeInviteStillWorks()
    {
        var adminPassword = "Admin@Pass123";
        var admin = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), adminPassword, role: Role.Admin);
        var warmup = await TestDataSeeder.CreateGameAsync(factory.Services, "Onboarding Warmup");
        var final = await TestDataSeeder.CreateGameAsync(factory.Services, "Onboarding Final");
        await SetWhitelistOnly(warmup.Id, true);
        await SetWhitelistOnly(final.Id, true);

        var teamName = $"Paid_{Guid.NewGuid():N}"[..18];
        var captainEmail = $"captain_{Guid.NewGuid():N}@example.local";

        using var adminClient = factory.CreateClient();
        var adminLogin = await adminClient.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = admin.UserName, Password = adminPassword });
        adminLogin.EnsureSuccessStatusCode();

        var batchResponse = await adminClient.PostAsJsonAsync("/api/Admin/Onboarding",
            new CaptainOnboardingBatchModel
            {
                GameIds = [warmup.Id, final.Id],
                Entries =
                [
                    new CaptainOnboardingEntryModel
                    {
                        TeamName = teamName,
                        CaptainEmail = captainEmail
                    }
                ]
            });
        batchResponse.EnsureSuccessStatusCode();

        var created = await batchResponse.Content.ReadFromJsonAsync<CaptainOnboardingCreatedModel[]>();
        Assert.NotNull(created);
        var invite = Assert.Single(created);
        var rawToken = new Uri(invite.OnboardingUrl).Query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .Single(part => part[0] == "token")[1];

        // No admin session: possession of the unguessable token is the only requirement.
        using var captainClient = factory.CreateClient();
        var infoResponse = await captainClient.GetAsync($"/api/Onboarding/{rawToken}");
        infoResponse.EnsureSuccessStatusCode();
        using var infoDocument = JsonDocument.Parse(await infoResponse.Content.ReadAsStringAsync());
        var info = infoDocument.RootElement;
        Assert.Equal(teamName, info.GetProperty("teamName").GetString());
        var gameTitles = info.GetProperty("gameTitles")
            .EnumerateArray()
            .Select(element => element.GetString())
            .ToArray();
        Assert.Contains(warmup.Title, gameTitles);
        Assert.Contains(final.Title, gameTitles);

        var openedHistoryResponse = await adminClient.GetAsync("/api/Admin/Onboarding?count=20&skip=0");
        openedHistoryResponse.EnsureSuccessStatusCode();
        using (var historyDocument = JsonDocument.Parse(await openedHistoryResponse.Content.ReadAsStringAsync()))
        {
            var historyEntry = historyDocument.RootElement
                .EnumerateArray()
                .Single(entry => entry.GetProperty("inviteId").GetInt32() == invite.InviteId);
            Assert.Equal("Opened", historyEntry.GetProperty("status").GetString());
            Assert.Equal(1, historyEntry.GetProperty("sendCount").GetInt32());
        }

        // Revoking retains history but immediately invalidates the current token.
        var revokeResponse = await adminClient.DeleteAsync($"/api/Admin/Onboarding/{invite.InviteId}");
        revokeResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NotFound,
            (await captainClient.GetAsync($"/api/Onboarding/{rawToken}")).StatusCode);

        var resendResponse = await adminClient.PostAsJsonAsync(
            $"/api/Admin/Onboarding/{invite.InviteId}/Resend",
            new CaptainOnboardingResendModel { ExpiresInHours = 24 });
        resendResponse.EnsureSuccessStatusCode();
        var replacement = await resendResponse.Content.ReadFromJsonAsync<CaptainOnboardingCreatedModel>();
        Assert.NotNull(replacement);
        rawToken = new Uri(replacement.OnboardingUrl).Query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .Single(part => part[0] == "token")[1];

        var replacementInfoResponse = await captainClient.GetAsync($"/api/Onboarding/{rawToken}");
        replacementInfoResponse.EnsureSuccessStatusCode();

        var captainName = TestDataSeeder.RandomName();
        var captainPassword = "Captain@Pass123";
        var redeemResponse = await captainClient.PostAsJsonAsync($"/api/Onboarding/{rawToken}/Redeem",
            new CaptainOnboardingRedeemModel
            {
                UserName = captainName,
                Password = captainPassword
            });
        redeemResponse.EnsureSuccessStatusCode();
        var redeem = await redeemResponse.Content.ReadFromJsonAsync<CaptainOnboardingRedeemResultModel>();
        Assert.NotNull(redeem);
        Assert.Equal(teamName, redeem.TeamName);
        Assert.False(string.IsNullOrWhiteSpace(redeem.InviteCode));

        var redeemedHistoryResponse = await adminClient.GetAsync("/api/Admin/Onboarding?count=20&skip=0");
        redeemedHistoryResponse.EnsureSuccessStatusCode();
        using (var historyDocument = JsonDocument.Parse(await redeemedHistoryResponse.Content.ReadAsStringAsync()))
        {
            var historyEntry = historyDocument.RootElement
                .EnumerateArray()
                .Single(entry => entry.GetProperty("inviteId").GetInt32() == invite.InviteId);
            Assert.Equal("Redeemed", historyEntry.GetProperty("status").GetString());
            Assert.Equal(2, historyEntry.GetProperty("sendCount").GetInt32());
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var team = await db.Teams
                .Include(item => item.Members)
                .Include(item => item.Captain)
                .SingleAsync(item => item.Id == redeem.TeamId);

            Assert.Equal(team.CaptainId, team.Members.Single().Id);
            Assert.Equal(captainEmail, team.Captain?.Email);

            var participations = await db.Participations
                .Where(participation => participation.TeamId == team.Id &&
                                        (participation.GameId == warmup.Id || participation.GameId == final.Id))
                .ToArrayAsync();

            Assert.Equal(2, participations.Length);
            Assert.All(participations, participation =>
            {
                Assert.Equal(ParticipationStatus.Accepted, participation.Status);
                Assert.Equal(WhitelistSource.BulkOnboarding, participation.WhitelistSource);
            });
        }

        // Captain receives the original GZCTF invite code and an ordinary account can still use it.
        var inviteCodeResponse = await captainClient.GetAsync($"/api/Team/{redeem.TeamId}/Invite");
        inviteCodeResponse.EnsureSuccessStatusCode();
        var inviteCode = await inviteCodeResponse.Content.ReadFromJsonAsync<string>();
        Assert.Equal(redeem.InviteCode, inviteCode);

        var memberPassword = "Member@Pass123";
        var member = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), memberPassword);
        using var memberClient = factory.CreateClient();
        var memberLogin = await memberClient.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = member.UserName, Password = memberPassword });
        memberLogin.EnsureSuccessStatusCode();

        var acceptResponse = await memberClient.PostAsJsonAsync("/api/Team/Accept", inviteCode);
        acceptResponse.EnsureSuccessStatusCode();

        using (var scope = factory.Services.CreateScope())
        {
            var isMember = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Teams
                .AnyAsync(team => team.Id == redeem.TeamId &&
                                  team.Members.Any(user => user.Id == member.Id));
            Assert.True(isMember);
        }
    }

    /// <summary>
    /// Test 1: Team from Bulk Onboarding auto-accepted in WhitelistOnly game
    /// </summary>
    [Fact]
    public async Task BulkOnboardingTeam_CanJoinWhitelistOnlyGame()
    {
        // Arrange
        var adminPassword = "Admin@Pass123";
        var admin = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), adminPassword, role: Role.Admin);

        var game = await TestDataSeeder.CreateGameAsync(factory.Services, "BulkOnboarding Whitelist Game");
        await SetWhitelistOnly(game.Id, true);

        // Simulate onboarding: create participation with WhitelistSource.BulkOnboarding, Accepted
        var userPassword = "User@Pass123";
        var captain = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), userPassword, email: $"captain_{Guid.NewGuid():N}@example.local");

        var teamName = $"OnboardedTeam_{Guid.NewGuid():N}"[..19];
        var team = await TestDataSeeder.CreateTeamAsync(factory.Services, captain.Id, teamName);

        // Directly create pre-approved participation (simulating onboarding redeem)
        using var scope = factory.Services.CreateScope();
        var gameRepo = scope.ServiceProvider.GetRequiredService<IGameRepository>();
        var gameEntity = await gameRepo.GetGameById(game.Id);
        Assert.NotNull(gameEntity);
        var dbTeam = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Teams
            .FirstAsync(t => t.Id == team.Id);
        var token = gameRepo.GetToken(gameEntity, dbTeam);

        var participation = new Participation
        {
            GameId = game.Id, TeamId = team.Id, Status = ParticipationStatus.Accepted,
            WhitelistSource = WhitelistSource.BulkOnboarding, Token = token
        };
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Participations.Add(participation);
        await context.SaveChangesAsync();

        // Act - Captain joins the game
        using var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = captain.UserName, Password = userPassword });
        login.EnsureSuccessStatusCode();

        var joinResponse = await client.PostAsJsonAsync($"/api/Game/{game.Id}",
            new GameJoinModel { TeamId = team.Id });

        // Assert - Should succeed because whitelisted via BulkOnboarding
        Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);
    }

    /// <summary>
    /// Test 2: Manual team (no whitelist) is rejected from WhitelistOnly game with 403
    /// </summary>
    [Fact]
    public async Task ManualTeam_RejectedFromWhitelistOnlyGame()
    {
        // Arrange
        var game = await TestDataSeeder.CreateGameAsync(factory.Services, "Reject Manual Team Game");
        await SetWhitelistOnly(game.Id, true);

        var userPassword = "User@Pass123";
        var user = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), userPassword);
        var team = await TestDataSeeder.CreateTeamAsync(factory.Services, user.Id,
            $"Manual_{user.UserName}");

        using var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = user.UserName, Password = userPassword });
        login.EnsureSuccessStatusCode();

        // Act
        var joinResponse = await client.PostAsJsonAsync($"/api/Game/{game.Id}",
            new GameJoinModel { TeamId = team.Id });

        // Assert - 403 with whitelist message
        Assert.Equal(HttpStatusCode.Forbidden, joinResponse.StatusCode);
        var error = await joinResponse.Content.ReadFromJsonAsync<RequestResponse>();
        Assert.NotNull(error);
        Assert.Contains("whitelist", error.Title, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Test 3: After admin manual whitelist, team can join WhitelistOnly game
    /// </summary>
    [Fact]
    public async Task ManualWhitelistedTeam_CanJoinWhitelistOnlyGame()
    {
        // Arrange
        var adminPassword = "Admin@Pass123";
        var admin = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), adminPassword, role: Role.Admin);

        var game = await TestDataSeeder.CreateGameAsync(factory.Services, "Manual Whitelist Game");
        await SetWhitelistOnly(game.Id, true);

        var userPassword = "User@Pass123";
        var user = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), userPassword);
        var team = await TestDataSeeder.CreateTeamAsync(factory.Services, user.Id,
            $"Whitelisted_{user.UserName}");

        using var adminClient = factory.CreateClient();
        var adminLogin = await adminClient.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = admin.UserName, Password = adminPassword });
        adminLogin.EnsureSuccessStatusCode();

        // Admin adds team to whitelist via API
        var whitelistResponse = await adminClient.PostAsJsonAsync(
            $"/api/Admin/Games/{game.Id}/Whitelist",
            new WhitelistTeamsRequest { TeamIds = [team.Id] });
        whitelistResponse.EnsureSuccessStatusCode();

        // Act - Join game
        using var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = user.UserName, Password = userPassword });
        login.EnsureSuccessStatusCode();

        var joinResponse = await client.PostAsJsonAsync($"/api/Game/{game.Id}",
            new GameJoinModel { TeamId = team.Id });

        // Assert
        Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

        // Verify WhitelistSource is ManualWhitelist
        using var scope = factory.Services.CreateScope();
        var participationRepo = scope.ServiceProvider.GetRequiredService<IParticipationRepository>();
        var participation = await participationRepo.GetParticipation(user.Id, game.Id);
        Assert.NotNull(participation);
        Assert.Equal(WhitelistSource.ManualWhitelist, participation.WhitelistSource);
        Assert.Equal(ParticipationStatus.Accepted, participation.Status);
    }

    /// <summary>
    /// Test 4: Game with WhitelistOnly=false behaves identically to before
    /// </summary>
    [Fact]
    public async Task NonWhitelistOnlyGame_JoinBehavesAsBefore()
    {
        // Arrange
        var gameId = await TestDataSeeder.GetOrCreateBasicGameAsync(factory.Services);
        var userPassword = "User@Pass123";
        var user = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), userPassword);
        var team = await TestDataSeeder.CreateTeamAsync(factory.Services, user.Id,
            $"Team {user.UserName}");

        using var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = user.UserName, Password = userPassword });
        login.EnsureSuccessStatusCode();

        // Act - Join game that has WhitelistOnly=false (default)
        var joinResponse = await client.PostAsJsonAsync($"/api/Game/{gameId}",
            new GameJoinModel { TeamId = team.Id });

        // Assert - Should succeed (normal behavior)
        Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

        // Verify participation created without WhitelistSource
        using var scope = factory.Services.CreateScope();
        var participationRepo = scope.ServiceProvider.GetRequiredService<IParticipationRepository>();
        var participation = await participationRepo.GetParticipation(user.Id, gameId);
        Assert.NotNull(participation);
        Assert.Equal(WhitelistSource.None, participation.WhitelistSource);
    }

    /// <summary>
    /// Test 5: User registration (POST /api/Account/Register) still works
    /// </summary>
    [Fact]
    public async Task UserRegistration_StillWorks()
    {
        // Arrange
        var userName = TestDataSeeder.RandomName();
        var password = "NewUser@Pass123";

        using var client = factory.CreateClient();

        // Act
        var registerResponse = await client.PostAsJsonAsync("/api/Account/Register",
            new RegisterModel
            {
                UserName = userName,
                Password = password,
                Email = $"{userName}@example.local"
            });

        // Assert
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        var result = await registerResponse.Content.ReadFromJsonAsync<RequestResponse<RegisterStatus>>();
        Assert.NotNull(result);
    }

    /// <summary>
    /// Test 6: Admin whitelist API - add and verify team appears in whitelist
    /// </summary>
    [Fact]
    public async Task AdminWhitelistApi_AddAndVerify()
    {
        // Arrange
        var adminPassword = "Admin@Pass123";
        var admin = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), adminPassword, role: Role.Admin);

        var game = await TestDataSeeder.CreateGameAsync(factory.Services, "Whitelist API Test Game");

        var userPassword = "User@Pass123";
        var user = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), userPassword);
        var team = await TestDataSeeder.CreateTeamAsync(factory.Services, user.Id,
            $"WL_API_{user.UserName}");

        using var adminClient = factory.CreateClient();
        var login = await adminClient.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = admin.UserName, Password = adminPassword });
        login.EnsureSuccessStatusCode();

        // Act - Add to whitelist
        var addResponse = await adminClient.PostAsJsonAsync(
            $"/api/Admin/Games/{game.Id}/Whitelist",
            new WhitelistTeamsRequest { TeamIds = [team.Id] });
        addResponse.EnsureSuccessStatusCode();

        // Get whitelist
        var getResponse = await adminClient.GetAsync($"/api/Admin/Games/{game.Id}/Whitelist");
        getResponse.EnsureSuccessStatusCode();
        var whitelist = await getResponse.Content.ReadFromJsonAsync<WhitelistTeamModel[]>();

        // Assert
        Assert.NotNull(whitelist);
        Assert.Contains(whitelist, w => w.TeamId == team.Id && w.Source == WhitelistSource.ManualWhitelist);
    }

    /// <summary>
    /// Test 7: Admin remove from whitelist - can't join after removal
    /// </summary>
    [Fact]
    public async Task AdminRemoveWhitelist_PreventsJoin()
    {
        // Arrange
        var adminPassword = "Admin@Pass123";
        var admin = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), adminPassword, role: Role.Admin);

        var game = await TestDataSeeder.CreateGameAsync(factory.Services, "Remove Whitelist Game");
        await SetWhitelistOnly(game.Id, true);

        var userPassword = "User@Pass123";
        var user = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), userPassword);
        var team = await TestDataSeeder.CreateTeamAsync(factory.Services, user.Id,
            $"RemoveWL_{user.UserName}");

        using var adminClient = factory.CreateClient();
        var adminLogin = await adminClient.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = admin.UserName, Password = adminPassword });
        adminLogin.EnsureSuccessStatusCode();

        // Add to whitelist
        var addResponse = await adminClient.PostAsJsonAsync(
            $"/api/Admin/Games/{game.Id}/Whitelist",
            new WhitelistTeamsRequest { TeamIds = [team.Id] });
        addResponse.EnsureSuccessStatusCode();

        // Remove from whitelist
        var removeResponse = await adminClient.DeleteAsync(
            $"/api/Admin/Games/{game.Id}/Whitelist/{team.Id}");
        removeResponse.EnsureSuccessStatusCode();

        // Act - Join game
        using var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = user.UserName, Password = userPassword });
        login.EnsureSuccessStatusCode();

        var joinResponse = await client.PostAsJsonAsync($"/api/Game/{game.Id}",
            new GameJoinModel { TeamId = team.Id });

        // Assert - Should be rejected
        Assert.Equal(HttpStatusCode.Forbidden, joinResponse.StatusCode);
    }

    /// <summary>
    /// Test 8: WhitelistOnly toggle can be set via GameInfoModel API
    /// </summary>
    [Fact]
    public async Task WhitelistOnlyToggle_CanBeSetViaApi()
    {
        // Arrange
        var adminPassword = "Admin@Pass123";
        var admin = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), adminPassword, role: Role.Admin);

        var game = await TestDataSeeder.CreateGameAsync(factory.Services, "Toggle WhitelistOnly Game");

        using var adminClient = factory.CreateClient();
        var login = await adminClient.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = admin.UserName, Password = adminPassword });
        login.EnsureSuccessStatusCode();

        // Set WhitelistOnly = true via PUT endpoint
        var enableResponse = await adminClient.PutAsJsonAsync($"/api/Edit/Games/{game.Id}",
            new GameInfoModel
            {
                Title = game.Title, Summary = "Test", Content = "Test",
                WhitelistOnly = true, AcceptWithoutReview = true,
                TeamMemberCountLimit = 0, ContainerCountLimit = 3,
                Hidden = false, PracticeMode = false, WriteupRequired = false,
                StartTimeUtc = game.Start, EndTimeUtc = game.End, WriteupDeadline = game.End
            });
        enableResponse.EnsureSuccessStatusCode();

        // Verify via DB directly (use a new context to avoid caching)
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var gameEntity = await db.Games.AsNoTracking().FirstAsync(g => g.Id == game.Id);
            Assert.True(gameEntity.WhitelistOnly);
        }

        // Set WhitelistOnly = false
        var disableResponse = await adminClient.PutAsJsonAsync($"/api/Edit/Games/{game.Id}",
            new GameInfoModel
            {
                Title = game.Title, Summary = "Test", Content = "Test",
                WhitelistOnly = false, AcceptWithoutReview = true,
                TeamMemberCountLimit = 0, ContainerCountLimit = 3,
                Hidden = false, PracticeMode = false, WriteupRequired = false,
                StartTimeUtc = game.Start, EndTimeUtc = game.End, WriteupDeadline = game.End
            });
        disableResponse.EnsureSuccessStatusCode();

        // Re-verify via DB (fresh scope, no tracking)
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var gameEntity = await db.Games.AsNoTracking().FirstAsync(g => g.Id == game.Id);
            Assert.False(gameEntity.WhitelistOnly);
        }
    }

    /// <summary>
    /// Test 9: Admin search whitelist teams endpoint works
    /// </summary>
    [Fact]
    public async Task AdminSearchWhitelistTeams_ReturnsCorrectTeams()
    {
        // Arrange
        var adminPassword = "Admin@Pass123";
        var admin = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), adminPassword, role: Role.Admin);

        var game = await TestDataSeeder.CreateGameAsync(factory.Services, "Search Whitelist Teams Game");

        var userPassword = "User@Pass123";
        var user = await TestDataSeeder.CreateUserAsync(factory.Services,
            TestDataSeeder.RandomName(), userPassword);
        var team = await TestDataSeeder.CreateTeamAsync(factory.Services, user.Id,
            $"SearchTeam_{user.UserName}");

        using var adminClient = factory.CreateClient();
        var login = await adminClient.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = admin.UserName, Password = adminPassword });
        login.EnsureSuccessStatusCode();

        // Add team to whitelist
        await adminClient.PostAsJsonAsync(
            $"/api/Admin/Games/{game.Id}/Whitelist",
            new WhitelistTeamsRequest { TeamIds = [team.Id] });

        // Search for the team - should be excluded since already whitelisted
        var searchQuery = team.Name.Length > 5 ? team.Name[..5] : team.Name;
        var searchResponse = await adminClient.GetAsync(
            $"/api/Admin/Games/{game.Id}/Whitelist/search-teams?query={Uri.EscapeDataString(searchQuery)}");
        searchResponse.EnsureSuccessStatusCode();
        var searchResults = await searchResponse.Content.ReadFromJsonAsync<TeamWithDetailedUserInfo[]>();

        Assert.NotNull(searchResults);
        Assert.DoesNotContain(searchResults, t => t.Id == team.Id);
    }
}
