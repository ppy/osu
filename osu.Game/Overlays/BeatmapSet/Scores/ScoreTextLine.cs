// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoreTextLine : Container
    {
        private const float text_size = 12;

        public const float RANK_POSITION = 30;
        public const float SCORE_POSITION = 90;
        public const float ACCURACY_POSITION = 170;
        public const float PLAYER_POSITION = 270;
        public const float MAX_COMBO_POSITION = 0.5f;
        public const float HIT_GREAT_POSITION = 0.6f;
        public const float HIT_GOOD_POSITION = 0.65f;
        public const float HIT_MEH_POSITION = 0.7f;
        public const float HIT_MISS_POSITION = 0.75f;
        public const float PP_POSITION = 0.8f;
        public const float MODS_POSITION = 0.9f;

        public ScoreTextLine()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                new ScoreText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,
                    Text = "rank".ToUpper(),
                    X = RANK_POSITION,
                },
                new ScoreText
                {
                    Text = "score".ToUpper(),
                    X = SCORE_POSITION,
                },
                new ScoreText
                {
                    Text = "accuracy".ToUpper(),
                    X = ACCURACY_POSITION,
                },
                new ScoreText
                {
                    Text = "player".ToUpper(),
                    X = PLAYER_POSITION,
                },
                new ScoreText
                {
                    Text = "max combo".ToUpper(),
                    X = MAX_COMBO_POSITION,
                    RelativePositionAxes = Axes.X,
                },
                new ScoreText
                {
                    Text = "300",
                    RelativePositionAxes = Axes.X,
                    X = HIT_GREAT_POSITION,
                },
                new ScoreText
                {
                    Text = "100".ToUpper(),
                    RelativePositionAxes = Axes.X,
                    X = HIT_GOOD_POSITION,
                },
                new ScoreText
                {
                    Text = "50".ToUpper(),
                    RelativePositionAxes = Axes.X,
                    X = HIT_MEH_POSITION,
                },
                new ScoreText
                {
                    Text = "miss".ToUpper(),
                    RelativePositionAxes = Axes.X,
                    X = HIT_MISS_POSITION,
                },
                new ScoreText
                {
                    Text = "pp".ToUpper(),
                    RelativePositionAxes = Axes.X,
                    X = PP_POSITION,
                },
                new ScoreText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    Text = "mods".ToUpper(),
                    X = MODS_POSITION,
                    RelativePositionAxes = Axes.X,
                },
            };
        }

        private class ScoreText : SpriteText
        {
            public ScoreText()
            {
                TextSize = text_size;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour = colours.ContextMenuGray;
            }
        }
    }
}
