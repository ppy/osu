// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class BeatmapRulesetTabItem : OverlayRulesetTabItem
    {
        public readonly Bindable<APIBeatmapSet> BeatmapSet = new Bindable<APIBeatmapSet>();

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        private OsuSpriteText count;
        private Container countContainer;

        public BeatmapRulesetTabItem(RulesetInfo value)
            : base(value)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(countContainer = new Container
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
                CornerRadius = 4f,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background6
                    },
                    count = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Margin = new MarginPadding { Horizontal = 5f },
                        Font = OsuFont.Default.With(weight: FontWeight.SemiBold),
                        Colour = colourProvider.Foreground1,
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            BeatmapSet.BindValueChanged(setInfo =>
            {
                int beatmapsCount = setInfo.NewValue?.Beatmaps.Count(b => b.Ruleset.MatchesOnlineID(Value)) ?? 0;
                int osuBeatmaps = setInfo.NewValue?.Beatmaps.Count(b => b.Ruleset.OnlineID == 0) ?? 0;

                count.Text = beatmapsCount.ToString();
                countContainer.FadeTo(beatmapsCount > 0 ? 1 : 0);

                Enabled.Value = beatmapsCount > 0 || osuBeatmaps > 0;
            }, true);
        }
    }
}
