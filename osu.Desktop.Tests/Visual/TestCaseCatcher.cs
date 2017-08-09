// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Catch.UI;
using OpenTK;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseCatcher : OsuTestCase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Children = new Drawable[]
            {
                new CatchInputManager
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new CatcherArea
                    {
                        RelativePositionAxes = Axes.Both,
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Size = new Vector2(1, 0.2f),
                    }
                },
            };
        }
    }
}
