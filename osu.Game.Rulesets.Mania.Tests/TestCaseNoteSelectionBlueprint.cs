// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Edit.Blueprints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class TestCaseNoteSelectionBlueprint : ManiaSelectionBlueprintTestCase
    {
        private readonly DrawableNote drawableObject;

        protected override Container<Drawable> Content => content ?? base.Content;
        private readonly Container content;

        public TestCaseNoteSelectionBlueprint()
        {
            var note = new Note { Column = 0 };
            note.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            base.Content.Child = content = new ScrollingTestContainer(ScrollingDirection.Down)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(50, 20),
                Child = drawableObject = new DrawableNote(note)
            };
        }

        protected override SelectionBlueprint CreateBlueprint() => new NoteSelectionBlueprint(drawableObject);
    }
}
