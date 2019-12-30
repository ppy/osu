// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osuTK.Graphics;
using System.Linq;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapRulesetTabItem : OverlayRulesetTabItem
    {
        private readonly OsuSpriteText count;

        public readonly Bindable<BeatmapSetInfo> BeatmapSet = new Bindable<BeatmapSetInfo>();

        public BeatmapRulesetTabItem(RulesetInfo value)
            : base(value)
        {
            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 4f,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.5f),
                    },
                    count = new OsuSpriteText
                    {
                        Alpha = 0,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Margin = new MarginPadding { Horizontal = 5f },
                        Font = OsuFont.Default.With(weight: FontWeight.SemiBold),
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            count.Colour = colours.Gray9;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            BeatmapSet.BindValueChanged(setInfo =>
            {
                var beatmapsCount = setInfo.NewValue?.Beatmaps.Count(b => b.Ruleset.Equals(Value)) ?? 0;

                count.Text = beatmapsCount.ToString();
                count.Alpha = beatmapsCount > 0 ? 1f : 0f;

                Enabled.Value = beatmapsCount > 0;
            }, true);
        }
    }
}
