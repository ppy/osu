// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
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
        private static readonly Vector2 swell_display_position = new Vector2(250f, 100f);

        private DrawableSwell drawableSwell = null!;

        private Container bodyContainer = null!;
        private Sprite warning = null!;
        private Sprite spinnerCircle = null!;
        private Sprite approachCircle = null!;
        private Sprite clearAnimation = null!;
        private SkinnableSound clearSample = null!;
        private LegacySpriteText remainingHitsText = null!;

        private bool samplePlayed;

        private int numHits;

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
                    Position = swell_display_position, // ballparked to be horizontally centred on 4:3 resolution

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
                clearSample = new SkinnableSound(new SampleInfo("spinner-osu")),
            };

            drawableSwell = (DrawableSwell)hitObject;
            drawableSwell.UpdateHitProgress += animateSwellProgress;
            drawableSwell.ApplyCustomUpdateState += updateStateTransforms;
        }

        private void animateSwellProgress(int numHits)
        {
            this.numHits = numHits;
            remainingHitsText.Text = (drawableSwell.HitObject.RequiredHits - numHits).ToString(CultureInfo.InvariantCulture);
            spinnerCircle.Scale = new Vector2(Math.Min(0.94f, spinnerCircle.Scale.X + 0.02f));
        }

        protected override void Update()
        {
            base.Update();

            int requiredHits = drawableSwell.HitObject.RequiredHits;
            int remainingHits = requiredHits - numHits;
            remainingHitsText.Scale = new Vector2((float)Interpolation.DampContinuously(
                remainingHitsText.Scale.X, 1.6f - (0.6f * ((float)remainingHits / requiredHits)), 17.5, Math.Abs(Time.Elapsed)));

            spinnerCircle.Rotation = (float)Interpolation.DampContinuously(spinnerCircle.Rotation, 180f * numHits, 130, Math.Abs(Time.Elapsed));
            spinnerCircle.Scale = new Vector2((float)Interpolation.DampContinuously(
                spinnerCircle.Scale.X, 0.8f, 120, Math.Abs(Time.Elapsed)));
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

                warning.MoveTo(swell_display_position, body_transition_duration)
                       .ScaleTo(3, body_transition_duration, Easing.Out)
                       .FadeOut(body_transition_duration);

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
                        clearSample.Play();
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
