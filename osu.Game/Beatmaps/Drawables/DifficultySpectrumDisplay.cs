// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Beatmaps.Drawables
{
    public partial class DifficultySpectrumDisplay : CompositeDrawable
    {
        private IBeatmapSetInfo? beatmapSet;

        public IBeatmapSetInfo? BeatmapSet
        {
            get => beatmapSet;
            set
            {
                beatmapSet = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        private FillFlowContainer<RulesetDifficultyGroup> flow = null!;

        private const int max_difficulties_before_collapsing = 12;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = flow = new FillFlowContainer<RulesetDifficultyGroup>
            {
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(10, 0),
                Direction = FillDirection.Horizontal,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateDisplay();
        }

        private void updateDisplay()
        {
            foreach (var group in flow)
                group.Alpha = 0;

            if (beatmapSet == null)
            {
                foreach (var group in flow)
                    group.Beatmaps = [];
                return;
            }

            // matching web: https://github.com/ppy/osu-web/blob/d06d8c5e735eb1f48799b1654b528e9a7afb0a35/resources/assets/lib/beatmapset-panel.tsx#L127
            bool collapsed = beatmapSet.Beatmaps.Count() > max_difficulties_before_collapsing;

            foreach (var rulesetGrouping in beatmapSet.Beatmaps.GroupBy(beatmap => beatmap.Ruleset).OrderBy(group => group.Key))
            {
                int rulesetId = rulesetGrouping.Key.OnlineID;

                var group = flow.SingleOrDefault(rg => rg.RulesetId == rulesetId);

                if (group == null)
                {
                    group = new RulesetDifficultyGroup(rulesetId);
                    flow.Add(group);
                    flow.SetLayoutPosition(group, rulesetId);
                }

                group.Alpha = 1;
                group.Beatmaps = rulesetGrouping.ToArray();
                group.Collapsed = collapsed;
            }
        }

        private partial class RulesetDifficultyGroup : FillFlowContainer
        {
            public readonly int RulesetId;

            private IBeatmapInfo[] beatmaps = [];

            public IBeatmapInfo[] Beatmaps
            {
                set
                {
                    beatmaps = value.OrderBy(bi => bi.StarRating).ToArray();
                    updateDisplay();
                }
            }

            private bool collapsed;

            public bool Collapsed
            {
                get => collapsed;
                set
                {
                    collapsed = value;
                    updateDisplay();
                }
            }

            private OsuSpriteText countText = null!;

            public RulesetDifficultyGroup(int rulesetId)
            {
                RulesetId = rulesetId;
            }

            [BackgroundDependencyLoader]
            private void load(RulesetStore rulesets)
            {
                AutoSizeAxes = Axes.Both;
                Spacing = new Vector2(1, 0);
                Direction = FillDirection.Horizontal;

                var icon = rulesets.GetRuleset(RulesetId)?.CreateInstance().CreateIcon() ?? new SpriteIcon { Icon = FontAwesome.Regular.QuestionCircle };
                Add(icon.With(i =>
                {
                    i.Size = new Vector2(14);
                    i.Anchor = i.Origin = Anchor.Centre;
                }));

                for (int i = 0; i < max_difficulties_before_collapsing; i++)
                    Add(new DifficultyDot());

                Add(countText = new OsuSpriteText
                {
                    Font = OsuFont.Style.Caption1,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding { Bottom = 1 }
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                updateDisplay();
            }

            private void updateDisplay()
            {
                countText.Alpha = collapsed ? 1 : 0;
                countText.Text = beatmaps.Length.ToLocalisableString(@"N0");

                var dots = this.OfType<DifficultyDot>().ToArray();

                for (int i = 0; i < max_difficulties_before_collapsing; i++)
                {
                    var dot = dots[i];

                    if (collapsed || i >= beatmaps.Length)
                    {
                        dot.Alpha = 0;
                        continue;
                    }

                    dot.Alpha = 1;
                    dot.StarDifficulty = beatmaps[i].StarRating;
                }
            }
        }

        private partial class DifficultyDot : Circle
        {
            private double starDifficulty;

            public double StarDifficulty
            {
                get => starDifficulty;
                set
                {
                    starDifficulty = value;
                    updateColour();
                }
            }

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                Size = new Vector2(5, 10);
                Anchor = Origin = Anchor.Centre;

                updateColour();
            }

            private void updateColour()
            {
                Colour = colours.ForStarDifficulty(starDifficulty);
            }
        }
    }
}
