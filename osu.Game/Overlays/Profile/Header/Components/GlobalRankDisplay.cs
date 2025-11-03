// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class GlobalRankDisplay : CompositeDrawable
    {
        public Bindable<UserStatistics?> UserStatistics = new Bindable<UserStatistics?>();
        public Bindable<APIUser.UserRankHighest?> HighestRank = new Bindable<APIUser.UserRankHighest?>();

        private ProfileValueDisplay info = null!;

        public GlobalRankDisplay()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = info = new ProfileValueDisplay(big: true)
            {
                Title = UsersStrings.ShowRankGlobalSimple
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            UserStatistics.BindValueChanged(_ => updateState());
            HighestRank.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            info.Content.Text = UserStatistics.Value?.GlobalRank?.ToLocalisableString("\\##,##0") ?? (LocalisableString)"-";
            info.Content.TooltipText = getGlobalRankTooltipText();
        }

        private LocalisableString getGlobalRankTooltipText()
        {
            var rankHighest = HighestRank.Value;
            var variants = UserStatistics.Value?.Variants;

            LocalisableString? result = null;

            if (variants?.Count > 0)
            {
                foreach (var variant in variants)
                {
                    if (variant.GlobalRank != null)
                    {
                        var variantText = LocalisableString.Interpolate($"{variant.VariantType.GetLocalisableDescription()}: {variant.GlobalRank.ToLocalisableString("\\##,##0")}");

                        if (result == null)
                            result = variantText;
                        else
                            result = LocalisableString.Interpolate($"{result}\n{variantText}");
                    }
                }
            }

            if (rankHighest != null)
            {
                var rankHighestText = UsersStrings.ShowRankHighest(
                    rankHighest.Rank.ToLocalisableString("\\##,##0"),
                    rankHighest.UpdatedAt.ToLocalisableString(@"d MMM yyyy"));

                if (result == null)
                    result = rankHighestText;
                else
                    result = LocalisableString.Interpolate($"{result}\n{rankHighestText}");
            }

            return result ?? default;
        }
    }
}
