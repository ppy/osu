// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoreTextLine : ScoreTableLine
    {
        private const float text_size = 12;

        public ScoreTextLine(int maxModsAmount) : base(maxModsAmount)
        {
            RankContainer.Add(new SpriteText
            {
                Text = @"rank".ToUpper(),
                TextSize = text_size,
            });
            ScoreContainer.Add(new SpriteText
            {
                Text = @"score".ToUpper(),
                TextSize = text_size,
            });
            AccuracyContainer.Add(new SpriteText
            {
                Text = @"accuracy".ToUpper(),
                TextSize = text_size,
            });
            PlayerContainer.Add(new SpriteText
            {
                Text = @"player".ToUpper(),
                TextSize = text_size,
            });
            MaxComboContainer.Add(new SpriteText
            {
                Text = @"max combo".ToUpper(),
                TextSize = text_size,
            });
            HitGreatContainer.Add(new SpriteText
            {
                Text = "300".ToUpper(),
                TextSize = text_size,
            });
            HitGoodContainer.Add(new SpriteText
            {
                Text = "100".ToUpper(),
                TextSize = text_size,
            });
            HitMehContainer.Add(new SpriteText
            {
                Text = "50".ToUpper(),
                TextSize = text_size,
            });
            HitMissContainer.Add(new SpriteText
            {
                Text = @"miss".ToUpper(),
                TextSize = text_size,
            });
            PPContainer.Add(new SpriteText
            {
                Text = @"pp".ToUpper(),
                TextSize = text_size,
            });
            ModsContainer.Add(new SpriteText
            {
                Text = @"mods".ToUpper(),
                TextSize = text_size,
            });
        }
    }
}
