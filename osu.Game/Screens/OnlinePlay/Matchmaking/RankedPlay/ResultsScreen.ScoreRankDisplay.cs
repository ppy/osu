// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class ResultsScreen
    {
        private partial class ScoreRankDisplay : CompositeDrawable
        {
            private readonly ScoreInfo score;

            public ScoreRankDisplay(ScoreInfo score)
            {
                this.score = score;

                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(SkinManager skinManager)
            {
                InternalChild = new Sprite
                {
                    Scale = new Vector2(0.5f),
                    Texture = skinManager.DefaultClassicSkin.GetTexture(DrawableRank.GetLegacyRankTextureName(score.Rank))
                };
            }
        }
    }
}
