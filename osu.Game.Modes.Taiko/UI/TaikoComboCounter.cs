// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.Modes.UI;
using OpenTK;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using OpenTK.Graphics;
using osu.Game.Modes.Objects.Drawables;
using System;

namespace osu.Game.Modes.Taiko.UI
{
    /// <summary>
    /// Allows tint and scaling animations. Used in osu!taiko.
    /// </summary>
    public class TaikoComboCounter : ComboCounter
    {
        protected virtual EasingTypes AnimationEasing => EasingTypes.None;
        protected virtual float ScaleFactor => 2;
        protected virtual int AnimationDuration => 300;
        protected virtual bool CanAnimateWhenBackwards => false;

        private BufferedContainer glowContainer;
        private OsuSpriteText glowText;

        public override float TextSize
        {
            get { return base.TextSize; }

            set
            {
                base.TextSize = value;

                if (glowText != null)
                    glowText.TextSize = value;
            }
        }

        public TaikoComboCounter()
        {
            Add(glowContainer = new BufferedContainer()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                BlurSigma = new Vector2(20),
                CacheDrawnFrameBuffer = true,

                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(3),

                BlendingMode = BlendingMode.Additive,

                Depth = 1,

                Children = new[]
                {
                    new Container()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,

                        AutoSizeAxes = Axes.Both,

                        Children = new[]
                        {
                            glowText = new OsuSpriteText()
                            {
                                Anchor = Anchor,
                                Origin = Origin,

                                Font = "Venera",

                                Colour = new Color4(17, 136, 170, 255)
                            }
                        }
                    }
                }
            });

            TextSize = 14f;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DisplayedCountSpriteText.Font = "Venera";
            DisplayedCountSpriteText.Anchor = Anchor;
            DisplayedCountSpriteText.Origin = Origin;
        }

        protected virtual void TransformAnimate(ulong newValue)
        {
            DisplayedCountSpriteText.Text = FormatCount(newValue);
            DisplayedCountSpriteText.ScaleTo(new Vector2(1, ScaleFactor));
            DisplayedCountSpriteText.ScaleTo(new Vector2(1, 1), AnimationDuration, AnimationEasing);

            glowText.Text = FormatCount(newValue);
            glowContainer.ScaleTo(new Vector2(1, ScaleFactor));
            glowContainer.ScaleTo(new Vector2(1, 1), AnimationDuration, AnimationEasing);
            glowContainer.ForceRedraw();
        }

        protected virtual void TransformNotAnimate(ulong newValue)
        {
            DisplayedCountSpriteText.Text = FormatCount(newValue);
            DisplayedCountSpriteText.ScaleTo(1);

            glowText.Text = FormatCount(newValue);
            glowContainer.ScaleTo(1);
        }

        protected override void OnDisplayedCountRolling(ulong currentValue, ulong newValue)
        {
            if (newValue == 0)
                DisplayedCountSpriteText.FadeOut(FadeOutDuration);
            else
                DisplayedCountSpriteText.Show();

            TransformNotAnimate(newValue);
        }

        protected override void OnDisplayedCountChange(ulong newValue)
        {
            DisplayedCountSpriteText.FadeTo(newValue == 0 ? 0 : 1);

            TransformNotAnimate(newValue);
        }

        protected override void OnDisplayedCountIncrement(ulong newValue)
        {
            DisplayedCountSpriteText.Show();

            TransformAnimate(newValue);
        }
    }
}
