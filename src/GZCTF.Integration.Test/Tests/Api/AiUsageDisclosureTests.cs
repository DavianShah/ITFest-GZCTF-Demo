using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GZCTF.Integration.Test.Base;
using GZCTF.Models;
using GZCTF.Models.Data;
using GZCTF.Models.Request.Account;
using GZCTF.Models.Request.Edit;
using GZCTF.Models.Request.Game;
using GZCTF.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GZCTF.Integration.Test.Tests.Api;

[Collection(nameof(IntegrationTestCollection))]
public class AiUsageDisclosureTests(GZCTFApplicationFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new DateTimeOffsetJsonConverter() }
    };

    private const string RequiredMessage =
        "Isi link AI atau pernyataan bahwa Anda tidak memakai AI sebelum mengirim flag.";

    [Theory]
    [InlineData(null)]
    [InlineData("   \t\r\n")]
    public async Task JeopardySubmission_WithoutDisclosure_IsRejectedBeforePersistence(string? disclosure)
    {
        var scenario = await CreateScenarioAsync(GameMode.Jeopardy);
        using var client = scenario.Client;

        var response = await client.PostAsJsonAsync(
            $"/api/Game/{scenario.GameId}/Challenges/{scenario.ChallengeId}",
            new FlagSubmitModel { Flag = scenario.Flag, AiUsageDisclosure = disclosure });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(RequiredMessage, await response.Content.ReadAsStringAsync());

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await context.Submissions.AnyAsync(s => s.GameId == scenario.GameId));
    }

    [Theory]
    [InlineData("  https://chatgpt.com/share/example  ", "https://chatgpt.com/share/example")]
    [InlineData("  Saya tidak memakai AI  ", "Saya tidak memakai AI")]
    public async Task JeopardySubmission_WithDisclosure_EntersNormalPipelineAndStoresTrimmedValue(
        string disclosure, string expected)
    {
        var scenario = await CreateScenarioAsync(GameMode.Jeopardy);
        using var client = scenario.Client;

        var response = await client.PostAsJsonAsync(
            $"/api/Game/{scenario.GameId}/Challenges/{scenario.ChallengeId}",
            new FlagSubmitModel { Flag = scenario.Flag, AiUsageDisclosure = disclosure });

        response.EnsureSuccessStatusCode();
        var submissionId = await response.Content.ReadFromJsonAsync<int>();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var submission = await context.Submissions.IgnoreAutoIncludes().SingleAsync(s => s.Id == submissionId);
        Assert.Equal(scenario.GameId, submission.GameId);
        Assert.Equal(scenario.ChallengeId, submission.ChallengeId);
        Assert.Equal(expected, submission.AiUsageDisclosure);
        Assert.True(submission.Status is AnswerResult.FlagSubmitted or AnswerResult.Accepted);
    }

    [Theory]
    [InlineData("aa")]
    [InlineData("Saya tidak memakai AI dalam menyelesaikan challenge ini.")]
    [InlineData("chatgpt.com/share/example")]
    [InlineData("ftp://example.com/conversation")]
    [InlineData("<img src=x onerror=alert('xss')>")]
    public async Task JeopardySubmission_WithInvalidDisclosure_IsRejectedBeforePersistence(string disclosure)
    {
        var scenario = await CreateScenarioAsync(GameMode.Jeopardy);
        using var client = scenario.Client;

        var response = await client.PostAsJsonAsync(
            $"/api/Game/{scenario.GameId}/Challenges/{scenario.ChallengeId}",
            new FlagSubmitModel { Flag = scenario.Flag, AiUsageDisclosure = disclosure });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<RequestResponse>();
        Assert.Equal(AiUsageDisclosureValidator.InvalidFormatMessage, error?.Title);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await context.Submissions.AnyAsync(s => s.GameId == scenario.GameId));
    }

    [Fact]
    public async Task SpeedrunSubmission_WithoutDisclosure_RemainsValidAndNullable()
    {
        var scenario = await CreateScenarioAsync(GameMode.Speedrun);
        using var client = scenario.Client;

        var response = await client.PostAsJsonAsync(
            $"/api/Game/{scenario.GameId}/Challenges/{scenario.ChallengeId}",
            new FlagSubmitModel { Flag = scenario.Flag });

        response.EnsureSuccessStatusCode();
        var submissionId = await response.Content.ReadFromJsonAsync<int>();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var submission = await context.Submissions.IgnoreAutoIncludes().AsNoTracking()
            .SingleAsync(s => s.Id == submissionId);
        Assert.Null(submission.AiUsageDisclosure);
    }

    [Fact]
    public async Task JeopardySubmission_RequiredSolverWithoutFile_IsRejectedBeforePersistence()
    {
        var scenario = await CreateScenarioAsync(GameMode.Jeopardy, requireSolverUpload: true);
        using var client = scenario.Client;

        var response = await client.PostAsJsonAsync(
            $"/api/Game/{scenario.GameId}/Challenges/{scenario.ChallengeId}",
            new FlagSubmitModel
            {
                Flag = scenario.Flag,
                AiUsageDisclosure = AiUsageDisclosureValidator.NoAiDeclaration
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Upload file solver", await response.Content.ReadAsStringAsync());

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await context.Submissions.AnyAsync(s => s.GameId == scenario.GameId));
    }

    [Fact]
    public async Task JeopardySubmission_WithRequiredSolver_StoresFileOnSubmission()
    {
        var scenario = await CreateScenarioAsync(GameMode.Jeopardy, requireSolverUpload: true);
        using var client = scenario.Client;
        using var content = CreateSolverSubmission(scenario.Flag, "solve.py", "print('solver')");

        var response = await client.PostAsync(
            $"/api/Game/{scenario.GameId}/Challenges/{scenario.ChallengeId}/WithSolver", content);

        response.EnsureSuccessStatusCode();
        var submissionId = await response.Content.ReadFromJsonAsync<int>();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var submission = await context.Submissions.AsNoTracking().Include(item => item.SolverFile)
            .SingleAsync(item => item.Id == submissionId);
        Assert.Equal("solve.py", submission.SolverFileName);
        Assert.NotNull(submission.SolverFile);
        Assert.True(submission.SolverFile.FileSize > 0);
    }

    [Fact]
    public async Task SpeedrunSubmission_RequiredSolverSettingStillAllowsSubmissionWithoutFile()
    {
        var scenario = await CreateScenarioAsync(GameMode.Speedrun, requireSolverUpload: true);
        using var client = scenario.Client;

        var response = await client.PostAsJsonAsync(
            $"/api/Game/{scenario.GameId}/Challenges/{scenario.ChallengeId}",
            new FlagSubmitModel { Flag = scenario.Flag });

        response.EnsureSuccessStatusCode();
        var submissionId = await response.Content.ReadFromJsonAsync<int>();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var submission = await context.Submissions.IgnoreAutoIncludes().AsNoTracking()
            .SingleAsync(item => item.Id == submissionId);
        Assert.Null(submission.SolverFileId);
    }

    [Fact]
    public async Task AdminDisclosureList_ExposesSolverMetadataAndProtectedDownload()
    {
        const string solverContents = "print('admin solver download')";
        var scenario = await CreateScenarioAsync(GameMode.Jeopardy, requireSolverUpload: true);
        int submissionId;
        using (scenario.Client)
        using (var content = CreateSolverSubmission(scenario.Flag, "admin-solve.py", solverContents))
        {
            var submitResponse = await scenario.Client.PostAsync(
                $"/api/Game/{scenario.GameId}/Challenges/{scenario.ChallengeId}/WithSolver", content);
            submitResponse.EnsureSuccessStatusCode();
            submissionId = await submitResponse.Content.ReadFromJsonAsync<int>();

            var participantDownload = await scenario.Client.GetAsync(
                $"/api/Edit/Games/{scenario.GameId}/Submissions/{submissionId}/Solver");
            Assert.Equal(HttpStatusCode.Forbidden, participantDownload.StatusCode);
        }

        const string adminPassword = "DisclosureAdmin@Test123";
        var admin = await TestDataSeeder.CreateUserAsync(factory.Services, TestDataSeeder.RandomName(),
            adminPassword, role: Role.Admin);
        using var adminClient = factory.CreateClient();
        var loginResponse = await adminClient.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = admin.UserName, Password = adminPassword });
        loginResponse.EnsureSuccessStatusCode();

        var listResponse = await adminClient.GetAsync($"/api/Edit/Games/{scenario.GameId}/AiDisclosures");
        listResponse.EnsureSuccessStatusCode();
        var disclosures = await listResponse.Content.ReadFromJsonAsync<AiUsageDisclosureModel[]>(JsonOptions);
        var disclosure = Assert.Single(disclosures!);
        Assert.True(disclosure.HasSolverFile);
        Assert.Equal("admin-solve.py", disclosure.SolverFileName);
        Assert.True(disclosure.SolverFileSize > 0);

        var downloadResponse = await adminClient.GetAsync(
            $"/api/Edit/Games/{scenario.GameId}/Submissions/{disclosure.SubmissionId}/Solver");
        downloadResponse.EnsureSuccessStatusCode();
        Assert.Equal("nosniff", downloadResponse.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("application/octet-stream", downloadResponse.Content.Headers.ContentType?.MediaType);
        Assert.Equal(solverContents, await downloadResponse.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task JeopardySubmission_OverMaximumLength_IsRejectedBeforePersistence()
    {
        var scenario = await CreateScenarioAsync(GameMode.Jeopardy);
        using var client = scenario.Client;

        var response = await client.PostAsJsonAsync(
            $"/api/Game/{scenario.GameId}/Challenges/{scenario.ChallengeId}",
            new FlagSubmitModel
            {
                Flag = scenario.Flag,
                AiUsageDisclosure = new string('a', Limits.MaxAiUsageDisclosureLength + 1)
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await context.Submissions.AnyAsync(s => s.GameId == scenario.GameId));
    }

    [Fact]
    public async Task ExistingSubmission_WithNullDisclosure_RemainsReadable()
    {
        var scenario = await CreateScenarioAsync(GameMode.Jeopardy);
        using var client = scenario.Client;

        int submissionId;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var participation = await context.Participations.SingleAsync(p => p.GameId == scenario.GameId);
            var submission = new Submission
            {
                Answer = "legacy{flag}",
                AiUsageDisclosure = null,
                Status = AnswerResult.WrongAnswer,
                SubmitTimeUtc = DateTimeOffset.UtcNow.AddDays(-1),
                UserId = scenario.UserId,
                TeamId = participation.TeamId,
                ParticipationId = participation.Id,
                GameId = scenario.GameId,
                ChallengeId = scenario.ChallengeId
            };
            context.Submissions.Add(submission);
            await context.SaveChangesAsync();
            submissionId = submission.Id;
        }

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var legacy = await context.Submissions.IgnoreAutoIncludes().AsNoTracking()
                .SingleAsync(s => s.Id == submissionId);
            Assert.Null(legacy.AiUsageDisclosure);
        }
    }

    [Fact]
    public async Task MonitorSubmission_ReturnsPotentialMarkupOnlyAsDisclosureData()
    {
        const string maliciousDisclosure = "<img src=x onerror=alert('xss')>";
        var scenario = await CreateScenarioAsync(GameMode.Jeopardy);
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var participation = await context.Participations.SingleAsync(p => p.GameId == scenario.GameId);
            context.Submissions.Add(new Submission
            {
                Answer = "legacy{markup}",
                AiUsageDisclosure = maliciousDisclosure,
                Status = AnswerResult.WrongAnswer,
                SubmitTimeUtc = DateTimeOffset.UtcNow,
                UserId = scenario.UserId,
                TeamId = participation.TeamId,
                ParticipationId = participation.Id,
                GameId = scenario.GameId,
                ChallengeId = scenario.ChallengeId
            });
            await context.SaveChangesAsync();
        }

        const string monitorPassword = "Monitor@Test123";
        var monitor = await TestDataSeeder.CreateUserAsync(factory.Services, TestDataSeeder.RandomName(),
            monitorPassword, role: Role.Monitor);
        using var monitorClient = factory.CreateClient();
        var loginResponse = await monitorClient.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = monitor.UserName, Password = monitorPassword });
        loginResponse.EnsureSuccessStatusCode();

        var monitorResponse = await monitorClient.GetAsync($"/api/Game/{scenario.GameId}/Submissions");
        monitorResponse.EnsureSuccessStatusCode();
        Assert.Equal("application/json", monitorResponse.Content.Headers.ContentType?.MediaType);

        var submissions = await monitorResponse.Content.ReadFromJsonAsync<Submission[]>(JsonOptions);
        Assert.Contains(submissions!, submission => submission.AiUsageDisclosure == maliciousDisclosure);
    }

    private static MultipartFormDataContent CreateSolverSubmission(string flag, string fileName, string contents)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(flag), nameof(FlagSubmitWithSolverModel.Flag) },
            {
                new StringContent(AiUsageDisclosureValidator.NoAiDeclaration),
                nameof(FlagSubmitWithSolverModel.AiUsageDisclosure)
            }
        };
        var solver = new ByteArrayContent(Encoding.UTF8.GetBytes(contents));
        solver.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(solver, nameof(FlagSubmitWithSolverModel.SolverFile), fileName);
        return content;
    }

    private async Task<TestScenario> CreateScenarioAsync(GameMode mode, bool requireSolverUpload = false)
    {
        const string password = "Disclosure@Test123";
        var user = await TestDataSeeder.CreateUserAsync(factory.Services, TestDataSeeder.RandomName(), password);
        var team = await TestDataSeeder.CreateTeamAsync(factory.Services, user.Id,
            $"AI {TestDataSeeder.RandomName(10)}");
        var game = await TestDataSeeder.CreateGameAsync(factory.Services,
            $"AI Disclosure {TestDataSeeder.RandomName(8)}");
        var challenge = await TestDataSeeder.CreateStaticChallengeAsync(factory.Services, game.Id,
            $"AI Challenge {TestDataSeeder.RandomName(8)}", $"flag{{{TestDataSeeder.RandomName(10)}}}");

        if (mode == GameMode.Speedrun || requireSolverUpload)
        {
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var challengeEntity = await context.GameChallenges.SingleAsync(item => item.Id == challenge.Id);
            challengeEntity.RequireSolverUpload = requireSolverUpload;

            if (mode != GameMode.Speedrun)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                var gameEntity = await context.Games.SingleAsync(g => g.Id == game.Id);
                gameEntity.Mode = GameMode.Speedrun;
                context.SpeedrunCategories.Add(new SpeedrunCategory
                {
                    GameId = game.Id,
                    Category = ChallengeCategory.Misc,
                    Used = true,
                    Included = true
                });
                context.SpeedrunRounds.Add(new SpeedrunRound
                {
                    GameId = game.Id,
                    Category = ChallengeCategory.Misc,
                    Status = SpeedrunRoundStatus.Running,
                    StartedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1),
                    EndsAtUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    DurationMinutes = 11,
                    DurationSeconds = 660,
                    OvertimeMinutes = 5,
                    OvertimeSeconds = 300,
                    CreatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1)
                });
                await context.SaveChangesAsync();
            }
        }

        await TestDataSeeder.JoinGameAsync(factory.Services, game.Id, team.Id, user.Id);

        var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/Account/LogIn",
            new LoginModel { UserName = user.UserName, Password = password });
        loginResponse.EnsureSuccessStatusCode();

        return new TestScenario(client, game.Id, challenge.Id, challenge.Flag, user.Id);
    }

    private sealed record TestScenario(HttpClient Client, int GameId, int ChallengeId, string Flag, Guid UserId);
}
