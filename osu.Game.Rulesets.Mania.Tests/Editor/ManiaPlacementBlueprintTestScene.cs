// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public abstract class ManiaPlacementBlueprintTestScene : PlacementBlueprintTestScene
    {
        private readonly Column column;

        [Cached(typeof(IReadOnlyList<Mod>))]
        private IReadOnlyList<Mod> mods { get; set; } = Array.Empty<Mod>();

        [Cached(typeof(IScrollingInfo))]
        private IScrollingInfo scrollingInfo;

        protected ManiaPlacementBlueprintTestScene()
        {
            scrollingInfo = ((ScrollingTestContainer)HitObjectContainer).ScrollingInfo;

            Add(column = new Column(0)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AccentColour = Color4.OrangeRed,
                Clock = new FramedClock(new StopwatchClock()), // No scroll
            });
        }

        protected override SnapResult SnapForBlueprint(PlacementBlueprint blueprint)
        {
            double time = column.TimeAtScreenSpacePosition(InputManager.CurrentState.Mouse.Position);
            var pos = column.ScreenSpacePositionAtTime(time);

            return new SnapResult(pos, time, column);
        }

        protected override Container CreateHitObjectContainer() => new ScrollingTestContainer(ScrollingDirection.Down) { RelativeSizeAxes = Axes.Both };

        protected override void AddHitObject(DrawableHitObject hitObject) => column.Add((DrawableManiaHitObject)hitObject);

        public ManiaPlayfield Playfield => null;
    }
}
