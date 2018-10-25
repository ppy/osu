// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens;
using osu.Mods.Evast.CursorTracers;
using osu.Mods.Evast.Easings;
using osu.Mods.Evast.Galaga;
using osu.Mods.Evast.LifeGame;
using osu.Mods.Evast.MusicVisualizers;
using osu.Mods.Evast.Numbers;
using osu.Mods.Evast.Particles;
using osu.Mods.Evast.Snake;
using osu.Mods.Evast.Tetris;
using osu.Mods.Evast.Visualizers;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Mods.Evast
{
    public class MoreScreen : OsuScreen
    {
        public MoreScreen()
        {
            Child = new ScrollContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Width = 600,
                RelativeSizeAxes = Axes.Y,
                ScrollbarOverlapsContent = false,
                Child = new FillFlowContainer<ScreenButton>
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(20),
                    Children = new[]
                    {
                        new ScreenButton(@"2048", () => Push(new NumbersGameScreen())),
                        new ScreenButton(@"Music Visualizers", () => Push(new VisualizerVariationsScreen())),
                        new ScreenButton(@"Visualizer Bar Variations", () => Push(new VisualizerBarVariationsScreen())),
                        new ScreenButton(@"Circular Visualizer Test", () => Push(new CircularVisualizerTestScreen())),
                        new ScreenButton(@"Linear Visualizer Test", () => Push(new LinearVisualizerTestScreen())),
                        new ScreenButton(@"Visualizer", () => Push(new Visualizer())),
                        new ScreenButton(@"Space Particles", () => Push(new SpaceParticlesTestScreen())),
                        new ScreenButton(@"Easings", () => Push(new EasingsTestScreen())),
                        new ScreenButton(@"Cursor Tracer", () => Push(new CursorTracerTestScreen())),
                        new ScreenButton(@"Particles", () => Push(new ParticlesTestScreen())),
                        new ScreenButton(@"Game of Life", () => Push(new LifeGameScreen())),
                        new ScreenButton(@"Snake", () => Push(new SnakeScreen())),
                        new ScreenButton(@"Tetris", () => Push(new TetrisScreen())),
                        new ScreenButton(@"Galaga", () => Push(new GalagaTestScreen())),
                    },
                }
            };
        }

        private class ScreenButton : OsuClickableContainer
        {
            private readonly Box hover;
            private readonly OsuSpriteText text;

            public ScreenButton(string title, Action clickAction = null)
            {
                Action = clickAction;

                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                RelativeSizeAxes = Axes.X;
                Height = 100;
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(130),
                    },
                    hover = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Colour = Color4.White.Opacity(50),
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = title,
                        TextSize = 25,
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                text.Colour = colours.Yellow;
            }

            protected override bool OnHover(HoverEvent e)
            {
                hover.FadeIn(100);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                hover.FadeOut(100);
                base.OnHoverLost(e);
            }
        }
    }
}
