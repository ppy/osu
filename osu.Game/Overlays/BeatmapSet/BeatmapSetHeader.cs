// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class BeatmapSetHeader : TabControlOverlayHeader<BeatmapSetTabs>
    {
        public readonly Bindable<APIBeatmapSet> BeatmapSet = new Bindable<APIBeatmapSet>();

        public BeatmapSetHeaderContent HeaderContent { get; private set; }

        [Cached]
        public BeatmapRulesetSelector RulesetSelector { get; private set; }

        [Cached(typeof(IBindable<RulesetInfo>))]
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        public BeatmapSetHeader()
        {
            Masking = true;

            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0.25f),
                Type = EdgeEffectType.Shadow,
                Radius = 3,
                Offset = new Vector2(0f, 1f),
            };
        }

        protected override Drawable CreateContent() => HeaderContent = new BeatmapSetHeaderContent
        {
            BeatmapSet = { BindTarget = BeatmapSet }
        };

        protected override Drawable CreateTabControlContent() => RulesetSelector = new BeatmapRulesetSelector
        {
            Current = ruleset
        };

        protected override OverlayTitle CreateTitle() => new BeatmapHeaderTitle();

        private partial class BeatmapHeaderTitle : OverlayTitle
        {
            public BeatmapHeaderTitle()
            {
                Title = PageTitleStrings.MainBeatmapsetsControllerShow;
                Icon = OsuIcon.Beatmap;
            }
        }
    }

    public enum BeatmapSetTabs
    {
        [LocalisableDescription(typeof(LayoutStrings), nameof(LayoutStrings.HeaderBeatmapsetsShow))]
        Info,
    }
}
