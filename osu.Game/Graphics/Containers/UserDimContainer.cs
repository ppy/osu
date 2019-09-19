// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;

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
        public readonly Bindable<bool> EnableUserDim = new Bindable<bool>(true);

        /// <summary>
        /// Whether or not the storyboard loaded should completely hide the background behind it.
        /// </summary>
        public readonly Bindable<bool> StoryboardReplacesBackground = new Bindable<bool>();

        /// <summary>
        /// Whether the content of this container is currently being displayed.
        /// </summary>
        public bool ContentDisplayed { get; private set; }

        protected Bindable<double> UserDimLevel { get; private set; }

        protected Bindable<bool> ShowStoryboard { get; private set; }

        protected Bindable<bool> ShowVideo { get; private set; }

        protected double DimLevel => EnableUserDim.Value ? UserDimLevel.Value : 0;

        protected override Container<Drawable> Content => dimContent;

        private Container dimContent { get; }

        /// <summary>
        /// Creates a new <see cref="UserDimContainer"/>.
        /// </summary>
        protected UserDimContainer()
        {
            AddInternal(dimContent = new Container { RelativeSizeAxes = Axes.Both });
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            UserDimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            ShowStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
            ShowVideo = config.GetBindable<bool>(OsuSetting.ShowVideoBackground);

            EnableUserDim.ValueChanged += _ => UpdateVisuals();
            UserDimLevel.ValueChanged += _ => UpdateVisuals();
            ShowStoryboard.ValueChanged += _ => UpdateVisuals();
            ShowVideo.ValueChanged += _ => UpdateVisuals();
            StoryboardReplacesBackground.ValueChanged += _ => UpdateVisuals();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            UpdateVisuals();
        }

        /// <summary>
        /// Whether the content of this container should currently be visible.
        /// </summary>
        protected virtual bool ShowDimContent => true;

        /// <summary>
        /// Should be invoked when any dependent dim level or user setting is changed and bring the visual state up-to-date.
        /// </summary>
        protected virtual void UpdateVisuals()
        {
            ContentDisplayed = ShowDimContent;

            dimContent.FadeTo(ContentDisplayed ? 1 : 0, BACKGROUND_FADE_DURATION, Easing.OutQuint);
            dimContent.FadeColour(OsuColour.Gray(1 - (float)DimLevel), BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }
    }
}
