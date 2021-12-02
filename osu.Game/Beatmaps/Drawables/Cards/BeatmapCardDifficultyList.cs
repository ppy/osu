// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class BeatmapCardDifficultyList : CompositeDrawable
    {
        public BeatmapCardDifficultyList(IBeatmapSetInfo beatmapSetInfo)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            FillFlowContainer flow;

            InternalChild = flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 3)
            };

            bool firstGroup = true;

            foreach (var group in beatmapSetInfo.Beatmaps.GroupBy(beatmap => beatmap.Ruleset.OnlineID).OrderBy(group => group.Key))
            {
                if (!firstGroup)
                {
                    flow.Add(Empty().With(s =>
                    {
                        s.RelativeSizeAxes = Axes.X;
                        s.Height = 4;
                    }));
                }

                foreach (var difficulty in group.OrderBy(b => b.StarRating))
                    flow.Add(new BeatmapCardDifficultyRow(difficulty));

                firstGroup = false;
            }
        }

        private class BeatmapCardDifficultyRow : CompositeDrawable
        {
            private readonly IBeatmapInfo beatmapInfo;

            public BeatmapCardDifficultyRow(IBeatmapInfo beatmapInfo)
            {
                this.beatmapInfo = beatmapInfo;
            }

            [BackgroundDependencyLoader]
            private void load(RulesetStore rulesets)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(4, 0),
                    Children = new[]
                    {
                        (rulesets.GetRuleset(beatmapInfo.Ruleset.OnlineID)?.CreateInstance().CreateIcon() ?? new SpriteIcon { Icon = FontAwesome.Regular.QuestionCircle }).With(icon =>
                        {
                            icon.Anchor = icon.Origin = Anchor.CentreLeft;
                            icon.Size = new Vector2(16);
                        }),
                        new StarRatingDisplay(new StarDifficulty(beatmapInfo.StarRating, 0), StarRatingDisplaySize.Small)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft
                        },
                        new LinkFlowContainer(s =>
                        {
                            s.Font = OsuFont.Default.With(size: 14, weight: FontWeight.SemiBold);
                        }).With(d =>
                        {
                            d.AutoSizeAxes = Axes.Both;
                            d.Anchor = Anchor.CentreLeft;
                            d.Origin = Anchor.CentreLeft;
                            d.Padding = new MarginPadding { Bottom = 2 };
                            d.AddLink(beatmapInfo.DifficultyName, LinkAction.OpenBeatmap, beatmapInfo.OnlineID.ToString());
                        })
                    }
                };
            }
        }
    }
}
