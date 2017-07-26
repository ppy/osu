// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Overlays.Profile.Sections;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Desktop.VisualTests.Tests
{
    public class TestCaseUserRanks : TestCase
    {
        public override string Description => "showing your latest achievements";

        public TestCaseUserRanks()
        {
            RanksSection ranks;

            Add(new Container
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(0.2f)
                    },
                    ranks = new RanksSection(),
                }
            });

            AddStep("Add First Place", () => ranks.FirstPlacePlays = new[]
            {
                new Play
                {
                    Rank = ScoreRank.A,
                    PerformancePoints = 666,
                    Accuracy = 0.735,
                    Date = DateTimeOffset.UtcNow,
                    Mods = new Mod[] { new ModAutoplay(), new ModDoubleTime() },
                    Beatmap = new BeatmapInfo
                    {
                        Metadata = new BeatmapMetadata
                        {
                            Title = "FREEDOM DiVE",
                            Artist = "xi"
                        },
                        Version = "FOUR DIMENSIONS",
                        OnlineBeatmapID = 129891,
                    }
                }
            });

            AddStep("Add Best Performances", () =>
            {
                List<Play> plays = new List<Play>();
                Mod[] availableMods = { new OsuModHidden(), new OsuModFlashlight(), new OsuModHardRock(), new OsuModDoubleTime(), new OsuModPerfect() };
                List<Mod> selectedMods = new List<Mod>(availableMods);
                for (int i = 0; i <= availableMods.Length; i++)
                {
                    plays.Add(new Play
                    {
                        Rank = (ScoreRank) Enum.GetValues(typeof(ScoreRank)).GetValue(Enum.GetValues(typeof(ScoreRank)).Length - 1 - i),
                        PerformancePoints = (int)(Math.Pow(0.50, i) * 800),
                        Accuracy = Math.Pow(0.99, i),
                        Date = DateTimeOffset.UtcNow.AddDays(-Math.Pow(i, 2)),
                        Mods = selectedMods.ToList(),
                        Beatmap = new BeatmapInfo
                        {
                            Metadata = new BeatmapMetadata
                            {
                                Title = "Highscore",
                                Artist = "Panda Eyes & Teminite"
                            },
                            Version = "Game Over",
                            OnlineBeatmapID = 736215,
                        }
                    });
                    if(i < availableMods.Length)
                        selectedMods.Remove(availableMods[i]);
                }
                ranks.BestPlays = plays;
            });
        }
    }
}
