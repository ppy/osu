﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Rulesets;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Screens;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    public abstract class EditorTestCase : ScreenTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(Editor), typeof(EditorScreen) };

        private readonly Ruleset ruleset;

        protected EditorTestCase(Ruleset ruleset)
        {
            this.ruleset = ruleset;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            osuGame.Beatmap.Value = new TestWorkingBeatmap(ruleset.RulesetInfo);

            LoadComponentAsync(new Editor(), LoadScreen);
        }
    }
}
