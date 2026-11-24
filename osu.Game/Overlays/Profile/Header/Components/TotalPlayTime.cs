// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class TotalPlayTime : CompositeDrawable
    {
        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        private ProfileValueDisplay info = null!;

        public TotalPlayTime()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = info = new ProfileValueDisplay(minimumWidth: 140) { Title = UsersStrings.ShowStatsPlayTime };

            User.BindValueChanged(updateTime, true);
        }

        private void updateTime(ValueChangedEvent<UserProfileData?> user)
        {
            var playTime = TimeSpan.FromSeconds(user.NewValue?.User.Statistics.PlayTime ?? 0);

            int totalMinutes = (int)Math.Floor(playTime.TotalMinutes);
            int totalHours = (int)Math.Round(playTime.TotalHours);

            info.Content.Text = playTime.ToString(playTime.Days > 0 ? @"d'd 'h'h 'm'm'" : @"h'h 'm'm'");
            info.Content.TooltipText = totalHours < 2
                ? CommonStrings.CountMinutes(totalMinutes)
                : CommonStrings.CountHours(totalHours);
        }
    }
}
