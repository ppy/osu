// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public class TestSceneNotePiece : ManiaSkinnableTestScene
    {
        [Cached(typeof(DrawableHitObject))]
        private DrawableHitObject testDrawableObject = new DrawableNote { AccentColour = { Value = Color4.Orange } };

        [Test]
        public void TestNotePiece() => createTest(ManiaSkinComponents.Note);

        [Test]
        public void TestHoldNoteHeadPiece() => createTest(ManiaSkinComponents.HoldNoteHead);

        [Test]
        public void TestHoldNoteTailPiece() => createTest(ManiaSkinComponents.HoldNoteTail);

        private void createTest(ManiaSkinComponents component) => AddStep("create note piece", () =>
        {
            SetContents(skin => new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Children = new Drawable[]
                {
                    new ColumnTestContainer(0, ManiaAction.Key1)
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = 80,
                        Child = new SkinnableDrawable(new ManiaSkinComponent(component), _ => new DefaultNotePiece())
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    },
                    new ColumnTestContainer(1, ManiaAction.Key2)
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = 80,
                        Child = new SkinnableDrawable(new ManiaSkinComponent(component), _ => new DefaultNotePiece())
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                }
            });
        });
    }
}
