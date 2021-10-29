// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
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
                Text = UsersStrings.ShowExtraUnranked,
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
            int[] userRanks = statistics?.RankHistory?.Data;
            Data = userRanks?.Select((x, index) => new KeyValuePair<int, int>(index, x)).Where(x => x.Value != 0).ToArray();
        }

        protected override float GetDataPointHeight(int rank) => -MathF.Log(rank);

        protected override void ShowGraph()
        {
            base.ShowGraph();
            placeholder.FadeOut(FADE_DURATION, Easing.Out);
        }

        protected override void HideGraph()
        {
            base.HideGraph();
            placeholder.FadeIn(FADE_DURATION, Easing.Out);
        }

        protected override UserGraphTooltipContent GetTooltipContent(int index, int rank)
        {
            int days = ranked_days - index + 1;

            return new UserGraphTooltipContent(
                UsersStrings.ShowRankGlobalSimple,
                rank.ToLocalisableString("\\##,##0"),
                days == 0 ? "now" : $"{"day".ToQuantity(days)} ago");
        }
    }
}
