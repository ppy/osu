// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public abstract class ArgonCirclePiece : BeatSyncedContainer
    {
        private const double pre_beat_transition_time = 80;

        private const float flash_opacity = 0.3f;

        private ColourInfo accentColour;

        /// <summary>
        /// The colour of the inner circle and outer glows.
        /// </summary>
        public ColourInfo AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                ring.Colour = AccentColour.MultiplyAlpha(0.5f);
                ring2.Colour = AccentColour;
            }
        }

        /// <summary>
        /// Whether Kiai mode effects are enabled for this circle piece.
        /// </summary>
        public bool KiaiMode { get; set; }

        public Box FlashBox;

        private readonly RingPiece ring;
        private readonly RingPiece ring2;

        protected ArgonCirclePiece()
        {
            RelativeSizeAxes = Axes.Both;

            EarlyActivationMilliseconds = pre_beat_transition_time;

            AddRangeInternal(new Drawable[]
            {
                new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(0, 22, 30, 190)
                },
                ring = new RingPiece(20 / 70f),
                ring2 = new RingPiece(5 / 70f),
                new CircularContainer
                {
                    Name = "Flash layer",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new[]
                    {
                        FlashBox = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                            Blending = BlendingParameters.Additive,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    },
                },
            });
        }

        [Resolved]
        private DrawableHitObject drawableHitObject { get; set; } = null!;

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (!effectPoint.KiaiMode)
                return;

            if (drawableHitObject.State.Value == ArmedState.Idle)
            {
                FlashBox
                    .FadeTo(flash_opacity)
                    .Then()
                    .FadeOut(timingPoint.BeatLength * 0.75, Easing.OutSine);
            }
        }
    }
}
