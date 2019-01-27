// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Rulesets.Mania.Edit;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests
{
    [Cached(Type = typeof(IManiaHitObjectComposer))]
    public abstract class ManiaPlacementBlueprintTestCase : PlacementBlueprintTestCase, IManiaHitObjectComposer
    {
        private readonly Column column;

        protected ManiaPlacementBlueprintTestCase()
        {
            Add(column = new Column(0)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AccentColour = Color4.OrangeRed,
                Clock = new FramedClock(new StopwatchClock()), // No scroll
            });
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.CacheAs(((ScrollingTestContainer)HitObjectContainer).ScrollingInfo);

            return dependencies;
        }

        protected override Container CreateHitObjectContainer() => new ScrollingTestContainer(ScrollingDirection.Down) { RelativeSizeAxes = Axes.Both };

        protected override void AddHitObject(DrawableHitObject hitObject) => column.Add((DrawableManiaHitObject)hitObject);

        public Column ColumnAt(Vector2 screenSpacePosition) => column;

        public int TotalColumns => 1;
    }
}
