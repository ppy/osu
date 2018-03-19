// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Screens.Compose;
using OpenTK;

namespace osu.Game.Tests.Visual
{
    public class TestCaseDrawableBeatDivisor : OsuTestCase
    {
        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(parent);

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.Cache(new BindableBeatDivisor());

            Child = new DrawableBeatDivisor
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Y = -200,
                Size = new Vector2(100, 110)
            };
        }
    }
}
