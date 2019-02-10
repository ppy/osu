// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoreTextLine : ScoreTableLine
    {
        public ScoreTextLine(int maxModsAmount) : base(maxModsAmount)
        {
            RankContainer.Add(new ScoreText
            {
                Text = @"rank".ToUpper(),
            });
            ScoreContainer.Add(new ScoreText
            {
                Text = @"score".ToUpper(),
            });
            AccuracyContainer.Add(new ScoreText
            {
                Text = @"accuracy".ToUpper(),
            });
            PlayerContainer.Add(new ScoreText
            {
                Text = @"player".ToUpper(),
            });
            MaxComboContainer.Add(new ScoreText
            {
                Text = @"max combo".ToUpper(),
            });
            HitGreatContainer.Add(new ScoreText
            {
                Text = "300".ToUpper(),
            });
            HitGoodContainer.Add(new ScoreText
            {
                Text = "100".ToUpper(),
            });
            HitMehContainer.Add(new ScoreText
            {
                Text = "50".ToUpper(),
            });
            HitMissContainer.Add(new ScoreText
            {
                Text = @"misses".ToUpper(),
            });
            PPContainer.Add(new ScoreText
            {
                Text = @"pp".ToUpper(),
            });
            ModsContainer.Add(new ScoreText
            {
                Text = @"mods".ToUpper(),
            });
        }

        private class ScoreText : SpriteText
        {
            private const float text_size = 12;

            public ScoreText()
            {
                TextSize = text_size;
                Font = @"Exo2.0-Black";
            }
        }
    }
}
