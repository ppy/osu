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
    /// </summary>
    public class UserDimContainer : Container
    {
        protected const float BACKGROUND_FADE_DURATION = 800;

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

        protected Bindable<double> UserDimLevel { get; private set; }

        protected Bindable<bool> ShowStoryboard { get; private set; }

        protected Container DimContainer { get; }

        protected override Container<Drawable> Content => DimContainer;

        private Bindable<double> userBlurLevel { get; set; }

        /// <summary>
        /// As an optimisation, we add the two blur portions to be applied rather than actually applying two separate blurs.
        /// </summary>
        private Vector2 blurTarget => EnableUserDim.Value
            ? new Vector2(BlurAmount.Value + (float)userBlurLevel.Value * 25)
            : new Vector2(BlurAmount.Value);

        /// <summary>
        /// Creates a new <see cref="UserDimContainer"/>.
        /// </summary>
        public UserDimContainer()
        {
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
            UserDimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            userBlurLevel = config.GetBindable<double>(OsuSetting.BlurLevel);
            ShowStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);

            EnableUserDim.ValueChanged += _ => updateVisuals();
            UserDimLevel.ValueChanged += _ => updateVisuals();
            ShowStoryboard.ValueChanged += _ => updateVisuals();
            StoryboardReplacesBackground.ValueChanged += _ => updateVisuals();
            BlurAmount.ValueChanged += _ => updateVisuals();
            userBlurLevel.ValueChanged += _ => updateVisuals();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateVisuals();
        }

        /// <summary>
        /// Apply non-dim related settings to the content, such as hiding and blurring.
        /// </summary>
        /// <remarks>
        /// While both backgrounds and storyboards allow user dim levels to be applied, storyboards can be toggled via <see cref="ShowStoryboard"/>
        /// and can cause backgrounds to become hidden via <see cref="StoryboardReplacesBackground"/>. Storyboards are also currently unable to be blurred.
        /// </remarks>
        protected virtual void ApplyFade()
        {
            // The background needs to be hidden in the case of it being replaced by the storyboard
            DimContainer.FadeTo(ShowStoryboard.Value && StoryboardReplacesBackground.Value ? 0 : 1, BACKGROUND_FADE_DURATION, Easing.OutQuint);
            Background?.BlurTo(blurTarget, BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }

        private void updateVisuals()
        {
            ApplyFade();

            DimContainer.FadeColour(EnableUserDim.Value ? OsuColour.Gray(1 - (float)UserDimLevel.Value) : Color4.White, BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }
    }
}
