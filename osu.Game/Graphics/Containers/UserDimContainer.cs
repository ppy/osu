// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osuTK.Graphics;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that applies user-configured visual settings to its contents.
    /// </summary>
    public abstract class UserDimContainer : Container
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

        protected Bindable<double> UserDimLevel { get; private set; }

        protected Bindable<bool> ShowStoryboard { get; private set; }

        protected Container DimContainer { get; }

        protected override Container<Drawable> Content => DimContainer;

        /// <summary>
        /// Creates a new <see cref="UserDimContainer"/>.
        /// </summary>
        protected UserDimContainer()
        {
            AddInternal(DimContainer = new Container { RelativeSizeAxes = Axes.Both });
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            UserDimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            ShowStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);

            EnableUserDim.ValueChanged += _ => UpdateVisuals();
            UserDimLevel.ValueChanged += _ => UpdateVisuals();
            ShowStoryboard.ValueChanged += _ => UpdateVisuals();
            StoryboardReplacesBackground.ValueChanged += _ => UpdateVisuals();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            UpdateVisuals();
        }

        /// <summary>
        /// Apply non-dim related settings to the content, such as hiding and blurring.
        /// </summary>
        /// <remarks>
        /// While both backgrounds and storyboards allow user dim levels to be applied, storyboards can be toggled via <see cref="ShowStoryboard"/>
        /// and can cause backgrounds to become hidden via <see cref="StoryboardReplacesBackground"/>. Storyboards are also currently unable to be blurred.
        /// </remarks>
        protected abstract void ApplyFade();

        protected void UpdateVisuals()
        {
            ApplyFade();

            DimContainer.FadeColour(EnableUserDim.Value ? OsuColour.Gray(1 - (float)UserDimLevel.Value) : Color4.White, BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }
    }
}
