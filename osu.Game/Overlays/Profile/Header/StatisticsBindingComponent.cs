// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Header
{
    public abstract class StatisticsBindingComponent : CompositeDrawable
    {
        public readonly Bindable<UserStatistics> Statistics = new Bindable<UserStatistics>();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Statistics.BindValueChanged(statistics => UpdateStatistics(statistics.NewValue), true);
        }

        protected abstract void UpdateStatistics(UserStatistics statistics);
    }
}
