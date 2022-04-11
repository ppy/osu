// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public abstract class MultiplayerGameplayLeaderboardTestScene : OsuTestScene
    {
        protected readonly BindableList<MultiplayerRoomUser> MultiplayerUsers = new BindableList<MultiplayerRoomUser>();
        private readonly BindableList<int> multiplayerUserIds = new BindableList<int>();

        protected MultiplayerGameplayLeaderboard Leaderboard { get; private set; }

        private OsuConfigManager config;

        private readonly Mock<SpectatorClient> spectatorClient = new Mock<SpectatorClient>();
        private readonly Mock<MultiplayerClient> multiplayerClient = new Mock<MultiplayerClient>();

        private readonly Dictionary<int, FrameHeader> lastHeaders = new Dictionary<int, FrameHeader>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(config = new OsuConfigManager(LocalStorage));
            Dependencies.CacheAs(spectatorClient.Object);
            Dependencies.CacheAs(multiplayerClient.Object);

            // To emulate `MultiplayerClient.CurrentMatchPlayingUserIds` we need a bindable list of *only IDs*.
            // This tracks the list of users 1:1.
            MultiplayerUsers.BindCollectionChanged((c, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        Debug.Assert(e.NewItems != null);

                        foreach (var user in e.NewItems.OfType<MultiplayerRoomUser>())
                            multiplayerUserIds.Add(user.UserID);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        Debug.Assert(e.OldItems != null);

                        foreach (var user in e.OldItems.OfType<MultiplayerRoomUser>())
                            multiplayerUserIds.Remove(user.UserID);
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        multiplayerUserIds.Clear();
                        break;
                }
            });

            multiplayerClient.SetupGet(c => c.CurrentMatchPlayingUserIds)
                             .Returns(() => multiplayerUserIds);
        }

        [SetUpSteps]
        public virtual void SetUpSteps()
        {
            AddStep("set local user", () => ((DummyAPIAccess)API).LocalUser.Value = new APIUser
            {
                Username = "local",
            });

            AddStep("populate users", () =>
            {
                MultiplayerUsers.Clear();
                for (int i = 0; i < 16; i++)
                    MultiplayerUsers.Add(CreateUser(i));
            });

            AddStep("create leaderboard", () =>
            {
                Leaderboard?.Expire();

                OsuScoreProcessor scoreProcessor;
                Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);

                var playableBeatmap = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);

                Child = scoreProcessor = new OsuScoreProcessor();

                scoreProcessor.ApplyBeatmap(playableBeatmap);

                LoadComponentAsync(Leaderboard = CreateLeaderboard(scoreProcessor), Add);
            });

            AddUntilStep("wait for load", () => Leaderboard.IsLoaded);
        }

        protected virtual MultiplayerRoomUser CreateUser(int i) => new MultiplayerRoomUser(i);

        protected abstract MultiplayerGameplayLeaderboard CreateLeaderboard(OsuScoreProcessor scoreProcessor);

        [Test]
        public void TestScoreUpdates()
        {
            AddRepeatStep("update state", UpdateUserStatesRandomly, 100);
            AddToggleStep("switch compact mode", expanded => Leaderboard.Expanded.Value = expanded);
        }

        [Test]
        public void TestUserQuit()
        {
            AddUntilStep("mark users quit", () =>
            {
                if (MultiplayerUsers.Count == 0)
                    return true;

                MultiplayerUsers.RemoveAt(0);
                return false;
            });
        }

        [Test]
        public void TestChangeScoringMode()
        {
            AddRepeatStep("update state", UpdateUserStatesRandomly, 5);
            AddStep("change to classic", () => config.SetValue(OsuSetting.ScoreDisplayMode, ScoringMode.Classic));
            AddStep("change to standardised", () => config.SetValue(OsuSetting.ScoreDisplayMode, ScoringMode.Standardised));
        }

        protected void UpdateUserStatesRandomly()
        {
            foreach (var user in MultiplayerUsers)
            {
                if (RNG.NextBool())
                    continue;

                int userId = user.UserID;

                if (!lastHeaders.TryGetValue(userId, out var header))
                {
                    lastHeaders[userId] = header = new FrameHeader(new ScoreInfo
                    {
                        Statistics = new Dictionary<HitResult, int>
                        {
                            [HitResult.Miss] = 0,
                            [HitResult.Meh] = 0,
                            [HitResult.Great] = 0
                        }
                    });
                }

                switch (RNG.Next(0, 3))
                {
                    case 0:
                        header.Combo = 0;
                        header.Statistics[HitResult.Miss]++;
                        break;

                    case 1:
                        header.Combo++;
                        header.MaxCombo = Math.Max(header.MaxCombo, header.Combo);
                        header.Statistics[HitResult.Meh]++;
                        break;

                    default:
                        header.Combo++;
                        header.MaxCombo = Math.Max(header.MaxCombo, header.Combo);
                        header.Statistics[HitResult.Great]++;
                        break;
                }

                spectatorClient.Raise(s => s.OnNewFrames -= null, userId, new FrameDataBundle(header, new[] { new LegacyReplayFrame(Time.Current, 0, 0, ReplayButtonState.None) }));
            }
        }
    }
}
