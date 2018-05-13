// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Timing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Edit;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Screens.Edit.Screens.Compose;
using osu.Game.Tests.Beatmaps;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseManiaObjectComposer : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(HitObjectComposer),
            typeof(ManiaHitObjectComposer),
        };

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(parent);

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            // This shit does not work and needs a fix
            // Probably the fact that every map needs to be converted is breaking it
            // Let's ignore that for now for as long as it does not throw any exceptions
            osuGame.Beatmap.Value = new TestWorkingBeatmap(new ManiaBeatmap(new StageDefinition { Columns = 4 })
            {
                HitObjects = new List<ManiaHitObject>
                {
                    new Note { Column = 2, StartTime = 0.5 },
                    new Note { Column = 3, StartTime = 1.74 },
                    new HoldNote
                    {
                        Column = 1,
                        StartTime = 2.42,
                        Duration = 1,
                    }
                },
                ControlPointInfo = new ControlPointInfo()
            });

            var clock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };
            dependencies.CacheAs<IAdjustableClock>(clock);
            dependencies.CacheAs<IFrameBasedClock>(clock);

            Child = new ManiaHitObjectComposer(new ManiaRuleset(), new BindableBeatDivisor());
        }
    }
}
