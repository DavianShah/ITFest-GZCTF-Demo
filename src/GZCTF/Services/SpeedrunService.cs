using GZCTF.Models.Request.Edit;
using GZCTF.Models.Request.Game;
using GZCTF.Repositories.Interface;
using GZCTF.Services.Cache;
using Microsoft.EntityFrameworkCore;

namespace GZCTF.Services;

public class SpeedrunService(AppDbContext context, IGameNoticeRepository noticeRepository, CacheHelper cacheHelper)
{
    public async Task<SpeedrunStateModel> GetState(int gameId, CancellationToken token = default)
    {
        var game = await context.Games.AsNoTracking().SingleOrDefaultAsync(g => g.Id == gameId, token);
        if (game is null || game.Mode != GameMode.Speedrun)
            return new() { IsSpeedrun = false };

        await UpdateExpiredRound(game, token);
        var round = await CurrentRound(gameId, token);
        if (round is { Status: SpeedrunRoundStatus.Running or SpeedrunRoundStatus.Overtime })
            await ReleaseDueHints(game, round, token);
        var categories = await context.SpeedrunCategories.AsNoTracking()
            .Where(c => c.GameId == gameId && c.Included).ToArrayAsync(token);
        var now = DateTimeOffset.UtcNow;
        return new()
        {
            IsSpeedrun = true,
            CurrentRound = round is null ? null : ToModel(round, now),
            UsedCategories = categories.Where(c => c.Used).Select(c => c.Category).ToArray(),
            RemainingCategories = categories.Where(c => !c.Used).Select(c => c.Category).ToArray(),
            Message = round?.Status == SpeedrunRoundStatus.Overtime ? game.SpeedrunEmergencyHintText : null
        };
    }

    public async Task<SpeedrunSettingsModel?> GetSettings(int gameId, CancellationToken token = default)
    {
        var game = await context.Games.AsNoTracking().SingleOrDefaultAsync(g => g.Id == gameId, token);
        if (game is null)
            return null;
        return new()
        {
            DefaultRoundDurationMinutes = game.SpeedrunDefaultRoundDurationMinutes,
            OvertimeMinutes = game.SpeedrunOvertimeMinutes,
            DefaultRoundDurationSeconds = game.SpeedrunDefaultRoundDurationSeconds,
            OvertimeSeconds = game.SpeedrunOvertimeSeconds,
            AllowManualExtend = game.SpeedrunAllowManualExtend,
            HideInactiveChallenges = game.SpeedrunHideInactiveChallenges,
            EmergencyHintEnabled = game.SpeedrunEmergencyHintEnabled,
            EmergencyHintText = game.SpeedrunEmergencyHintText,
            State = await GetState(gameId, token),
            Categories = await context.SpeedrunCategories.AsNoTracking().Where(c => c.GameId == gameId)
                .OrderBy(c => c.Category).Select(c => new SpeedrunCategoryModel
                { Id = c.Id, Category = c.Category, Included = c.Included, Used = c.Used }).ToArrayAsync(token)
        };
    }

    public async Task RefreshCategories(int gameId, CancellationToken token = default)
    {
        var found = await context.GameChallenges.AsNoTracking().Where(c => c.GameId == gameId)
            .Select(c => c.Category).Distinct().ToArrayAsync(token);
        var existing = await context.SpeedrunCategories.Where(c => c.GameId == gameId).ToArrayAsync(token);
        foreach (var category in found.Where(category => existing.All(e => e.Category != category)))
            context.SpeedrunCategories.Add(new() { GameId = gameId, Category = category });
        await context.SaveChangesAsync(token);
    }

