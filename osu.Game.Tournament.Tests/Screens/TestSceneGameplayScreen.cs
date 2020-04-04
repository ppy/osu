// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens;
using osu.Game.Tournament.Screens.Gameplay;
using osu.Game.Tournament.Screens.Gameplay.Components;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneGameplayScreen : TournamentTestScene
    {
        [Cached]
        private TournamentMatchChatDisplay chat = new TournamentMatchChatDisplay { Width = 0.5f };

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(TeamScore),
            typeof(TeamScoreDisplay),
            typeof(TeamDisplay),
            typeof(MatchHeader),
            typeof(MatchScoreDisplay),
            typeof(BeatmapInfoScreen),
            typeof(SongBar),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new GameplayScreen());
            Add(chat);
        }
    }
}
