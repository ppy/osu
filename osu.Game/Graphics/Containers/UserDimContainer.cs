// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Screens.Play;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that applies user-configured visual settings to its contents.
    /// </summary>
    public abstract class UserDimContainer : Container
    {
        /// <summary>
        /// Amount of lightening to apply to current dim level during break times.
        /// </summary>
        public const float BREAK_LIGHTEN_AMOUNT = 0.3f;

        protected const double BACKGROUND_FADE_DURATION = 800;

        /// <summary>
        /// Whether or not user-configured dim levels should be applied to the container.
        /// </summary>
        public readonly Bindable<bool> EnableUserDim = new Bindable<bool>(true);

        /// <summary>
        /// Whether or not user-configured settings relating to brightness of elements should be ignored
        /// </summary>
        public readonly Bindable<bool> IgnoreUserSettings = new Bindable<bool>();

        /// <summary>
        /// Whether or not the storyboard loaded should completely hide the background behind it.
        /// </summary>
        public readonly Bindable<bool> StoryboardReplacesBackground = new Bindable<bool>();

        /// <summary>
        /// Whether player is in break time.
        /// Must be bound to <see cref="BreakTracker.IsBreakTime"/> to allow for dim adjustments in gameplay.
        /// </summary>
        public readonly IBindable<bool> IsBreakTime = new Bindable<bool>();

        /// <summary>
        /// Whether the content of this container is currently being displayed.
        /// </summary>
        public bool ContentDisplayed { get; private set; }

        protected Bindable<double> UserDimLevel { get; private set; }

        protected Bindable<bool> LightenDuringBreaks { get; private set; }

        protected Bindable<bool> ShowStoryboard { get; private set; }

        private float breakLightening => LightenDuringBreaks.Value && IsBreakTime.Value ? BREAK_LIGHTEN_AMOUNT : 0;

        protected float DimLevel => Math.Max(EnableUserDim.Value && !IgnoreUserSettings.Value ? (float)UserDimLevel.Value - breakLightening : 0, 0);

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
            LightenDuringBreaks = config.GetBindable<bool>(OsuSetting.LightenDuringBreaks);
            ShowStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);

            EnableUserDim.ValueChanged += _ => UpdateVisuals();
            UserDimLevel.ValueChanged += _ => UpdateVisuals();
            LightenDuringBreaks.ValueChanged += _ => UpdateVisuals();
            IsBreakTime.ValueChanged += _ => UpdateVisuals();
            ShowStoryboard.ValueChanged += _ => UpdateVisuals();
            StoryboardReplacesBackground.ValueChanged += _ => UpdateVisuals();
            IgnoreUserSettings.ValueChanged += _ => UpdateVisuals();
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
            dimContent.FadeColour(OsuColour.Gray(1f - DimLevel), BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }
    }
}
