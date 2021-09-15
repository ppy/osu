// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public class TestSceneHoldNoteBody : ManiaSkinnableTestScene
    {
        [Cached(typeof(DrawableHitObject))]
        private DrawableHoldNote testDrawableObject = new DrawableHoldNote { AccentColour = { Value = Color4.Orange } };

        [SetUp]
        public void Setup() => Schedule(() =>
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
                        Child = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.HoldNoteBody), _ => new DefaultBodyPiece
                        {
                            RelativeSizeAxes = Axes.Both,
                        })
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                    },
                    new ColumnTestContainer(1, ManiaAction.Key2)
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = 80,
                        Child = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.HoldNoteBody), _ => new DefaultBodyPiece
                        {
                            RelativeSizeAxes = Axes.Both,
                        })
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                    }
                }
            });
        });

        [Test]
        public void TestHoldNote()
        {
            AddToggleStep("toggle hitting", v => ((Bindable<bool>)testDrawableObject.IsHitting).Value = v);
        }
    }
}
