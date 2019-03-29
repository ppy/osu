// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that applies user-configured visual settings to its contents.
    /// This container specifies behavior that applies to both Storyboards and Backgrounds.
    /// </summary>
    public class UserDimContainer : Container
    {
        private const float background_fade_duration = 800;

        /// <summary>
        /// Whether or not user-configured dim levels should be applied to the container.
        /// </summary>
        public readonly Bindable<bool> EnableUserDim = new Bindable<bool>();

        /// <summary>
        /// Whether or not the storyboard loaded should completely hide the background behind it.
        /// </summary>
        public readonly Bindable<bool> StoryboardReplacesBackground = new Bindable<bool>();

        /// <summary>
        /// The amount of blur to be applied to the background in addition to user-specified blur.
        /// </summary>
        /// <remarks>
        /// Used in contexts where there can potentially be both user and screen-specified blurring occuring at the same time, such as in <see cref="PlayerLoader"/>
        /// </remarks>
        public readonly Bindable<float> BlurAmount = new Bindable<float>();

        private Bindable<double> userDimLevel { get; set; }

        private Bindable<double> userBlurLevel { get; set; }

        private Bindable<bool> showStoryboard { get; set; }

        protected Container DimContainer { get; }

        protected override Container<Drawable> Content => DimContainer;

        private readonly bool isStoryboard;

        /// <summary>
        /// As an optimisation, we add the two blur portions to be applied rather than actually applying two separate blurs.
        /// </summary>
        private Vector2 blurTarget => EnableUserDim.Value
            ? new Vector2(BlurAmount.Value + (float)userBlurLevel.Value * 25)
            : new Vector2(BlurAmount.Value);

        /// <summary>
        /// Creates a new <see cref="UserDimContainer"/>.
        /// </summary>
        /// <param name="isStoryboard"> Whether or not this instance contains a storyboard.
        /// <remarks>
        /// While both backgrounds and storyboards allow user dim levels to be applied, storyboards can be toggled via <see cref="showStoryboard"/>
        /// and can cause backgrounds to become hidden via <see cref="StoryboardReplacesBackground"/>. Storyboards are also currently unable to be blurred.
        /// </remarks>
        /// </param>
        public UserDimContainer(bool isStoryboard = false)
        {
            this.isStoryboard = isStoryboard;
            AddInternal(DimContainer = new Container { RelativeSizeAxes = Axes.Both });
        }

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

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            userDimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            userBlurLevel = config.GetBindable<double>(OsuSetting.BlurLevel);
            showStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);

            EnableUserDim.ValueChanged += _ => updateVisuals();
            userDimLevel.ValueChanged += _ => updateVisuals();
            userBlurLevel.ValueChanged += _ => updateVisuals();
            showStoryboard.ValueChanged += _ => updateVisuals();
            StoryboardReplacesBackground.ValueChanged += _ => updateVisuals();
            BlurAmount.ValueChanged += _ => updateVisuals();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateVisuals();
        }

        private void updateVisuals()
        {
            if (isStoryboard)
            {
                DimContainer.FadeTo(!showStoryboard.Value || userDimLevel.Value == 1 ? 0 : 1, background_fade_duration, Easing.OutQuint);
            }
            else
            {
                // The background needs to be hidden in the case of it being replaced by the storyboard
                DimContainer.FadeTo(showStoryboard.Value && StoryboardReplacesBackground.Value ? 0 : 1, background_fade_duration, Easing.OutQuint);

                Background?.BlurTo(blurTarget, background_fade_duration, Easing.OutQuint);
            }

            DimContainer.FadeColour(EnableUserDim.Value ? OsuColour.Gray(1 - (float)userDimLevel.Value) : Color4.White, background_fade_duration, Easing.OutQuint);
        }
    }
}
