﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Components
{
    public class BottomBarContainer : Container
    {
        private const float corner_radius = 5;
        private const float contents_padding = 15;

        protected readonly IBindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();
        protected Track Track => Beatmap.Value.Track;

        private readonly Drawable background;
        private readonly Container content;

        protected override Container<Drawable> Content => content;

        public BottomBarContainer()
        {
            Masking = true;
            CornerRadius = corner_radius;

            InternalChildren = new[]
            {
                background = new Box { RelativeSizeAxes = Axes.Both },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = contents_padding },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap beatmap, OsuColour colours)
        {
            Beatmap.BindTo(beatmap);
            background.Colour = colours.Gray1;
        }
    }
}