    public async Task<SpeedrunRoundModel?> Spin(Game game, Guid? userId, CancellationToken token = default)
    {
        await UpdateExpiredRound(game, token);
        if (await CurrentRound(game.Id, token) is not null)
            return null;
        var categories = await context.SpeedrunCategories.Where(c => c.GameId == game.Id && c.Included && !c.Used)
            .ToArrayAsync(token);
        if (categories.Length == 0)
            return null;
        var selected = categories[Random.Shared.Next(categories.Length)];
        var round = new SpeedrunRound
        {
            GameId = game.Id, Category = selected.Category, Status = SpeedrunRoundStatus.Ready,
            DurationMinutes = game.SpeedrunDefaultRoundDurationMinutes, OvertimeMinutes = game.SpeedrunOvertimeMinutes,
            DurationSeconds = game.SpeedrunDefaultRoundDurationSeconds, OvertimeSeconds = game.SpeedrunOvertimeSeconds,
            SelectedByUserId = userId
        };
        context.SpeedrunRounds.Add(round);
        await context.SaveChangesAsync(token);
        await Announce(game.Id, $"Speedrun category selected: {selected.Category}", token);
        await cacheHelper.FlushScoreboardCache(game.Id, token);
        return ToModel(round, DateTimeOffset.UtcNow);
    }

    public async Task<bool> Start(Game game, int roundId, CancellationToken token = default)
    {
        if (await context.SpeedrunRounds.AnyAsync(r => r.GameId == game.Id &&
            (r.Status == SpeedrunRoundStatus.Running || r.Status == SpeedrunRoundStatus.Overtime), token))
            return false;
        var round = await context.SpeedrunRounds.SingleOrDefaultAsync(r => r.Id == roundId && r.GameId == game.Id, token);
        if (round is null || round.Status != SpeedrunRoundStatus.Ready)
            return false;
        var category = await context.SpeedrunCategories.SingleAsync(c => c.GameId == game.Id && c.Category == round.Category, token);
        category.Used = true;
        round.Status = SpeedrunRoundStatus.Running;
        round.StartedAtUtc = DateTimeOffset.UtcNow;
        round.EndsAtUtc = round.StartedAtUtc.Value.AddSeconds(round.DurationSeconds);
        await context.SaveChangesAsync(token);
        await Announce(game.Id, $"Speedrun round started: {round.Category}", token);
        await cacheHelper.FlushScoreboardCache(game.Id, token);
        return true;
    }

    public async Task<bool> End(int gameId, int roundId, CancellationToken token = default)
    {
        var round = await context.SpeedrunRounds.SingleOrDefaultAsync(r => r.Id == roundId && r.GameId == gameId, token);
        if (round is null || round.Status is SpeedrunRoundStatus.Finished or SpeedrunRoundStatus.Cancelled)
            return false;
        round.Status = SpeedrunRoundStatus.Finished;
        round.FinishedAtUtc = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(token);
        await Announce(gameId, "Speedrun round finished.", token);
        await cacheHelper.FlushScoreboardCache(gameId, token);
        return true;
    }

    public async Task<bool> Extend(Game game, int roundId, int seconds, CancellationToken token = default)
    {
        if (!game.SpeedrunAllowManualExtend)
            return false;
        var round = await context.SpeedrunRounds.SingleOrDefaultAsync(r => r.Id == roundId && r.GameId == game.Id, token);
        if (round is null || round.Status is not (SpeedrunRoundStatus.Running or SpeedrunRoundStatus.Overtime))
            return false;
        if (round.Status == SpeedrunRoundStatus.Overtime)
            round.OvertimeEndsAtUtc = (round.OvertimeEndsAtUtc ?? DateTimeOffset.UtcNow).AddSeconds(seconds);
        else
            round.EndsAtUtc = (round.EndsAtUtc ?? DateTimeOffset.UtcNow).AddSeconds(seconds);
        round.ManuallyExtendedSeconds += seconds;
        round.ManuallyExtendedMinutes = round.ManuallyExtendedSeconds / 60;
        await context.SaveChangesAsync(token);
        await cacheHelper.FlushScoreboardCache(game.Id, token);
        return true;
    }

    public async Task<bool> SetRemainingTime(Game game, int roundId, int remainingSeconds,
        CancellationToken token = default)
    {
        var round = await context.SpeedrunRounds.SingleOrDefaultAsync(r => r.Id == roundId && r.GameId == game.Id,
            token);
        if (round is null || round.Status is not (SpeedrunRoundStatus.Running or SpeedrunRoundStatus.Overtime))
            return false;

        var end = DateTimeOffset.UtcNow.AddSeconds(remainingSeconds);
        if (round.Status == SpeedrunRoundStatus.Overtime)
            round.OvertimeEndsAtUtc = end;
        else
            round.EndsAtUtc = end;
        await context.SaveChangesAsync(token);
        await cacheHelper.FlushScoreboardCache(game.Id, token);
        return true;
    }

