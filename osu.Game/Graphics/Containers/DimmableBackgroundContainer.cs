// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    public class DimmableBackgroundContainer : UserDimContainer
    {
        /// <summary>
        /// The amount of blur to be applied to the background in addition to user-specified blur.
        /// </summary>
        /// <remarks>
        /// Used in contexts where there can potentially be both user and screen-specified blurring occuring at the same time, such as in <see cref="PlayerLoader"/>
        /// </remarks>
        public readonly Bindable<float> BlurAmount = new Bindable<float>();

        private Bindable<double> userBlurLevel { get; set; }

        private Background background;

        public Background Background
        {
            get => background;
            set
            {
                base.Add(background = value);
                background.BlurTo(blurTarget, 0, Easing.OutQuint);
            }
        }

        public override void Add(Drawable drawable)
        {
            if (drawable is Background)
                throw new InvalidOperationException($"Use {nameof(Background)} to set a background.");

            base.Add(drawable);
        }

        /// <summary>
        /// As an optimisation, we add the two blur portions to be applied rather than actually applying two separate blurs.
        /// </summary>
        private Vector2 blurTarget => EnableUserDim.Value
            ? new Vector2(BlurAmount.Value + (float)userBlurLevel.Value * 25)
            : new Vector2(BlurAmount.Value);

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            userBlurLevel = config.GetBindable<double>(OsuSetting.BlurLevel);
            BlurAmount.ValueChanged += _ => UpdateVisuals();
            userBlurLevel.ValueChanged += _ => UpdateVisuals();
        }

        protected override void ApplyFade()
        {
            // The background needs to be hidden in the case of it being replaced by the storyboard
            DimContainer.FadeTo(ShowStoryboard.Value && StoryboardReplacesBackground.Value ? 0 : 1, BACKGROUND_FADE_DURATION, Easing.OutQuint);
            Background?.BlurTo(blurTarget, BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }
    }
}
