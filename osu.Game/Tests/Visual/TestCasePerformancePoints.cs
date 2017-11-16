// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Music;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Visual
{
    public abstract class TestCasePerformancePoints : OsuTestCase
    {
        public TestCasePerformancePoints(Ruleset ruleset)
        {
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = 300,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.5f,
                        },
                        new ScrollContainer(Direction.Vertical)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = new BeatmapList(ruleset)
                        }
                    }
                },
                new PpDisplay(ruleset)
            };
        }

        private class BeatmapList : CompositeDrawable
        {
            private readonly Container<BeatmapDisplay> beatmapDisplays;
            private readonly Ruleset ruleset;

            public BeatmapList(Ruleset ruleset)
            {
                this.ruleset = ruleset;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                InternalChild = beatmapDisplays = new FillFlowContainer<BeatmapDisplay>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 4)
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuGameBase osuGame, BeatmapManager beatmaps)
            {
                var sets = beatmaps.GetAllUsableBeatmapSets();
                var allBeatmaps = sets.SelectMany(s => s.Beatmaps).Where(b => ruleset.LegacyID < 0 || b.RulesetID == ruleset.LegacyID);

                allBeatmaps.ForEach(b => beatmapDisplays.Add(new BeatmapDisplay(b)));
            }

            private class BeatmapDisplay : CompositeDrawable
            {
                private readonly OsuSpriteText text;
                private readonly BeatmapInfo beatmap;

                private BeatmapManager beatmaps;
                private OsuGameBase osuGame;

                private bool isSelected;

                public BeatmapDisplay(BeatmapInfo beatmap)
                {
                    this.beatmap = beatmap;

                    AutoSizeAxes = Axes.Both;
                    InternalChild = text = new OsuSpriteText();
                }

                protected override bool OnClick(InputState state)
                {
                    if (osuGame.Beatmap.Value.BeatmapInfo.ID == beatmap.ID)
                        return false;

                    osuGame.Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap);
                    isSelected = true;
                    return true;
                }

                protected override bool OnHover(InputState state)
                {
                    if (isSelected)
                        return false;
                    this.FadeColour(Color4.Yellow, 100);
                    return true;
                }

                protected override void OnHoverLost(InputState state)
                {
                    if (isSelected)
                        return;
                    this.FadeColour(Color4.White, 100);
                }

                [BackgroundDependencyLoader]
                private void load(OsuGameBase osuGame, BeatmapManager beatmaps)
                {
                    this.osuGame = osuGame;
                    this.beatmaps = beatmaps;

                    var working = beatmaps.GetWorkingBeatmap(beatmap);
                    text.Text = $"{beatmap.Metadata.Artist} - {beatmap.Metadata.Title} ({beatmap.Metadata.AuthorString}) [{beatmap.Version}]";

                    osuGame.Beatmap.ValueChanged += beatmapChanged;
                }

                private void beatmapChanged(WorkingBeatmap newBeatmap)
                {
                    if (isSelected)
                        this.FadeColour(Color4.White, 100);
                    isSelected = false;
                }
            }
        }

        private class PpDisplay : CompositeDrawable
        {
            private readonly Container<OsuSpriteText> strainsContainer;
            private readonly OsuSpriteText totalPp;

            private readonly Ruleset ruleset;

            public PpDisplay(Ruleset ruleset)
            {
                this.ruleset = ruleset;

                RelativeSizeAxes = Axes.Y;
                Width = 400;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.2f
                    },
                    totalPp = new OsuSpriteText { TextSize = 18 },
                    new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Y = 26,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black,
                                Alpha = 0.2f,
                            },
                            strainsContainer = new FillFlowContainer<OsuSpriteText>
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 5)
                            }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuGameBase osuGame)
            {
                osuGame.Beatmap.ValueChanged += beatmapChanged;
            }

            private void beatmapChanged(WorkingBeatmap beatmap)
            {
                var diffCalculator = ruleset.CreateDifficultyCalculator(beatmap.Beatmap);

                var strains = new Dictionary<string, string>();
                double pp = diffCalculator.Calculate(strains);

                totalPp.Text = $"Total PP: {pp.ToString("n2")}";

                strainsContainer.Clear();
                foreach (var kvp in strains)
                    strainsContainer.Add(new OsuSpriteText { Text = $"{kvp.Key} : {kvp.Value}" });
            }
        }
    }
}
