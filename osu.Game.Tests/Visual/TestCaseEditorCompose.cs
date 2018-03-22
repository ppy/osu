// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Timing;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit.Screens.Compose;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseEditorCompose : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(Compose) };

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(parent);

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            osuGame.Beatmap.Value = new TestWorkingBeatmap(new OsuRuleset().RulesetInfo);

            var clock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };
            dependencies.CacheAs<IAdjustableClock>(clock);
            dependencies.CacheAs<IFrameBasedClock>(clock);

            var compose = new Compose();
            compose.Beatmap.BindTo(osuGame.Beatmap);

            Child = compose;
        }
    }
}
