// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
        protected List<LaneCover> laneCovers = new List<LaneCover>();

        public virtual void ApplyToDrawableRuleset(DrawableRuleset<ManiaHitObject> drawableRuleset)
        {
            ManiaPlayfield maniaPlayfield = (ManiaPlayfield)drawableRuleset.Playfield;

            foreach (Column column in maniaPlayfield.Stages.SelectMany(stage => stage.Columns))
            {
                HitObjectContainer hoc = column.HitObjectArea.HitObjectContainer;
                Container hocParent = (Container)hoc.Parent;

                LaneCover laneCover;

                hocParent.Remove(hoc);
                hocParent.Add(new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        hoc,
                        laneCover = new LaneCover
                        {
                            Coverage = 0.5f,
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre
                        }
                    }
                });

                laneCovers.Add(laneCover);
            }
        }

        protected class LaneCover : CompositeDrawable
        {
            private readonly Box gradient;
            private readonly Box filled;
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
                        Height = 0.25f,
                        Colour = ColourInfo.GradientVertical(
                            Color4.White.Opacity(0f),
                            Color4.White.Opacity(1f)
                        )
                    },
                    filled = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(IScrollingInfo scrollingInfo)
            {
                scrollDirection.BindTo(scrollingInfo.Direction);
                scrollDirection.BindValueChanged(onScrollDirectionChanged, true);
            }

            private void onScrollDirectionChanged(ValueChangedEvent<ScrollingDirection> valueChangedEvent)
            {
                bool isUpscroll = valueChangedEvent.NewValue == ScrollingDirection.Up;
                Rotation = isUpscroll ? 180f : 0f;
            }

            public float Coverage
            {
                set
                {
                    filled.Height = value;
                    gradient.Y = 1 - filled.Height - gradient.Height;
                }
            }
        }
    }
}
