// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
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
                        new LaneCover(0.5f, false)
                        {
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

            public LaneCover(float initialCoverage, bool reversed)
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
                Coverage = initialCoverage;
                Reversed = reversed;
            }

            private float coverage;

            public float Coverage
            {
                set
                {
                    filled.Height = value;
                    gradient.Y = reversed ? 1 - value - gradient.Height : value;
                    coverage = value;
                }
            }

            private bool reversed;

            public bool Reversed
            {
                set
                {
                    filled.Anchor = value ? Anchor.BottomLeft : Anchor.TopLeft;
                    filled.Origin = value ? Anchor.BottomLeft : Anchor.TopLeft;
                    gradient.Colour = ColourInfo.GradientVertical(
                        Color4.White.Opacity(value ? 0f : 1f),
                        Color4.White.Opacity(value ? 1f : 0f)
                    );

                    reversed = value;
                    Coverage = coverage; //re-apply coverage to update visuals
                }
            }

            [BackgroundDependencyLoader]
            private void load(ManiaRulesetConfigManager configManager)
            {
                var scrollDirection = configManager.GetBindable<ManiaScrollingDirection>(ManiaRulesetSetting.ScrollDirection);

                if (scrollDirection.Value == ManiaScrollingDirection.Up)
                    Reversed = !reversed;
            }
        }
    }
}
