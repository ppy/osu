// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSpectatorDrivenLeaderboard : OsuTestScene
    {
        [Cached(typeof(SpectatorStreamingClient))]
        private TestSceneSpectator.TestSpectatorStreamingClient testSpectatorStreamingClient = new TestSceneSpectator.TestSpectatorStreamingClient();

        // used just to show beatmap card for the time being.
        protected override bool UseOnlineAPI => true;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            OsuScoreProcessor scoreProcessor;

            testSpectatorStreamingClient.StartPlay(55);

            Children = new Drawable[]
            {
                scoreProcessor = new OsuScoreProcessor(),
                new MultiplayerGameplayLeaderboard(scoreProcessor)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        });
    }

    public class MultiplayerGameplayLeaderboard : GameplayLeaderboard
    {
        private readonly OsuScoreProcessor scoreProcessor;

        public MultiplayerGameplayLeaderboard(OsuScoreProcessor scoreProcessor)
        {
            this.scoreProcessor = scoreProcessor;

            AddPlayer(new BindableDouble(), new GuestUser());
        }

        [Resolved]
        private SpectatorStreamingClient streamingClient { get; set; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Console.WriteLine("got here");

            foreach (var user in streamingClient.PlayingUsers)
            {
                streamingClient.WatchUser(user);
                var resolvedUser = userLookupCache.GetUserAsync(user).Result;
                AddPlayer(new BindableDouble(), resolvedUser);
            }
        }
    }
}
