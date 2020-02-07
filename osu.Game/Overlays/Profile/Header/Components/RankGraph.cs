// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class RankGraph : UserGraph<int, int>
    {
        private const int ranked_days = 88;

        public readonly Bindable<UserStatistics> Statistics = new Bindable<UserStatistics>();

        private readonly OsuSpriteText placeholder;

        public RankGraph()
        {
            Add(placeholder = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "No recent plays",
                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular)
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Statistics.BindValueChanged(statistics => updateStatistics(statistics.NewValue), true);
        }

        private void updateStatistics(UserStatistics statistics)
        {
            placeholder.FadeIn(FADE_DURATION, Easing.Out);

            if (statistics?.Ranks.Global == null)
            {
                Graph.FadeOut(FADE_DURATION, Easing.Out);
                Data = null;
                return;
            }

            int[] userRanks = statistics.RankHistory?.Data ?? new[] { statistics.Ranks.Global.Value };
            Data = userRanks.Select((x, index) => new KeyValuePair<int, int>(index, x)).Where(x => x.Value != 0).ToArray();

            if (Data.Length > 1)
            {
                placeholder.FadeOut(FADE_DURATION, Easing.Out);

                Graph.DefaultValueCount = Data.Length;
                Graph.Values = Data.Select(x => -MathF.Log(x.Value));
            }

            Graph.FadeTo(Data.Length > 1 ? 1 : 0, FADE_DURATION, Easing.Out);
        }

        protected override object GetTooltipContent()
        {
            if (Statistics.Value?.Ranks.Global == null)
                return null;

            var days = ranked_days - Data[DataIndex].Key + 1;

            return new TooltipDisplayContent
            {
                Rank = $"#{Data[DataIndex].Value:#,##0}",
                Time = days == 0 ? "now" : $"{days} days ago"
            };
        }

        protected override UserGraphTooltip GetTooltip() => new RankGraphTooltip();

        private class RankGraphTooltip : UserGraphTooltip
        {
            public RankGraphTooltip()
                : base(@"Global Ranking")
            {
            }

            public override bool SetContent(object content)
            {
                if (!(content is TooltipDisplayContent info))
                    return false;

                Counter.Text = info.Rank;
                BottomText.Text = info.Time;
                return true;
            }
        }

        private class TooltipDisplayContent
        {
            public string Rank;
            public string Time;
        }
    }
}
