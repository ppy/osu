// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Framework.Audio.Sample;
using osu.Game.Audio;
using osuTK;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Extensions.ObjectExtensions;
using System;
using System.Globalization;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public partial class LegacySwell : Container
    {
        private const float scale_adjust = 768f / 480;

        private DrawableSwell drawableSwell = null!;

        private Container bodyContainer = null!;
        private Sprite warning = null!;
        private Sprite spinnerCircle = null!;
        private Sprite approachCircle = null!;
        private Sprite clearAnimation = null!;
        private ISample? clearSample;
        private LegacySpriteText remainingHitsText = null!;

        private bool samplePlayed;

        public LegacySwell()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject hitObject, ISkinSource skin)
        {
            Children = new Drawable[]
            {
                warning = new Sprite
                {
                    Texture = skin.GetTexture("spinner-warning"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = skin.GetTexture("spinner-warning") != null ? Vector2.One : new Vector2(0.18f),
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Position = new Vector2(250f, 100f), // ballparked to be horizontally centred on 4:3 resolution

                    Children = new Drawable[]
                    {
                        bodyContainer = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,

                            Children = new Drawable[]
                            {
                                spinnerCircle = new Sprite
                                {
                                    Texture = skin.GetTexture("spinner-circle"),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Scale = new Vector2(0.8f),
                                },
                                approachCircle = new Sprite
                                {
                                    Texture = skin.GetTexture("spinner-approachcircle"),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Scale = new Vector2(1.86f * 0.8f),
                                    Alpha = 0.8f,
                                },
                                remainingHitsText = new LegacySpriteText(LegacyFont.Score)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Position = new Vector2(0f, 130f),
                                    Scale = Vector2.One,
                                },
                            }
                        },
                        clearAnimation = new Sprite
                        {
                            Texture = skin.GetTexture("spinner-osu"),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Y = -40,
                        },
                    },
                },
            };

            drawableSwell = (DrawableSwell)hitObject;
            drawableSwell.UpdateHitProgress += animateSwellProgress;
            drawableSwell.ApplyCustomUpdateState += updateStateTransforms;
            clearSample = skin.GetSample(new SampleInfo("spinner-osu"));
        }

        private void animateSwellProgress(int numHits, int requiredHits)
        {
            int remainingHits = requiredHits - numHits;
            remainingHitsText.Text = remainingHits.ToString(CultureInfo.InvariantCulture);
            remainingHitsText.ScaleTo(1.6f - (0.6f * ((float)remainingHits / requiredHits)), 60, Easing.Out);

            spinnerCircle.ClearTransforms();
            spinnerCircle
                .RotateTo(180f * numHits, 1000, Easing.OutQuint)
                .ScaleTo(Math.Min(0.94f, spinnerCircle.Scale.X + 0.02f))
                .ScaleTo(0.8f, 400, Easing.Out);
        }

        private void updateStateTransforms(DrawableHitObject drawableHitObject, ArmedState state)
        {
            if (!(drawableHitObject is DrawableSwell))
                return;

            Swell swell = drawableSwell.HitObject;

            using (BeginAbsoluteSequence(swell.StartTime))
            {
                if (state == ArmedState.Idle)
                {
                    remainingHitsText.Text = $"{swell.RequiredHits}";
                    samplePlayed = false;
                }

                const double body_transition_duration = 200;

                warning.FadeOut(body_transition_duration);
                bodyContainer.FadeIn(body_transition_duration);
                approachCircle.ResizeTo(0.1f * 0.8f, swell.Duration);
            }

            using (BeginAbsoluteSequence(drawableSwell.HitStateUpdateTime))
            {
                const double clear_transition_duration = 300;
                const double clear_fade_in = 120;

                bodyContainer.FadeOut(clear_transition_duration, Easing.OutQuad);
                spinnerCircle.ScaleTo(spinnerCircle.Scale.X + 0.05f, clear_transition_duration, Easing.OutQuad);

                if (state == ArmedState.Hit)
                {
                    if (!samplePlayed)
                    {
                        clearSample?.Play();
                        samplePlayed = true;
                    }

                    clearAnimation
                        .MoveToOffset(new Vector2(0, -90 * scale_adjust), clear_fade_in * 2, Easing.Out)
                        .ScaleTo(0.4f)
                        .ScaleTo(1f, clear_fade_in * 2, Easing.Out)
                        .FadeIn()
                        .Delay(clear_fade_in * 3)
                        .FadeOut(clear_fade_in * 2.5);
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableSwell.IsNotNull())
            {
                drawableSwell.UpdateHitProgress -= animateSwellProgress;
                drawableSwell.ApplyCustomUpdateState -= updateStateTransforms;
            }
        }
    }
}
