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
    /// A container that applies user-configured dim levels to its contents.
    /// This container specifies behavior that applies to both Storyboards and Backgrounds.
    /// </summary>
    public class UserDimContainer : Container
    {
        protected Bindable<double> DimLevel;

        protected Bindable<bool> ShowStoryboard;

        /// <summary>
        /// Whether or not user-configured dim levels should be applied to the container.
        /// </summary>
        public readonly Bindable<bool> EnableUserDim = new Bindable<bool>();

        /// <summary>
        /// Whether or not the storyboard loaded should completely hide the background behind it.
        /// </summary>
        public Bindable<bool> StoryboardReplacesBackground = new Bindable<bool>();

        protected Container DimContainer;

        protected override Container<Drawable> Content => DimContainer;

        private readonly bool isStoryboard;

        private const float background_fade_duration = 800;

        /// <summary>
        /// </summary>
        /// <param name="isStoryboard">
        /// Whether or not this instance of UserDimContainer contains a storyboard.
        /// While both backgrounds and storyboards allow user dim levels to be applied, storyboards can be toggled via <see cref="ShowStoryboard"/>
        /// and can cause backgrounds to become hidden via <see cref="StoryboardReplacesBackground"/>.
        /// </param>
        public UserDimContainer(bool isStoryboard = false)
        {
            DimContainer = new Container { RelativeSizeAxes = Axes.Both };
            this.isStoryboard = isStoryboard;
            AddInternal(DimContainer);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            DimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            ShowStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
            EnableUserDim.ValueChanged += _ => updateBackgroundDim();
            DimLevel.ValueChanged += _ => updateBackgroundDim();
            ShowStoryboard.ValueChanged += _ => updateBackgroundDim();
            StoryboardReplacesBackground.ValueChanged += _ => updateBackgroundDim();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateBackgroundDim();
        }

        private void updateBackgroundDim()
        {
            if (isStoryboard)
            {
                DimContainer.FadeTo(!ShowStoryboard.Value || DimLevel.Value == 1 ? 0 : 1, background_fade_duration, Easing.OutQuint);
            }
            else
            {
                // The background needs to be hidden in the case of it being replaced by the storyboard
                DimContainer.FadeTo(ShowStoryboard.Value && StoryboardReplacesBackground.Value ? 0 : 1, background_fade_duration, Easing.OutQuint);
            }

            DimContainer.FadeColour(EnableUserDim.Value ? OsuColour.Gray(1 - (float)DimLevel.Value) : Color4.White, background_fade_duration, Easing.OutQuint);
        }
    }
}
