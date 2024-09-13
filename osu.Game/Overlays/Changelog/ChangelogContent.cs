// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Changelog
{
    public partial class ChangelogContent : FillFlowContainer
    {
        public Action<APIChangelogBuild>? BuildSelected;

        public void SelectBuild(APIChangelogBuild build) => BuildSelected?.Invoke(build);

        public ChangelogContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
        }
    }
}
