// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using static osu.Game.Users.User;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class UserHistoryGraph : UserGraph<DateTime, long>
    {
        private readonly LocalisableString tooltipCounterName;

        [CanBeNull]
        public UserHistoryCount[] Values
        {
            set => Data = value?.Select(v => new KeyValuePair<DateTime, long>(v.Date, v.Count)).ToArray();
        }

        public UserHistoryGraph(LocalisableString tooltipCounterName)
        {
            this.tooltipCounterName = tooltipCounterName;
        }

        protected override float GetDataPointHeight(long playCount) => playCount;

        protected override UserGraphTooltipContent GetTooltipContent(DateTime date, long playCount) =>
            new UserGraphTooltipContent(
                tooltipCounterName,
                playCount.ToLocalisableString("N0"),
                date.ToLocalisableString("MMMM yyyy"));
    }
}
