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

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public partial class LegacySwell : Container, ISkinnableSwell
    {
        private Container bodyContainer = null!;
        private Sprite spinnerCircle = null!;
        private Sprite shrinkingRing = null!;
        private Sprite clearAnimation = null!;
        private ISample? clearSample;
        private LegacySpriteText remainingHitsCountdown = null!;

        private bool samplePlayed;

        public LegacySwell()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, SkinManager skinManager)
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = new Vector2(200f, 100f),

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
                            shrinkingRing = new Sprite
                            {
                                Texture = skin.GetTexture("spinner-approachcircle") ?? skinManager.DefaultClassicSkin.GetTexture("spinner-approachcircle"),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Scale = Vector2.One,
                            },
                            remainingHitsCountdown = new LegacySpriteText(LegacyFont.Combo)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Position = new Vector2(0f, 165f),
                                Scale = Vector2.One,
                            },
                        }
                    },
                    clearAnimation = new Sprite
                    {
                        // File extension is included here because of a GetTexture limitation, see #21543
                        Texture = skin.GetTexture("spinner-osu.png"),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Position = new Vector2(0f, -165f),
                        Scale = new Vector2(0.3f),
                        Alpha = 0,
                    },
                }
            };

            clearSample = skin.GetSample(new SampleInfo("spinner-osu"));
        }

        public void AnimateSwellProgress(DrawableTaikoHitObject<Swell> swell, int numHits, SkinnableDrawable mainPiece)
        {
            remainingHitsCountdown.Text = $"{swell.HitObject.RequiredHits - numHits}";
            spinnerCircle.RotateTo(180f * numHits, 1000, Easing.OutQuint);
        }

        public void AnimateSwellCompletion(ArmedState state, SkinnableDrawable mainPiece)
        {
            const double clear_transition_duration = 300;

            bodyContainer.FadeOut(clear_transition_duration, Easing.OutQuad);

            if (state == ArmedState.Hit)
            {
                if (!samplePlayed)
                {
                    clearSample?.Play();
                    samplePlayed = true;
                }

                clearAnimation
                    .FadeIn(clear_transition_duration, Easing.InQuad)
                    .ScaleTo(0.8f, clear_transition_duration, Easing.InQuad)
                    .Delay(700).FadeOut(200, Easing.OutQuad);
            }
        }

        public void AnimateSwellStart(DrawableTaikoHitObject<Swell> swell, SkinnableDrawable mainPiece)
        {
            if (swell.IsHit == false)
            {
                remainingHitsCountdown.Text = $"{swell.HitObject.RequiredHits}";
                samplePlayed = false;
            }

            const double body_transition_duration = 100;

            mainPiece.FadeOut(body_transition_duration);
            bodyContainer.FadeIn(body_transition_duration);
            shrinkingRing.ResizeTo(0.1f, swell.HitObject.Duration);
        }
    }
}
