// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    /// <summary>
    /// A component of the playfield that captures input and displays input as a drum.
    /// </summary>
    internal class LegacyInputDrum : Container
    {
        private LegacyHalfDrum left;
        private LegacyHalfDrum right;
        private Container content;

        public LegacyInputDrum()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            Child = content = new Container
            {
                Size = new Vector2(180, 200),
                Children = new Drawable[]
                {
                    new Sprite
                    {
                        Texture = skin.GetTexture("taiko-bar-left")
                    },
                    left = new LegacyHalfDrum(false)
                    {
                        Name = "Left Half",
                        RelativeSizeAxes = Axes.Both,
                        RimAction = TaikoAction.LeftRim,
                        CentreAction = TaikoAction.LeftCentre
                    },
                    right = new LegacyHalfDrum(true)
                    {
                        Name = "Right Half",
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.TopRight,
                        Scale = new Vector2(-1, 1),
                        RimAction = TaikoAction.RightRim,
                        CentreAction = TaikoAction.RightCentre
                    }
                }
            };

            // this will be used in the future for stable skin alignment. keeping here for reference.
            const float taiko_bar_y = 0;

            // stable things
            const float ratio = 1.6f;

            // because the right half is flipped, we need to position using width - position to get the true "topleft" origin position
            float negativeScaleAdjust = content.Width / ratio;

            if (skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value >= 2.1m)
            {
                left.Centre.Position = new Vector2(0, taiko_bar_y) * ratio;
                right.Centre.Position = new Vector2(negativeScaleAdjust - 56, taiko_bar_y) * ratio;
                left.Rim.Position = new Vector2(0, taiko_bar_y) * ratio;
                right.Rim.Position = new Vector2(negativeScaleAdjust - 56, taiko_bar_y) * ratio;
            }
            else
            {
                left.Centre.Position = new Vector2(18, taiko_bar_y + 31) * ratio;
                right.Centre.Position = new Vector2(negativeScaleAdjust - 54, taiko_bar_y + 31) * ratio;
                left.Rim.Position = new Vector2(8, taiko_bar_y + 23) * ratio;
                right.Rim.Position = new Vector2(negativeScaleAdjust - 53, taiko_bar_y + 23) * ratio;
            }
        }

        protected override void Update()
        {
            base.Update();

            // Relying on RelativeSizeAxes.Both + FillMode.Fit doesn't work due to the precise pixel layout requirements.
            // This is a bit ugly but makes the non-legacy implementations a lot cleaner to implement.
            content.Scale = new Vector2(DrawHeight / content.Size.Y);
        }

        /// <summary>
        /// A half-drum. Contains one centre and one rim hit.
        /// </summary>
        private class LegacyHalfDrum : Container, IKeyBindingHandler<TaikoAction>
        {
            /// <summary>
            /// The key to be used for the rim of the half-drum.
            /// </summary>
            public TaikoAction RimAction;

            /// <summary>
            /// The key to be used for the centre of the half-drum.
            /// </summary>
            public TaikoAction CentreAction;

            public readonly Sprite Rim;
            public readonly Sprite Centre;

            [Resolved]
            private DrumSampleTriggerSource sampleTriggerSource { get; set; }

            public LegacyHalfDrum(bool flipped)
            {
                Masking = true;

                Children = new Drawable[]
                {
                    Rim = new Sprite
                    {
                        Scale = new Vector2(-1, 1),
                        Origin = flipped ? Anchor.TopLeft : Anchor.TopRight,
                        Alpha = 0,
                    },
                    Centre = new Sprite
                    {
                        Alpha = 0,
                        Origin = flipped ? Anchor.TopRight : Anchor.TopLeft,
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                Rim.Texture = skin.GetTexture(@"taiko-drum-outer");
                Centre.Texture = skin.GetTexture(@"taiko-drum-inner");
            }

            public bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
            {
                Drawable target = null;

                if (e.Action == CentreAction)
                {
                    target = Centre;
                    sampleTriggerSource.Play(HitType.Centre);
                }
                else if (e.Action == RimAction)
                {
                    target = Rim;
                    sampleTriggerSource.Play(HitType.Rim);
                }

                if (target != null)
                {
                    const float alpha_amount = 1;

                    const float down_time = 80;
                    const float up_time = 50;

                    target.Animate(
                        t => t.FadeTo(Math.Min(target.Alpha + alpha_amount, 1), down_time)
                    ).Then(
                        t => t.FadeOut(up_time)
                    );
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<TaikoAction> e)
            {
            }
        }
    }
}