    public async Task<bool> UpdateCategory(int gameId, int categoryId, bool? used, bool? included,
        CancellationToken token = default)
    {
        var category = await context.SpeedrunCategories.SingleOrDefaultAsync(c => c.Id == categoryId &&
            c.GameId == gameId, token);
        if (category is null)
            return false;

        var active = await CurrentRound(gameId, token);
        if (active?.Category == category.Category)
            return false;

        if (used.HasValue)
            category.Used = used.Value;
        if (included.HasValue)
            category.Included = included.Value;
        await context.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> ResetCategories(int gameId, CancellationToken token = default)
    {
        if (await CurrentRound(gameId, token) is not null)
            return false;
        await context.SpeedrunCategories.Where(c => c.GameId == gameId).ExecuteUpdateAsync(s => s.SetProperty(c => c.Used, false), token);
        return true;
    }

    public async Task<bool> CanAccessChallenge(Game game, int challengeId, CancellationToken token = default)
    {
        if (game.Mode != GameMode.Speedrun)
            return true;
        await UpdateExpiredRound(game, token);
        var round = await CurrentRound(game.Id, token);
        if (round is null || round.Status is not (SpeedrunRoundStatus.Running or SpeedrunRoundStatus.Overtime))
            return false;
        var challenge = await context.GameChallenges.AsNoTracking().SingleOrDefaultAsync(c => c.Id == challengeId && c.GameId == game.Id, token);
        if (challenge?.Category != round.Category)
            return false;
        return round.Status != SpeedrunRoundStatus.Overtime ||
               !await context.FirstSolves.AsNoTracking().AnyAsync(fs => fs.ChallengeId == challengeId, token);
    }

    public async Task<List<string>?> GetVisibleHints(Game game, GameChallenge challenge,
        CancellationToken token = default)
    {
        if (game.Mode != GameMode.Speedrun || challenge.Hints is not { Count: > 0 } hints)
            return challenge.Hints;

        var round = await CurrentRound(game.Id, token);
        if (round?.Status is not (SpeedrunRoundStatus.Running or SpeedrunRoundStatus.Overtime) ||
            round.StartedAtUtc is null)
            return [];

        await ReleaseDueHints(game, round, token);
        var elapsedSeconds = Math.Max(0, (int)(DateTimeOffset.UtcNow - round.StartedAtUtc.Value).TotalSeconds);
        return hints.Where((_, index) =>
            GetHintReleaseSeconds(challenge, index) <= elapsedSeconds).ToList();
    }

    private async Task UpdateExpiredRound(Game game, CancellationToken token)
    {
        var round = await context.SpeedrunRounds.SingleOrDefaultAsync(r => r.GameId == game.Id &&
            (r.Status == SpeedrunRoundStatus.Running || r.Status == SpeedrunRoundStatus.Overtime), token);
        if (round is null)
            return;
        var now = DateTimeOffset.UtcNow;
        if (round.Status == SpeedrunRoundStatus.Running && round.EndsAtUtc <= now)
        {
            var challengeIds = context.GameChallenges.Where(c => c.GameId == game.Id && c.Category == round.Category).Select(c => c.Id);
            var hasUnsolved = await challengeIds.AnyAsync(id => !context.FirstSolves.Any(fs => fs.ChallengeId == id), token);
            if (hasUnsolved && round.OvertimeSeconds > 0)
            {
                round.Status = SpeedrunRoundStatus.Overtime;
                round.OvertimeEndsAtUtc = now.AddSeconds(round.OvertimeSeconds);
                if (!round.OvertimeNoticeSent)
                {
                    round.OvertimeNoticeSent = true;
                    await Announce(game.Id, game.SpeedrunEmergencyHintEnabled ? game.SpeedrunEmergencyHintText : "Speedrun overtime started.", token);
                }
            }
            else
            {
                round.Status = SpeedrunRoundStatus.Finished;
                round.FinishedAtUtc = now;
            }
            await context.SaveChangesAsync(token);
            if (round.Status == SpeedrunRoundStatus.Finished)
                await Announce(game.Id, "Speedrun round finished.", token);
            await cacheHelper.FlushScoreboardCache(game.Id, token);
        }
        else if (round.Status == SpeedrunRoundStatus.Overtime && round.OvertimeEndsAtUtc <= now)
        {
            round.Status = SpeedrunRoundStatus.Finished;
            round.FinishedAtUtc = now;
            await context.SaveChangesAsync(token);
            await Announce(game.Id, "Speedrun round finished.", token);
            await cacheHelper.FlushScoreboardCache(game.Id, token);
        }
    }

    private Task<SpeedrunRound?> CurrentRound(int gameId, CancellationToken token) =>
        context.SpeedrunRounds.AsNoTracking().Where(r => r.GameId == gameId &&
            (r.Status == SpeedrunRoundStatus.Ready || r.Status == SpeedrunRoundStatus.Running || r.Status == SpeedrunRoundStatus.Overtime))
            .OrderByDescending(r => r.Id).FirstOrDefaultAsync(token);

    private async Task ReleaseDueHints(Game game, SpeedrunRound round, CancellationToken token)
    {
        if (round.StartedAtUtc is null)
            return;

        var elapsedSeconds = Math.Max(0, (int)(DateTimeOffset.UtcNow - round.StartedAtUtc.Value).TotalSeconds);
        var challenges = await context.GameChallenges.AsNoTracking()
            .Where(challenge => challenge.GameId == game.Id && challenge.Category == round.Category &&
                                challenge.IsEnabled)
            .ToArrayAsync(token);
        challenges = challenges.Where(challenge => challenge.Hints is { Count: > 0 }).ToArray();
        if (round.Status == SpeedrunRoundStatus.Overtime)
        {
            var solvedIds = await context.FirstSolves.AsNoTracking()
                .Where(solve => solve.Challenge.GameId == game.Id && solve.Challenge.Category == round.Category)
                .Select(solve => solve.ChallengeId)
                .ToArrayAsync(token);
            challenges = challenges.Where(challenge => !solvedIds.Contains(challenge.Id)).ToArray();
        }

        foreach (var challenge in challenges)
        {
            for (var index = 0; index < challenge.Hints!.Count; index++)
            {
                if (GetHintReleaseSeconds(challenge, index) > elapsedSeconds)
                    continue;

                var released = await context.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO "SpeedrunHintReleaseLogs" ("GameId", "RoundId", "ChallengeId", "HintIndex", "ReleasedAtUtc")
                    VALUES ({game.Id}, {round.Id}, {challenge.Id}, {index}, {DateTimeOffset.UtcNow})
                    ON CONFLICT ("RoundId", "ChallengeId", "HintIndex") DO NOTHING
                    """, token);
                if (released == 1)
                    await Announce(game.Id, $"Hint #{index + 1} dropped for {challenge.Title}.", token);
            }
        }
    }

    private static int GetHintReleaseSeconds(GameChallenge challenge, int index) =>
        challenge.SpeedrunHintReleaseSeconds?.ElementAtOrDefault(index) ??
        (challenge.SpeedrunHintReleaseMinutes?.ElementAtOrDefault(index) ?? 0) * 60;

    private async Task Announce(int gameId, string message, CancellationToken token) =>
        await noticeRepository.AddNotice(new() { GameId = gameId, Type = NoticeType.Normal, Values = [message] }, token);

    private static SpeedrunRoundModel ToModel(SpeedrunRound round, DateTimeOffset now)
    {
        var end = round.Status == SpeedrunRoundStatus.Overtime ? round.OvertimeEndsAtUtc : round.EndsAtUtc;
        return new()
        {
            Id = round.Id, Category = round.Category, Status = round.Status, StartedAtUtc = round.StartedAtUtc,
            EndsAtUtc = round.EndsAtUtc, OvertimeEndsAtUtc = round.OvertimeEndsAtUtc,
            TimeLeftSeconds = end is null ? 0 : Math.Max(0, (int)(end.Value - now).TotalSeconds)
        };
    }
}
