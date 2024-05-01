// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Simple <see cref="ResumeOverlay"/> that resumes after a short delay.
    /// </summary>
    public partial class DelayedResumeOverlay : ResumeOverlay
    {
        // todo: this shouldn't define its own colour provider, but nothing in DrawableRuleset guarantees this, so let's do it locally for now.
        // (of note, Player does cache one but any test which uses a DrawableRuleset without Player will fail without this).
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private const float outer_size = 200;
        private const float inner_size = 150;
        private const float progress_stroke_width = 7;
        private const float progress_size = inner_size + progress_stroke_width / 2f;

        private const double countdown_time = 2000;

        private const int total_count = 3;

        protected override LocalisableString Message => string.Empty;

        private int? countdownCount;
        private double countdownStartTime;
        private bool countdownComplete;

        private Drawable outerContent = null!;
        private Container innerContent = null!;

        private Container countdownComponents = null!;
        private Drawable countdownBackground = null!;
        private SpriteText countdownText = null!;
        private CircularProgress countdownProgress = null!;

        private Sample? sampleCountdown;

        public DelayedResumeOverlay()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            Add(outerContent = new Circle
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(outer_size),
                Colour = colourProvider.Background6,
            });

            Add(innerContent = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    countdownBackground = new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(inner_size),
                        Colour = colourProvider.Background4,
                    },
                    countdownComponents = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            countdownProgress = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(progress_size),
                                InnerRadius = progress_stroke_width / progress_size,
                                RoundedCaps = true
                            },
                            countdownText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                UseFullGlyphHeight = false,
                                AlwaysPresent = true,
                                Font = OsuFont.Torus.With(size: 70, weight: FontWeight.Light)
                            }
                        }
                    }
                }
            });

            sampleCountdown = audio.Samples.Get(@"Gameplay/resume-countdown");
        }

        protected override void PopIn()
        {
            this.FadeIn();

            // The transition effects.
            outerContent.FadeIn().ScaleTo(Vector2.Zero).Then().ScaleTo(Vector2.One, 200, Easing.OutQuint);
            innerContent.FadeIn().ScaleTo(Vector2.Zero).Then().ScaleTo(Vector2.One, 400, Easing.OutElasticHalf);
            countdownComponents.FadeOut().Delay(50).FadeTo(1, 100);

            countdownProgress.Progress = 0;

            // Reset states for various components.
            countdownBackground.FadeIn();
            countdownText.FadeIn();
            countdownText.Text = string.Empty;
            countdownProgress.FadeIn().ScaleTo(1);

            countdownComplete = false;
            countdownCount = null;
            countdownStartTime = Time.Current + 200;
        }

        protected override void PopOut()
        {
            this.Delay(300).FadeOut();

            outerContent.FadeOut();
            countdownBackground.FadeOut();
            countdownText.FadeOut();

            if (countdownComplete)
            {
                countdownProgress.ScaleTo(2f, 300, Easing.OutQuint);
                countdownProgress.FadeOut(300, Easing.OutQuint);
            }
            else
                countdownProgress.FadeOut();
        }

        protected override void Update()
        {
            base.Update();
            updateCountdown();
        }

        private void updateCountdown()
        {
            if (State.Value == Visibility.Hidden || countdownComplete || Time.Current < countdownStartTime)
                return;

            double amountTimePassed = Math.Clamp((Time.Current - countdownStartTime) / countdown_time, 0, countdown_time);
            int newCount = Math.Clamp(total_count - (int)Math.Floor(amountTimePassed * total_count), 0, total_count);

            countdownProgress.Progress = amountTimePassed;
            countdownProgress.InnerRadius = progress_stroke_width / progress_size / countdownProgress.Scale.X;

            Alpha = 0.2f + 0.8f * newCount / total_count;

            if (countdownCount != newCount)
            {
                if (newCount > 0)
                {
                    countdownText.Text = Math.Max(1, newCount).ToString();
                    countdownText.ScaleTo(0.25f).Then().ScaleTo(1, 200, Easing.OutQuint);
                    outerContent.Delay(25).Then().ScaleTo(1.05f, 100).Then().ScaleTo(1f, 200, Easing.Out);

                    countdownBackground.FlashColour(colourProvider.Background3, 400, Easing.Out);
                }

                var chan = sampleCountdown?.GetChannel();

                if (chan != null)
                {
                    chan.Frequency.Value = newCount == 0 ? 0.5f : 1;
                    chan.Play();
                }
            }

            countdownCount = newCount;

            if (countdownCount == 0)
            {
                countdownComplete = true;
                Resume();
            }
        }
    }
}
