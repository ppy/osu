// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Edit.Blueprints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class TestSceneHoldNoteSelectionBlueprint : ManiaSelectionBlueprintTestScene
    {
        private readonly DrawableHoldNote drawableObject;

        protected override Container<Drawable> Content => content ?? base.Content;
        private readonly Container content;

        public TestSceneHoldNoteSelectionBlueprint()
        {
            var holdNote = new HoldNote { Column = 0, Duration = 1000 };
            holdNote.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            base.Content.Child = content = new ScrollingTestContainer(ScrollingDirection.Down)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 50,
                Child = drawableObject = new DrawableHoldNote(holdNote)
                {
                    Height = 300,
                    AccentColour = { Value = OsuColour.Gray(0.3f) }
                }
            };

            AddBlueprint(new HoldNoteSelectionBlueprint(drawableObject));
        }

        protected override void Update()
        {
            base.Update();

            foreach (var nested in drawableObject.NestedHitObjects)
            {
                double finalPosition = (nested.HitObject.StartTime - drawableObject.HitObject.StartTime) / drawableObject.HitObject.Duration;
                nested.Y = (float)(-finalPosition * content.DrawHeight);
            }
        }
    }
}
