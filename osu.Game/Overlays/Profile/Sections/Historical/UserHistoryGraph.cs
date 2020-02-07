// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using static osu.Game.Users.User;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class UserHistoryGraph : UserGraph<DateTime, long>
    {
        private UserHistoryCount[] values;

        public UserHistoryCount[] Values
        {
            get => values;
            set
            {
                values = value;
                updateValues(value);
            }
        }

        private readonly string tooltipCounterName;

        public UserHistoryGraph(string tooltipCounterName)
        {
            this.tooltipCounterName = tooltipCounterName;
        }

        private void updateValues(UserHistoryCount[] values)
        {
            if (values == null || !values.Any())
            {
                Graph.FadeOut(FADE_DURATION, Easing.Out);
                Data = null;
                return;
            }

            Data = values.Select(v => new KeyValuePair<DateTime, long>(v.Date, v.Count)).ToArray();

            if (values.Length > 1)
            {
                Graph.DefaultValueCount = Data.Length;
                Graph.Values = Data.Select(x => (float)x.Value);
                Graph.FadeIn(FADE_DURATION, Easing.Out);
            }
        }

        protected override object GetTooltipContent()
        {
            if (!Data?.Any() ?? true)
                return null;

            return new TooltipDisplayContent
            {
                Count = Data[DataIndex].Value.ToString("N0"),
                Date = Data[DataIndex].Key.ToString("MMMM yyyy")
            };
        }

        protected override UserGraphTooltip GetTooltip() => new HistoryGraphTooltip(tooltipCounterName);

        private class HistoryGraphTooltip : UserGraphTooltip
        {
            public HistoryGraphTooltip(string topText)
                : base(topText)
            {
            }

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
