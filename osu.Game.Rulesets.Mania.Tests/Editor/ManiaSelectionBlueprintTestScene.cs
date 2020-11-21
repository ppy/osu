// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public abstract class ManiaSelectionBlueprintTestScene : SelectionBlueprintTestScene
    {
        [Cached(Type = typeof(IAdjustableClock))]
        private readonly IAdjustableClock clock = new StopwatchClock();

        protected ManiaSelectionBlueprintTestScene()
        {
            Add(new Column(0)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AccentColour = Color4.OrangeRed,
                Clock = new FramedClock(new StopwatchClock()), // No scroll
            });
        }

        public ManiaPlayfield Playfield => null;
    }
}
