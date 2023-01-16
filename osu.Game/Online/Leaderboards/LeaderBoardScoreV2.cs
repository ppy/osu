// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Online.Leaderboards
{
    public partial class LeaderBoardScoreV2 : OsuClickableContainer
    {
        private const int HEIGHT = 60;
        private const int corner_radius = 10;

        private static readonly Vector2 shear = new Vector2(0.15f, 0);

        private Container content = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Shear = shear;
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
            Child = content = new Container
            {
                Masking = true,
                CornerRadius = corner_radius,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            };
        }
    }
}
