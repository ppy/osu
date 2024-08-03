// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public abstract partial class ArgonCirclePiece : BeatSyncedContainer, IHasAccentColour
    {
        public const float ICON_SIZE = 20 / 70f;

        private const double pre_beat_transition_time = 80;

        private const float kiai_flash_opacity = 0.15f;

        private ColourInfo argonAccentColour;

        /// <summary>
        /// The colour of the inner circle and outer glows.
        /// </summary>
        public ColourInfo ArgonAccentColour
        {
            get => argonAccentColour;
            set
            {
                argonAccentColour = value;

                ring.Colour = ArgonAccentColour.MultiplyAlpha(0.5f);
                ring2.Colour = ArgonAccentColour;
            }
        }

        public Color4 AccentColour
        {
            get => ArgonAccentColour.AverageColour;
            set => ArgonAccentColour = value;
        }

        [Resolved]
        private DrawableHitObject drawableHitObject { get; set; } = null!;

        private readonly Drawable flash;

        private readonly RingPiece ring;
        private readonly RingPiece ring2;

        protected ArgonCirclePiece()
        {
            RelativeSizeAxes = Axes.Both;

            EarlyActivationMilliseconds = pre_beat_transition_time;

            AddRangeInternal(new[]
            {
                new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(0, 0, 0, 190)
                },
                ring = new RingPiece(20 / 70f),
                ring2 = new RingPiece(5 / 70f),
                flash = new Circle
                {
                    Name = "Flash layer",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            drawableHitObject.ApplyCustomUpdateState += updateStateTransforms;
            updateStateTransforms(drawableHitObject, drawableHitObject.State.Value);
        }

        private void updateStateTransforms(DrawableHitObject h, ArmedState state)
        {
            if (h.HitObject is not Hit)
                return;

            switch (state)
            {
                case ArmedState.Hit:
                    using (BeginAbsoluteSequence(h.HitStateUpdateTime))
                    {
                        flash.FadeTo(0.9f).FadeOut(500, Easing.OutQuint);
                    }

                    break;
            }
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (!effectPoint.KiaiMode)
                return;

            if (drawableHitObject.State.Value == ArmedState.Idle)
            {
                flash
                    .FadeTo(kiai_flash_opacity)
                    .Then()
                    .FadeOut(timingPoint.BeatLength * 0.75, Easing.OutSine);
            }
        }
    }
}
