// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Game.Rulesets.UI;
using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Manage the animation to be applied when a player fails.
    /// Single file; automatically disposed after use.
    /// </summary>
    public class FailAnimation : Component
    {
        public Action OnComplete;

        private readonly DrawableRuleset drawableRuleset;

        private readonly BindableDouble trackFreq = new BindableDouble(1);

        private Track track;

        private const float duration = 2500;

        private Sample failSample;

        public FailAnimation(DrawableRuleset drawableRuleset)
        {
            this.drawableRuleset = drawableRuleset;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, IBindable<WorkingBeatmap> beatmap)
        {
            track = beatmap.Value.Track;
            failSample = audio.Samples.Get(@"Gameplay/failsound");
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

            failSample.Play();

            this.TransformBindableTo(trackFreq, 0, duration).OnComplete(_ =>
            {
                OnComplete?.Invoke();
                Expire();
            });

            track.AddAdjustment(AdjustableProperty.Frequency, trackFreq);

            applyToPlayfield(drawableRuleset.Playfield);
            drawableRuleset.Playfield.HitObjectContainer.FlashColour(Color4.Red, 500);
            drawableRuleset.Playfield.HitObjectContainer.FadeOut(duration / 2);
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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            track?.RemoveAdjustment(AdjustableProperty.Frequency, trackFreq);
        }
    }
}
