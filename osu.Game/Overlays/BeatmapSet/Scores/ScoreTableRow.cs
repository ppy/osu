// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public abstract class ScoreTableRow
    {
        protected const int TEXT_SIZE = 14;

        public IEnumerable<Drawable> CreateDrawables()
        {
            yield return new Container
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                AutoSizeAxes = Axes.Both,
                Child = CreateIndexCell()
            };

            yield return new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Child = CreateRankCell()
            };

            yield return new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding { Right = 20 },
                Child = CreateScoreCell()
            };

            yield return new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding { Right = 20 },
                Child = CreateAccuracyCell()
            };

            yield return new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding { Right = 20 },
                Child = CreatePlayerCell()
            };

            foreach (var cell in CreateStatisticsCells())
            {
                yield return new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Child = cell
                };
            }

            yield return new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                AutoSizeAxes = Axes.Both,
                Child = CreatePpCell()
            };

            yield return new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding { Right = 20 },
                Child = CreateModsCell()
            };
        }

        protected abstract Drawable CreateIndexCell();

        protected abstract Drawable CreateRankCell();

        protected abstract Drawable CreateScoreCell();

        protected abstract Drawable CreateAccuracyCell();

        protected abstract Drawable CreatePlayerCell();

        protected abstract IEnumerable<Drawable> CreateStatisticsCells();

        protected abstract Drawable CreatePpCell();

        protected abstract Drawable CreateModsCell();
    }
}
