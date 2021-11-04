// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Game.Rulesets.UI;
using System;
using System.Collections.Generic;
using ManagedBass.Fx;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Audio.Effects;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Manage the animation to be applied when a player fails.
    /// Single use and automatically disposed after use.
    /// </summary>
    public class FailAnimation : Container
    {
        public Action OnComplete;

        private readonly DrawableRuleset drawableRuleset;
        private readonly BindableDouble trackFreq = new BindableDouble(1);

        private Container filters;

        private Box redFlashLayer;

        private Track track;

        private AudioFilter failLowPassFilter;
        private AudioFilter failHighPassFilter;

        private const float duration = 2500;

        private Sample failSample;

        [Resolved]
        private OsuConfigManager config { get; set; }

        protected override Container<Drawable> Content { get; } = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
        };

        public FailAnimation(DrawableRuleset drawableRuleset)
        {
            this.drawableRuleset = drawableRuleset;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, IBindable<WorkingBeatmap> beatmap)
        {
            track = beatmap.Value.Track;
            failSample = audio.Samples.Get(@"Gameplay/failsound");

            AddRangeInternal(new Drawable[]
            {
                filters = new Container
                {
                    Children = new Drawable[]
                    {
                        failLowPassFilter = new AudioFilter(audio.TrackMixer),
                        failHighPassFilter = new AudioFilter(audio.TrackMixer, BQFType.HighPass),
                    },
                },
                Content,
                redFlashLayer = new Box
                {
                    Colour = Color4.Red.Opacity(0.6f),
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingParameters.Additive,
                    Depth = float.MinValue,
                    Alpha = 0
                },
            });
        }

        private bool started;

        /// <summary>
        /// Start the fail animation playing.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if started more than once.</exception>
        public void Start()
        {
            if (started) throw new InvalidOperationException("Animation cannot be started more than once.");

            started = true;

            this.TransformBindableTo(trackFreq, 0, duration).OnComplete(_ =>
            {
                RemoveFilters();
                OnComplete?.Invoke();
            });

            failHighPassFilter.CutoffTo(300);
            failLowPassFilter.CutoffTo(300, duration, Easing.OutCubic);
            failSample.Play();

            track.AddAdjustment(AdjustableProperty.Frequency, trackFreq);

            applyToPlayfield(drawableRuleset.Playfield);
            drawableRuleset.Playfield.HitObjectContainer.FadeOut(duration / 2);

            if (config.Get<bool>(OsuSetting.FadePlayfieldWhenHealthLow))
                redFlashLayer.FadeOutFromOne(1000);

            Content.Masking = true;

            Content.Add(new Box
            {
                Colour = Color4.Black,
                RelativeSizeAxes = Axes.Both,
                Depth = float.MaxValue
            });

            Content.ScaleTo(0.85f, duration, Easing.OutQuart);
            Content.RotateTo(1, duration, Easing.OutQuart);
            Content.FadeColour(Color4.Gray, duration);
        }

        public void RemoveFilters()
        {
            if (filters.Parent == null)
                return;

            RemoveInternal(filters);
            filters.Dispose();

            track?.RemoveAdjustment(AdjustableProperty.Frequency, trackFreq);
        }

        protected override void Update()
        {
            base.Update();

            if (!started)
                return;

            applyToPlayfield(drawableRuleset.Playfield);
        }

        private readonly List<DrawableHitObject> appliedObjects = new List<DrawableHitObject>();

        private void applyToPlayfield(Playfield playfield)
        {
            double failTime = playfield.Time.Current;

            foreach (var nested in playfield.NestedPlayfields)
                applyToPlayfield(nested);

            foreach (DrawableHitObject obj in playfield.HitObjectContainer.AliveObjects)
            {
                if (appliedObjects.Contains(obj))
                    continue;

                float rotation = RNG.NextSingle(-90, 90);
                Vector2 originalPosition = obj.Position;
                Vector2 originalScale = obj.Scale;

                dropOffScreen(obj, failTime, rotation, originalScale, originalPosition);

                // need to reapply the fail drop after judgement state changes
                obj.ApplyCustomUpdateState += (o, _) => dropOffScreen(obj, failTime, rotation, originalScale, originalPosition);

                appliedObjects.Add(obj);
            }
        }

        private void dropOffScreen(DrawableHitObject obj, double failTime, float randomRotation, Vector2 originalScale, Vector2 originalPosition)
        {
            using (obj.BeginAbsoluteSequence(failTime))
            {
                obj.RotateTo(randomRotation, duration);
                obj.ScaleTo(originalScale * 0.5f, duration);
                obj.MoveTo(originalPosition + new Vector2(0, 400), duration);
            }
        }
    }
}
