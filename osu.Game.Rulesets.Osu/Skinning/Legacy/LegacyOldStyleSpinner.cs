// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    /// <summary>
    /// Legacy skinned spinner with one main spinning layer and a background layer.
    /// </summary>
    public class LegacyOldStyleSpinner : LegacySpinner
    {
        private Sprite disc;
        private Sprite metreSprite;
        private Container metre;

        private bool spinnerBlink;

        private const float final_metre_height = 692 * SPRITE_SCALE;

        [BackgroundDependencyLoader]
        private void load(ISkinSource source)
        {
            spinnerBlink = source.GetConfig<OsuSkinConfiguration, bool>(OsuSkinConfiguration.SpinnerNoBlink)?.Value != true;

            AddInternal(new Container
            {
                // the old-style spinner relied heavily on absolute screen-space coordinate values.
                // wrap everything in a container simulating absolute coords to preserve alignment
                // as there are skins that depend on it.
                Width = 640,
                Height = 480,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = source.GetTexture("spinner-background"),
                        Scale = new Vector2(SPRITE_SCALE)
                    },
                    disc = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = source.GetTexture("spinner-circle"),
                        Scale = new Vector2(SPRITE_SCALE)
                    },
                    metre = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        // this anchor makes no sense, but that's what stable uses.
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        // adjustment for stable (metre has additional offset)
                        Margin = new MarginPadding { Top = 20 },
                        Masking = true,
                        Child = metreSprite = new Sprite
                        {
                            Texture = source.GetTexture("spinner-metre"),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Scale = new Vector2(SPRITE_SCALE)
                        }
                    }
                }
            });
        }

        protected override void UpdateStateTransforms(DrawableHitObject drawableHitObject, ArmedState state)
        {
            base.UpdateStateTransforms(drawableHitObject, state);

            if (!(drawableHitObject is DrawableSpinner d))
                return;

            Spinner spinner = d.HitObject;

            using (BeginAbsoluteSequence(spinner.StartTime - spinner.TimePreempt, true))
                this.FadeOut();

            using (BeginAbsoluteSequence(spinner.StartTime - spinner.TimeFadeIn / 2, true))
                this.FadeInFromZero(spinner.TimeFadeIn / 2);
        }

        protected override void Update()
        {
            base.Update();
            disc.Rotation = DrawableSpinner.RotationTracker.Rotation;

            // careful: need to call this exactly once for all calculations in a frame
            // as the function has a random factor in it
            var metreHeight = getMetreHeight(DrawableSpinner.Progress);

            // hack to make the metre blink up from below than down from above.
            // move down the container to be able to apply masking for the metre,
            // and then move the sprite back up the same amount to keep its position absolute.
            metre.Y = final_metre_height - metreHeight;
            metreSprite.Y = -metre.Y;
        }

        private const int total_bars = 10;

        private float getMetreHeight(float progress)
        {
            progress *= 100;

            // the spinner should still blink at 100% progress.
            if (spinnerBlink)
                progress = Math.Min(99, progress);

            int barCount = (int)progress / 10;

            if (spinnerBlink && RNG.NextBool(((int)progress % 10) / 10f))
                barCount++;

            return (float)barCount / total_bars * final_metre_height;
        }
    }
}
