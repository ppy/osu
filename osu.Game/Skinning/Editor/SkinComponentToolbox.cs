// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning.Editor
{
    public class SkinComponentToolbox : CompositeDrawable
    {
        public SkinComponentToolbox()
        {
            RelativeSizeAxes = Axes.Y;
            Width = 500;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            FillFlowContainer fill;

            InternalChild = new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = fill = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = 0.5f,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(20)
                }
            };

            var skinnableTypes = typeof(OsuGame).Assembly.GetTypes().Where(t => typeof(ISkinnableComponent).IsAssignableFrom(t)).ToArray();

            foreach (var type in skinnableTypes)
            {
                var container = attemptAddComponent(type);
                if (container != null)
                    fill.Add(container);
            }
        }

        private static Drawable attemptAddComponent(Type type)
        {
            try
            {
                var instance = (Drawable)Activator.CreateInstance(type);

                Debug.Assert(instance != null);

                return new ToolboxComponent(instance);
            }
            catch
            {
            }

            return null;
        }

        private class ToolboxComponent : CompositeDrawable
        {
            public ToolboxComponent(Drawable instance)
            {
                Container innerContainer;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText { Text = instance.GetType().Name },
                        innerContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Masking = true,
                            CornerRadius = 10,
                            Children = new[]
                            {
                                new Box
                                {
                                    Colour = Color4.Black,
                                    Alpha = 0.5f,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                instance
                            }
                        },
                    }
                };

                // adjust provided component to fit / display in a known state.

                instance.Anchor = Anchor.Centre;
                instance.Origin = Anchor.Centre;

                if (instance.RelativeSizeAxes != Axes.None)
                {
                    innerContainer.AutoSizeAxes = Axes.None;
                    innerContainer.Height = 100;
                }

                switch (instance)
                {
                    case IScoreCounter score:
                        score.Current.Value = 133773;
                        break;

                    case IComboCounter combo:
                        combo.Current.Value = 727;
                        break;
                }
            }
        }
    }
}
