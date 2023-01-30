// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Edit;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public partial class TestSceneManiaHitObjectComposer : EditorClockTestScene
    {
        private TestComposer composer;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            BeatDivisor.Value = 8;
            EditorClock.Seek(0);

            Child = composer = new TestComposer { RelativeSizeAxes = Axes.Both };
        });

        [Test]
        public void TestDragOffscreenSelectionVerticallyUpScroll()
        {
            DrawableHitObject lastObject = null;
            double originalTime = 0;
            Vector2 originalPosition = Vector2.Zero;

            setScrollStep(ScrollingDirection.Up);

            AddStep("seek to last object", () =>
            {
                lastObject = this.ChildrenOfType<DrawableHitObject>().Single(d => d.HitObject == composer.EditorBeatmap.HitObjects.Last());
                originalTime = lastObject.HitObject.StartTime;
                EditorClock.Seek(composer.EditorBeatmap.HitObjects.Last().StartTime);
            });

            AddStep("select all objects", () => composer.EditorBeatmap.SelectedHitObjects.AddRange(composer.EditorBeatmap.HitObjects));

            AddStep("click last object", () =>
            {
                originalPosition = lastObject.DrawPosition;

                InputManager.MoveMouseTo(lastObject);
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("move mouse downwards", () =>
            {
                InputManager.MoveMouseTo(lastObject, new Vector2(0, lastObject.ScreenSpaceDrawQuad.Height * 4));
                InputManager.ReleaseButton(MouseButton.Left);
            });

            AddAssert("hitobjects not moved columns", () => composer.EditorBeatmap.HitObjects.All(h => ((ManiaHitObject)h).Column == 0));
            AddAssert("hitobjects moved downwards", () => lastObject.DrawPosition.Y - originalPosition.Y > 0);
            AddAssert("hitobject has moved time", () => lastObject.HitObject.StartTime == originalTime + 125);
        }

        [Test]
        public void TestDragOffscreenSelectionVerticallyDownScroll()
        {
            DrawableHitObject lastObject = null;
            double originalTime = 0;
            Vector2 originalPosition = Vector2.Zero;

            setScrollStep(ScrollingDirection.Down);

            AddStep("seek to last object", () =>
            {
                lastObject = this.ChildrenOfType<DrawableHitObject>().Single(d => d.HitObject == composer.EditorBeatmap.HitObjects.Last());
                originalTime = lastObject.HitObject.StartTime;
                EditorClock.Seek(composer.EditorBeatmap.HitObjects.Last().StartTime);
            });

            AddStep("select all objects", () => composer.EditorBeatmap.SelectedHitObjects.AddRange(composer.EditorBeatmap.HitObjects));

            AddStep("click last object", () =>
            {
                originalPosition = lastObject.DrawPosition;

                InputManager.MoveMouseTo(lastObject);
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("move mouse upwards", () =>
            {
                InputManager.MoveMouseTo(lastObject, new Vector2(0, -lastObject.ScreenSpaceDrawQuad.Height * 4));
                InputManager.ReleaseButton(MouseButton.Left);
            });

            AddAssert("hitobjects not moved columns", () => composer.EditorBeatmap.HitObjects.All(h => ((ManiaHitObject)h).Column == 0));
            AddAssert("hitobjects moved upwards", () => originalPosition.Y - lastObject.DrawPosition.Y > 0);
            AddAssert("hitobject has moved time", () => lastObject.HitObject.StartTime == originalTime + 125);
        }

        [Test]
        public void TestDragOffscreenSelectionHorizontally()
        {
            DrawableHitObject lastObject = null;
            Vector2 originalPosition = Vector2.Zero;

            setScrollStep(ScrollingDirection.Down);

            AddStep("seek to last object", () =>
            {
                lastObject = this.ChildrenOfType<DrawableHitObject>().Single(d => d.HitObject == composer.EditorBeatmap.HitObjects.Last());
                EditorClock.Seek(composer.EditorBeatmap.HitObjects.Last().StartTime);
            });

            AddStep("select all objects", () => composer.EditorBeatmap.SelectedHitObjects.AddRange(composer.EditorBeatmap.HitObjects));

            AddStep("click last object", () =>
            {
                originalPosition = lastObject.DrawPosition;

                InputManager.MoveMouseTo(lastObject);
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("move mouse right", () =>
            {
                var firstColumn = composer.Composer.Playfield.GetColumn(0);
                var secondColumn = composer.Composer.Playfield.GetColumn(1);

                InputManager.MoveMouseTo(lastObject, new Vector2(secondColumn.ScreenSpaceDrawQuad.Centre.X - firstColumn.ScreenSpaceDrawQuad.Centre.X + 1, 0));
                InputManager.ReleaseButton(MouseButton.Left);
            });

            AddAssert("hitobjects moved columns", () => composer.EditorBeatmap.HitObjects.All(h => ((ManiaHitObject)h).Column == 1));

            // Todo: They'll move vertically by the height of a note since there's no snapping and the selection point is the middle of the note.
            AddAssert("hitobjects not moved vertically", () => lastObject.DrawPosition.Y - originalPosition.Y <= DefaultNotePiece.NOTE_HEIGHT);
        }

        [Test]
        public void TestDragHoldNoteSelectionVertically()
        {
            setScrollStep(ScrollingDirection.Down);

            AddStep("setup beatmap", () =>
            {
                composer.EditorBeatmap.Clear();
                composer.EditorBeatmap.Add(new HoldNote
                {
                    Column = 1,
                    EndTime = 200
                });
            });

            DrawableHoldNote holdNote = null;

            AddStep("grab hold note", () =>
            {
                holdNote = this.ChildrenOfType<DrawableHoldNote>().Single();
                InputManager.MoveMouseTo(holdNote);
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("move drag upwards", () =>
            {
                InputManager.MoveMouseTo(holdNote, new Vector2(0, -100));
                InputManager.ReleaseButton(MouseButton.Left);
            });

            AddAssert("head note positioned correctly", () => Precision.AlmostEquals(holdNote.ScreenSpaceDrawQuad.BottomLeft, holdNote.Head.ScreenSpaceDrawQuad.BottomLeft));
            AddAssert("tail note positioned correctly", () => Precision.AlmostEquals(holdNote.ScreenSpaceDrawQuad.TopLeft, holdNote.Tail.ScreenSpaceDrawQuad.BottomLeft));

            AddAssert("head blueprint positioned correctly", () => this.ChildrenOfType<EditNotePiece>().ElementAt(0).DrawPosition == holdNote.Head.DrawPosition);
            AddAssert("tail blueprint positioned correctly", () => this.ChildrenOfType<EditNotePiece>().ElementAt(1).DrawPosition == holdNote.Tail.DrawPosition);
        }

        private void setScrollStep(ScrollingDirection direction)
            => AddStep($"set scroll direction = {direction}", () => ((Bindable<ScrollingDirection>)composer.Composer.ScrollingInfo.Direction).Value = direction);

        private partial class TestComposer : CompositeDrawable
        {
            [Cached(typeof(EditorBeatmap))]
            [Cached(typeof(IBeatSnapProvider))]
            public readonly EditorBeatmap EditorBeatmap;

            public readonly ManiaHitObjectComposer Composer;

            public TestComposer()
            {
                InternalChildren = new Drawable[]
                {
                    EditorBeatmap = new EditorBeatmap(new ManiaBeatmap(new StageDefinition(4))
                    {
                        BeatmapInfo = { Ruleset = new ManiaRuleset().RulesetInfo }
                    }),
                    Composer = new ManiaHitObjectComposer(new ManiaRuleset())
                };

                for (int i = 0; i < 10; i++)
                    EditorBeatmap.Add(new Note { StartTime = 125 * i });
            }
        }
    }
}
