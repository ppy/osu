// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Screens.Play
{
    public partial class ArgonKeyCounterDisplay : KeyCounterDisplay
    {
        protected override FillFlowContainer<KeyCounter> KeyFlow { get; }

        public ArgonKeyCounterDisplay()
        {
            InternalChild = KeyFlow = new FillFlowContainer<KeyCounter>
            {
                Direction = FillDirection.Horizontal,
                AutoSizeAxes = Axes.Both,
                Alpha = 0,
                Spacing = new Vector2(2),
            };
        }

        protected override KeyCounter CreateCounter(InputTrigger trigger) => new ArgonKeyCounter(trigger);
    }
}
