// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public partial class TestSceneNotes : OsuTestScene
    {
        [Test]
        public void TestVariousNotes()
        {
            DrawableNote note1 = null;
            DrawableNote note2 = null;
            DrawableHoldNote holdNote1 = null;
            DrawableHoldNote holdNote2 = null;

            AddStep("create notes", () =>
            {
                Child = new FillFlowContainer
                {
                    Clock = new FramedClock(new ManualClock()),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(20),
                    Children = new[]
                    {
                        createNoteDisplay(ScrollingDirection.Down, 1, out note1),
                        createNoteDisplay(ScrollingDirection.Up, 2, out note2),
                        createHoldNoteDisplay(ScrollingDirection.Down, 1, out holdNote1),
                        createHoldNoteDisplay(ScrollingDirection.Up, 2, out holdNote2),
                    }
                };
            });

            AddAssert("note 1 facing downwards", () => verifyAnchors(note1, Anchor.y2));
            AddAssert("note 2 facing upwards", () => verifyAnchors(note2, Anchor.y0));
            AddAssert("hold note 1 facing downwards", () => verifyAnchors(holdNote1, Anchor.y2));
            AddAssert("hold note 2 facing upwards", () => verifyAnchors(holdNote2, Anchor.y0));
        }

        private Drawable createNoteDisplay(ScrollingDirection direction, int identifier, out DrawableNote hitObject)
        {
            var note = new Note { StartTime = 0 };
            note.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            return new ScrollingTestContainer(direction)
            {
                AutoSizeAxes = Axes.Both,
                Child = new NoteContainer(direction, $"note {identifier}, scrolling {direction.ToString().ToLowerInvariant()}")
                {
                    Child = hitObject = new DrawableNote(note) { AccentColour = { Value = Color4.OrangeRed } }
                }
            };
        }

        private Drawable createHoldNoteDisplay(ScrollingDirection direction, int identifier, out DrawableHoldNote hitObject)
        {
            var note = new HoldNote { StartTime = 0, Duration = 5000 };
            note.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            return new ScrollingTestContainer(direction)
            {
                AutoSizeAxes = Axes.Both,
                Child = new NoteContainer(direction, $"hold note {identifier}, scrolling {direction.ToString().ToLowerInvariant()}")
                {
                    Child = hitObject = new DrawableHoldNote(note)
                    {
                        RelativeSizeAxes = Axes.Both,
                        AccentColour = { Value = Color4.OrangeRed },
                    }
                }
            };
        }

        private bool verifyAnchors(DrawableHitObject hitObject, Anchor expectedAnchor)
            => hitObject.Anchor.HasFlagFast(expectedAnchor) && hitObject.Origin.HasFlagFast(expectedAnchor);

        private bool verifyAnchors(DrawableHoldNote holdNote, Anchor expectedAnchor)
            => verifyAnchors((DrawableHitObject)holdNote, expectedAnchor) && holdNote.NestedHitObjects.All(n => verifyAnchors(n, expectedAnchor));

        private partial class NoteContainer : Container
        {
            private readonly Container content;
            protected override Container<Drawable> Content => content;

            private readonly ScrollingDirection direction;

            public NoteContainer(ScrollingDirection direction, string description)
            {
                this.direction = direction;
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(0, 10),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Width = 45,
                            Height = 100,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 1.25f,
                                    Colour = Color4.Green.Opacity(0.5f)
                                },
                                content = new Container { RelativeSizeAxes = Axes.Both }
                            }
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = OsuFont.GetFont(size: 14),
                            Text = description
                        }
                    }
                };
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
                dependencies.CacheAs<IBindable<ManiaAction>>(new Bindable<ManiaAction>());
                return dependencies;
            }

            protected override void Update()
            {
                base.Update();

                foreach (var obj in content.OfType<DrawableHitObject>())
                {
                    if (!(obj.HitObject is IHasDuration endTime))
                        continue;

                    foreach (var nested in obj.NestedHitObjects)
                    {
                        double finalPosition = (nested.HitObject.StartTime - obj.HitObject.StartTime) / endTime.Duration;

                        switch (direction)
                        {
                            case ScrollingDirection.Up:
                                nested.Y = (float)(finalPosition * content.DrawHeight);
                                break;

                            case ScrollingDirection.Down:
                                nested.Y = (float)(-finalPosition * content.DrawHeight);
                                break;
                        }
                    }
                }
            }
        }
    }
}
