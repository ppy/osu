// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonScoreWedge : CompositeDrawable, ISerialisableDrawable
    {
        private SliderPath barPath = null!;

        private const float main_path_radius = 1f;

        public bool UsesFixedAnchor { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            const float bar_length = 430 - main_path_radius * 2;
            const float bar_top = 0;
            const float bar_bottom = 80;
            const float curve_start = bar_length - 105;
            const float curve_end = bar_length - 35;

            const float curve_smoothness = 10;

            Vector2 diagonalDir = (new Vector2(curve_end, bar_top) - new Vector2(curve_start, bar_bottom)).Normalized();

            barPath = new SliderPath(new[]
            {
                new PathControlPoint(new Vector2(0, bar_bottom), PathType.Linear),
                new PathControlPoint(new Vector2(curve_start - curve_smoothness, bar_bottom), PathType.Bezier),
                new PathControlPoint(new Vector2(curve_start, bar_bottom)),
                new PathControlPoint(new Vector2(curve_start, bar_bottom) + diagonalDir * curve_smoothness, PathType.Linear),
                new PathControlPoint(new Vector2(curve_end, bar_top) - diagonalDir * curve_smoothness, PathType.Bezier),
                new PathControlPoint(new Vector2(curve_end, bar_top)),
                new PathControlPoint(new Vector2(curve_end + curve_smoothness, bar_top), PathType.Linear),
                new PathControlPoint(new Vector2(bar_length, bar_top)),
            });

            var vertices = new List<Vector2>();
            barPath.GetPathToProgress(vertices, 0, 1);

            InternalChildren = new Drawable[]
            {
                new ArgonWedgePiece
                {
                    WedgeWidth = { Value = 380 },
                    WedgeHeight = { Value = 72 },
                },
                new ArgonWedgePiece
                {
                    WedgeWidth = { Value = 380 },
                    WedgeHeight = { Value = 72 },
                    Position = new Vector2(4, 5)
                },
                new SmoothPath
                {
                    Colour = Color4.White,
                    PathRadius = 1f,
                    Vertices = vertices,
                },
                new Circle
                {
                    Y = bar_bottom - 1.5f + main_path_radius,
                    Size = new Vector2(300f, 3f),
                }
            };
        }
    }
}
