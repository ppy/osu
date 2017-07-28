// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Rulesets.Scoring;
using System;
using System.Collections.Generic;

namespace osu.Game.Overlays.Profile.Sections
{
    public class RanksSection : ProfileSection
    {
        public override string Title => "Ranks";

        public override string Identifier => "top_ranks";

        private readonly FillFlowContainer<DrawableScore> best, firstPlace;

        public RanksSection()
        {
            Children = new Drawable[]
            {
                new OsuSpriteText {
                    TextSize = 15,
                    Text = "Best Performance",
                    Font = "Exo2.0-RegularItalic",
                },
                best = new FillFlowContainer<DrawableScore>
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                },
                new OsuSpriteText {
                    TextSize = 15,
                    Text = "First Place Ranks",
                    Font = "Exo2.0-RegularItalic",
                },
                firstPlace = new FillFlowContainer<DrawableScore>
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                },
            };
        }

        public IEnumerable<Score> BestScores
        {
            set
            {
                int i = 0;
                foreach (Score score in value)
                {
                    best.Add(new DrawableScore(score, Math.Pow(0.95, i))
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 60,
                    });
                    i++;
                }
            }
        }

        public IEnumerable<Score> FirstPlaceScores
        {
            set
            {
                foreach (Score score in value)
                    firstPlace.Add(new DrawableScore(score)
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 60,
                    });
            }
        }
    }
}
