// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract partial class ProfileSubsection : FillFlowContainer
    {
        protected readonly Bindable<UserProfileData?> UserProfileData = new Bindable<UserProfileData?>();

        private readonly LocalisableString headerText;
        private readonly CounterVisibilityState counterVisibilityState;

        private ProfileSubsectionHeader header = null!;

        protected ProfileSubsection(Bindable<UserProfileData?> userProfileData, LocalisableString? headerText = null, CounterVisibilityState counterVisibilityState = CounterVisibilityState.AlwaysHidden)
        {
            this.headerText = headerText ?? string.Empty;
            this.counterVisibilityState = counterVisibilityState;
            UserProfileData.BindTo(userProfileData);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;

            Children = new[]
            {
                header = new ProfileSubsectionHeader(headerText, counterVisibilityState)
                {
                    Alpha = string.IsNullOrEmpty(headerText.ToString()) ? 0 : 1
                },
                CreateContent()
            };
        }

        protected abstract Drawable CreateContent();

        protected void SetCount(int value) => header.Current.Value = value;
    }
}
