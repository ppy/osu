// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    public abstract class HitObjectComposerTestCase : OsuTestCase
    {
        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(parent);

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            osuGame.Beatmap.Value = new TestWorkingBeatmap(CreateBeatmap());

            var clock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };
            dependencies.CacheAs<IAdjustableClock>(clock);
            dependencies.CacheAs<IFrameBasedClock>(clock);

            Child = CreateComposer();
        }

        protected abstract IBeatmap CreateBeatmap();

        protected abstract HitObjectComposer CreateComposer();
    }
}
