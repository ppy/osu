// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using System;

namespace osu.Game.Overlays.Profile.Sections
{
    public class ClickableLink : Container
    {
        private const int duration = 300;

        protected readonly BeatmapInfo Beatmap;
        protected Action ClickAction;
        private Container underscore;
        protected FillFlowContainer TextContent;
        protected Box UnderscoreBackground;

        public ClickableLink(BeatmapInfo beatmap)
        {
            Beatmap = beatmap;
            AutoSizeAxes = Axes.Both;
            Child = new Container
            {
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    underscore = new Container
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.X,
                        Height = 1,
                        Alpha = 0,
                        AlwaysPresent = true,
                        Child = UnderscoreBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    },
                    TextContent = new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        AutoSizeAxes = Axes.Both,
                    },
                },
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            underscore.FadeIn(duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            underscore.FadeOut(duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            ClickAction?.Invoke();
            return base.OnClick(e);
        }
    }
}
