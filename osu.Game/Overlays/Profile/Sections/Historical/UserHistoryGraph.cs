// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using static osu.Game.Users.User;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class UserHistoryGraph : UserGraph<DateTime, long>
    {
        private readonly string tooltipCounterName;

        [CanBeNull]
        public UserHistoryCount[] Values
        {
            set => Data = value?.Select(v => new KeyValuePair<DateTime, long>(v.Date, v.Count)).ToArray();
        }

        public UserHistoryGraph(string tooltipCounterName)
        {
            this.tooltipCounterName = tooltipCounterName;
        }

        protected override float GetDataPointHeight(long playCount) => playCount;

        protected override UserGraphTooltip GetTooltip() => new HistoryGraphTooltip(tooltipCounterName);

        protected override object GetTooltipContent(DateTime date, long playCount)
        {
            return new TooltipDisplayContent
            {
                Name = tooltipCounterName,
                Count = playCount.ToString("N0"),
                Date = date.ToString("MMMM yyyy")
            };
        }

        protected class HistoryGraphTooltip : UserGraphTooltip
        {
            private readonly string tooltipCounterName;

            public HistoryGraphTooltip(string tooltipCounterName)
                : base(tooltipCounterName)
            {
                this.tooltipCounterName = tooltipCounterName;
            }

            public override bool SetContent(object content)
            {
                if (!(content is TooltipDisplayContent info) || info.Name != tooltipCounterName)
                    return false;

                Counter.Text = info.Count;
                BottomText.Text = info.Date;
                return true;
            }
        }

        private class TooltipDisplayContent
        {
            public string Name;
            public string Count;
            public string Date;
        }
    }
}
