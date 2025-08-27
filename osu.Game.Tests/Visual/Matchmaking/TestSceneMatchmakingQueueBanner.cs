// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Matchmaking;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneMatchmakingQueueBanner : MultiplayerTestScene
    {
        private readonly Mock<TestPerformerFromScreenRunner> performer = new Mock<TestPerformerFromScreenRunner>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.CacheAs<IPerformFromScreenRunner>(performer.Object);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add banner", () => Child = new MatchmakingQueueBanner
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        [Test]
        public void TestChangeState()
        {
            AddStep("searching", () =>
            {
                ((IMultiplayerClient)MultiplayerClient).MatchmakingQueueJoined().WaitSafely();
                ((IMultiplayerClient)MultiplayerClient).MatchmakingQueueStatusChanged(new MatchmakingQueueStatus.Searching()).WaitSafely();
            });

            AddStep("found", () =>
            {
                ((IMultiplayerClient)MultiplayerClient).MatchmakingRoomInvited().WaitSafely();
                ((IMultiplayerClient)MultiplayerClient).MatchmakingQueueStatusChanged(new MatchmakingQueueStatus.MatchFound()).WaitSafely();
            });

            AddStep("joining", () =>
            {
                ((IMultiplayerClient)MultiplayerClient).MatchmakingQueueStatusChanged(new MatchmakingQueueStatus.JoiningMatch()).WaitSafely();
            });

            AddStep("queue left", () => ((IMultiplayerClient)MultiplayerClient).MatchmakingQueueLeft().WaitSafely());
        }

        // interface mocks break hot reload, mocking this stub implementation instead works around it.
        // see: https://github.com/moq/moq4/issues/1252
        [UsedImplicitly]
        public class TestPerformerFromScreenRunner : IPerformFromScreenRunner
        {
            public virtual void PerformFromScreen(Action<IScreen> action, IEnumerable<Type>? validScreens = null)
            {
            }
        }
    }
}
