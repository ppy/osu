// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using static osu.Game.Users.User;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public abstract class UserHistoryGraph : UserGraph<DateTime, long>
    {
        public UserHistoryCount[] Values
        {
            set => Data = value?.Select(v => new KeyValuePair<DateTime, long>(v.Date, v.Count)).ToArray();
        }

        protected override float GetDataPointHeight(long playCount) => playCount;

        protected override object GetTooltipContent(DateTime date, long playCount)
        {
            return new TooltipDisplayContent
            {
                Count = playCount.ToString("N0"),
                Date = date.ToString("MMMM yyyy")
            };
        }

        protected abstract class HistoryGraphTooltip : UserGraphTooltip
        {
            public override bool SetContent(object content)
            {
                if (!(content is TooltipDisplayContent info))
                    return false;

                Counter.Text = info.Count;
                BottomText.Text = info.Date;
                return true;
            }
        }

        private class TooltipDisplayContent
        {
            public string Count;
            public string Date;
        }
    }
}
