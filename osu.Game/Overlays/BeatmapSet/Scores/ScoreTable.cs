// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using System.Collections.Generic;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoreTable : FillFlowContainer
    {
        private IEnumerable<APIScoreInfo> scores;
        public IEnumerable<APIScoreInfo> Scores
        {
            set
            {
                scores = value;

                int maxModsAmount = 0;
                foreach (var s in scores)
                {
                    var scoreModsAmount = s.Mods.Length;
                    if (scoreModsAmount > maxModsAmount)
                        maxModsAmount = scoreModsAmount;
                }

                Add(new ScoreTextLine(maxModsAmount));


                int index = 0;
                foreach (var s in scores)
                    Add(new DrawableScore(index++, s, maxModsAmount));
            }
            get
            {
                return scores;
            }
        }

        public ScoreTable()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
        }

        public void ClearScores()
        {
            scores = null;
            foreach (var s in this)
            {
                if (s is DrawableScore)
                    Remove(s);
            }
        }
    }
}
