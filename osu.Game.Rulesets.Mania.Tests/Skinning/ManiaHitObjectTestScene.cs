// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    /// <summary>
    /// A test scene for a mania hitobject.
    /// </summary>
    public abstract class ManiaHitObjectTestScene : ManiaSkinnableTestScene
    {
        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            SetContents(() => new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Height = 0.7f,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    new ColumnTestContainer(0, ManiaAction.Key1, true)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Y,
                        Width = 80,
                        Child = new ScrollingHitObjectContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                        }.With(c =>
                        {
                            c.Add(CreateHitObject().With(h =>
                            {
                                h.HitObject.StartTime = START_TIME;
                                h.AccentColour.Value = Color4.Orange;
                            }));
                        })
                    },
                    new ColumnTestContainer(1, ManiaAction.Key2, true)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Y,
                        Width = 80,
                        Child = new ScrollingHitObjectContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                        }.With(c =>
                        {
                            c.Add(CreateHitObject().With(h =>
                            {
                                h.HitObject.StartTime = START_TIME;
                                h.AccentColour.Value = Color4.Orange;
                            }));
                        })
                    },
                }
            });
        });

        protected abstract DrawableManiaHitObject CreateHitObject();
    }
}
