// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Lines;
using osuTK;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class ProgressionPath : Path
    {
        public DrawableTournamentMatch Source { get; private set; }
        public DrawableTournamentMatch Destination { get; private set; }

        public ProgressionPath(DrawableTournamentMatch source, DrawableTournamentMatch destination)
        {
            Source = source;
            Destination = destination;

            PathRadius = 3;
            BypassAutoSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Vector2 getCenteredVector(Vector2 top, Vector2 bottom) => new Vector2(top.X, top.Y + (bottom.Y - top.Y) / 2);

            var q1 = Source.ScreenSpaceDrawQuad;
            var q2 = Destination.ScreenSpaceDrawQuad;

            float padding = q1.Width / 20;

            bool progressionToRight = q2.TopLeft.X > q1.TopLeft.X;

            if (!progressionToRight)
            {
                var temp = q2;
                q2 = q1;
                q1 = temp;
            }

            var c1 = getCenteredVector(q1.TopRight, q1.BottomRight) + new Vector2(padding, 0);
            var c2 = getCenteredVector(q2.TopLeft, q2.BottomLeft) - new Vector2(padding, 0);

            var p1 = c1;
            var p2 = p1 + new Vector2(padding, 0);

            if (p2.X > c2.X)
            {
                c2 = getCenteredVector(q2.TopRight, q2.BottomRight) + new Vector2(padding, 0);
                p2.X = c2.X + padding;
            }

            var p3 = new Vector2(p2.X, c2.Y);
            var p4 = new Vector2(c2.X, p3.Y);

            var points = new[] { p1, p2, p3, p4 };

            float minX = points.Min(p => p.X);
            float minY = points.Min(p => p.Y);

            var topLeft = new Vector2(minX, minY);

            Position = Parent.ToLocalSpace(topLeft);
            Vertices = points.Select(p => Parent.ToLocalSpace(p) - Parent.ToLocalSpace(topLeft)).ToList();
        }
    }
}
