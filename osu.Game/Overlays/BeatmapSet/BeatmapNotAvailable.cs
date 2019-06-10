// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Text;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapNotAvailable : Container
    {
        private LinkFlowContainer linkContainer;

        public override void Show()
        {
            AutoSizeAxes = Axes.Both;
            Margin = new MarginPadding() { Top = 10 };

            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black.Opacity(0.6f),
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Margin = new MarginPadding() { Top = 10, Left = 5, Right = 20 },

                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Margin = new MarginPadding() { Bottom = 10, Horizontal = 5 },
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Medium),
                            Text = "This beatmap is currently not available for download.",
                            Colour = Color4.Orange,
                        },
                        linkContainer = new LinkFlowContainer(text => text.Font = OsuFont.GetFont(size: 14))
                        {
                            Direction = FillDirection.Full,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Bottom = 10, Horizontal = 5 },
                        },
                    },
                },
            };

            base.Show();
        }

        public string Link
        {
            set => linkContainer.AddLink("Check here for more information.", value);
        }
    }
}
