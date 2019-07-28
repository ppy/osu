// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Rulesets.Mania.Edit;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests
{
    [Cached(Type = typeof(IManiaHitObjectComposer))]
    public abstract class ManiaSelectionBlueprintTestScene : SelectionBlueprintTestScene, IManiaHitObjectComposer
    {
        [Cached(Type = typeof(IAdjustableClock))]
        private readonly IAdjustableClock clock = new StopwatchClock();

        private readonly Column column;

        protected ManiaSelectionBlueprintTestScene()
        {
            Add(column = new Column(0)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AccentColour = Color4.OrangeRed,
                Clock = new FramedClock(new StopwatchClock()), // No scroll
            });
        }

        public Column ColumnAt(Vector2 screenSpacePosition) => column;

        public int TotalColumns => 1;
    }
}
