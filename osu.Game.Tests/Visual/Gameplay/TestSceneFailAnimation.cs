// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneFailAnimation : AllPlayersTestScene
    {
        protected override Player CreatePlayer(Ruleset ruleset)
        {
            Mods.Value = Array.Empty<Mod>();
            return new FailPlayer();
        }

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(AllPlayersTestScene),
            typeof(TestPlayer),
            typeof(Player),
        };

        protected override void AddCheckSteps()
        {
            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddUntilStep("wait for fail overlay", () => ((FailPlayer)Player).FailOverlay.State.Value == Visibility.Visible);
        }

        private class FailPlayer : TestPlayer
        {
            public new FailOverlay FailOverlay => base.FailOverlay;

            public FailPlayer()
                : base(false, false)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                ScoreProcessor.FailConditions += (_, __) => true;
            }
        }
    }
}
