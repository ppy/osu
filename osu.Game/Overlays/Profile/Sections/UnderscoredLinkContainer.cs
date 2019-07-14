// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using System;
using System.Collections.Generic;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class UnderscoredLinkContainer : Container
    {
        private const int duration = 200;
        private readonly Container underscore;
        private readonly FillFlowContainer<OsuSpriteText> textContent;

        protected OsuGame Game;

        protected Action ClickAction;

        public IReadOnlyList<OsuSpriteText> Text
        {
            get => textContent.Children;
            set
            {
                textContent.Clear();
                textContent.AddRange(value);
            }
        }

        protected UnderscoredLinkContainer()
        {
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
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    },
                    textContent = new FillFlowContainer<OsuSpriteText>
                    {
                        Direction = FillDirection.Horizontal,
                        AutoSizeAxes = Axes.Both,
                    },
                },
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game)
        {
            Game = game;
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
