// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public partial class UserHistoryGraph : UserGraph<DateTime, long>
    {
        private readonly LocalisableString tooltipCounterName;

        public APIUserHistoryCount[]? Values
        {
            set => Data = value?.Select(v => new KeyValuePair<DateTime, long>(v.Date, v.Count)).ToArray();
        }

        public UserHistoryGraph(LocalisableString tooltipCounterName)
        {
            this.tooltipCounterName = tooltipCounterName;
        }

        protected override float GetDataPointHeight(long playCount) => playCount;

        protected override UserGraphTooltipContent GetTooltipContent(DateTime date, long playCount) =>
            new UserGraphTooltipContent
            {
                Name = tooltipCounterName,
                Count = playCount.ToLocalisableString("N0"),
                Time = date.ToLocalisableString("MMMM yyyy")
            };
    }
}
