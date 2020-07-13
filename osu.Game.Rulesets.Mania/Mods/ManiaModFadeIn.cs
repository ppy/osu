// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModFadeIn : Mod, IApplicableToDrawableRuleset<ManiaHitObject>
    {
        public override string Name => "Fade In";
        public override string Acronym => "FI";
        public override IconUsage? Icon => OsuIcon.ModHidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => @"Keys appear out of nowhere!";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModFlashlight<ManiaHitObject>) };

        public void ApplyToDrawableRuleset(DrawableRuleset<ManiaHitObject> drawableRuleset)
        {
            ManiaPlayfield maniaPlayfield = (ManiaPlayfield)drawableRuleset.Playfield;

            foreach (Column column in maniaPlayfield.Stages.SelectMany(stage => stage.Columns))
            {
                ((BufferedContainer)column.HitObjectArea.HitObjectContainer.Parent).Add(new LaneCover(false)
                {
                    RelativeSizeAxes = Axes.Both,
                    SizeFilled = 0.5f,
                    SizeGradient = 0.25f
                });
            }
        }

        private class LaneCover : CompositeDrawable
        {
            private readonly Box gradient;
            private readonly Box filled;
            private readonly bool reversed;

            public LaneCover(bool reversed)
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
                        Colour = ColourInfo.GradientVertical(Color4.White.Opacity(1f), Color4.White.Opacity(0f))
                    },
                    filled = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                };

                if (reversed)
                {
                    gradient.Colour = ColourInfo.GradientVertical(Color4.White.Opacity(0f), Color4.White.Opacity(1f));
                    filled.Anchor = Anchor.BottomLeft;
                    filled.Origin = Anchor.BottomLeft;
                }

                this.reversed = reversed;
            }

            public float SizeFilled
            {
                set
                {
                    filled.Height = value;
                    if (!reversed)
                        gradient.Y = value;
                }
            }

            public float SizeGradient
            {
                set
                {
                    gradient.Height = value;
                    if (reversed)
                        gradient.Y = 1 - value - filled.Height;
                }
            }
        }
    }
}
