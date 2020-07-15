// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModHidden : ModHidden, IApplicableToDrawableRuleset<ManiaHitObject>
    {
        public override string Description => @"Keys fade out before you hit them!";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(ModFlashlight<ManiaHitObject>) };

        public void ApplyToDrawableRuleset(DrawableRuleset<ManiaHitObject> drawableRuleset)
        {
            ManiaPlayfield maniaPlayfield = (ManiaPlayfield)drawableRuleset.Playfield;

            foreach (Column column in maniaPlayfield.Stages.SelectMany(stage => stage.Columns))
            {
                HitObjectContainer hoc = column.HitObjectArea.HitObjectContainer;
                Container hocParent = (Container)hoc.Parent;

                hocParent.Remove(hoc);
                hocParent.Add(new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        hoc,
                        new LaneCover
                        {
                            Coverage = 0.5f,
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                });
            }
        }

        private class LaneCover : CompositeDrawable
        {
            private readonly Box gradient;
            private readonly Box filled;
            private bool reversed;
            private readonly IBindable<ScrollingDirection> scrollDirection = new Bindable<ScrollingDirection>();

            public LaneCover()
            {
                Blending = new BlendingParameters
                {
                    RGBEquation = BlendingEquation.Add,
                    Source = BlendingType.Zero,
                    Destination = BlendingType.One,
                    AlphaEquation = BlendingEquation.Add,
                    SourceAlpha = BlendingType.Zero,
                    DestinationAlpha = BlendingType.OneMinusSrcAlpha
                };

                InternalChildren = new Drawable[]
                {
                    gradient = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.Both,
                        Height = 0.25f
                    },
                    filled = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(IScrollingInfo scrollingInfo)
            {
                scrollDirection.BindTo(scrollingInfo.Direction);
                scrollDirection.BindValueChanged(onScrollDirectionChanged, true);
            }

            private void updateCoverage()
            {
                filled.Anchor = reversed ? Anchor.BottomLeft : Anchor.TopLeft;
                filled.Origin = reversed ? Anchor.BottomLeft : Anchor.TopLeft;
                filled.Height = coverage;

                gradient.Y = reversed ? 1 - filled.Height - gradient.Height : coverage;
                gradient.Colour = ColourInfo.GradientVertical(
                    Color4.White.Opacity(reversed ? 0f : 1f),
                    Color4.White.Opacity(reversed ? 1f : 0f)
                );
            }

            private void onScrollDirectionChanged(ValueChangedEvent<ScrollingDirection> valueChangedEvent)
            {
                reversed = valueChangedEvent.NewValue == ScrollingDirection.Up;
                updateCoverage();
            }

            private float coverage;

            public float Coverage
            {
                set
                {
                    if (coverage == value)
                        return;

                    coverage = value;

                    updateCoverage();
                }
            }
        }
    }
}
