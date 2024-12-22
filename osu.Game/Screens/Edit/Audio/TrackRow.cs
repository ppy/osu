// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class TrackRow : Container
    {
        private const int row_height = 40;

        public TrackRow()
        {
            RelativeSizeAxes = Axes.X;
            Height = row_height;
        }
    }
}
