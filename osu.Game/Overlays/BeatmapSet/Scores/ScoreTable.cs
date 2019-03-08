// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoreTable : CompositeDrawable
    {
        private readonly FillFlowContainer scoresFlow;

        public ScoreTable()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = scoresFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical
            };
        }

        public IEnumerable<APIScoreInfo> Scores
        {
            set
            {
                scoresFlow.Clear();

                if (value == null || !value.Any())
                    return;

                int maxModsAmount = 0;
                foreach (var s in value)
                {
                    var scoreModsAmount = s.Mods.Length;
                    if (scoreModsAmount > maxModsAmount)
                        maxModsAmount = scoreModsAmount;
                }

                scoresFlow.Add(new ScoreTextLine(maxModsAmount));

                int index = 0;
                foreach (var s in value)
                    scoresFlow.Add(new DrawableScore(index++, s, maxModsAmount));
            }
        }
    }
}
