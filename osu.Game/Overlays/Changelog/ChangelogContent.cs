// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using System;
using osu.Framework.Graphics.Cursor;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogContent : PopoverContainer
    {
        public Action<APIChangelogBuild> BuildSelected;

        public void SelectBuild(APIChangelogBuild build) => BuildSelected?.Invoke(build);

        protected override Container<Drawable> Content { get; }

        public ChangelogContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            base.Content.Add(Content = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical
            });
        }
    }
}
