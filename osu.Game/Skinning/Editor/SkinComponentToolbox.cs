// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning.Editor
{
    public class SkinComponentToolbox : ScrollingToolboxGroup
    {
        public Action<Type> RequestPlacement;

        public SkinComponentToolbox(float height)
            : base("Components", height)
        {
            RelativeSizeAxes = Axes.None;
            Width = 200;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            FillFlowContainer fill;

            Child = fill = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(20)
            };

            var skinnableTypes = typeof(OsuGame).Assembly.GetTypes().Where(t => typeof(ISkinnableComponent).IsAssignableFrom(t)).ToArray();

            foreach (var type in skinnableTypes)
            {
                var component = attemptAddComponent(type);

                if (component != null)
                {
                    component.RequestPlacement = t => RequestPlacement?.Invoke(t);
                    fill.Add(component);
                }
            }
        }

        private static ToolboxComponent attemptAddComponent(Type type)
        {
            try
            {
                var instance = (Drawable)Activator.CreateInstance(type);

                Debug.Assert(instance != null);

                return new ToolboxComponent(instance);
            }
            catch
            {
                return null;
            }
        }

        private class ToolboxComponent : CompositeDrawable
        {
            private readonly Drawable component;
            private readonly Box box;

            public Action<Type> RequestPlacement;

            public ToolboxComponent(Drawable component)
            {
                this.component = component;
                Container innerContainer;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText { Text = component.GetType().Name },
                        innerContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Masking = true,
                            CornerRadius = 10,
                            Children = new[]
                            {
                                box = new Box
                                {
                                    Colour = Color4.Black,
                                    Alpha = 0.5f,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                component
                            }
                        },
                    }
                };

                // adjust provided component to fit / display in a known state.

                component.Anchor = Anchor.Centre;
                component.Origin = Anchor.Centre;

                if (component.RelativeSizeAxes != Axes.None)
                {
                    innerContainer.AutoSizeAxes = Axes.None;
                    innerContainer.Height = 100;
                }

                switch (component)
                {
                    case IScoreCounter score:
                        score.Current.Value = 133773;
                        break;

                    case IComboCounter combo:
                        combo.Current.Value = 727;
                        break;
                }
            }

            [Resolved]
            private OsuColour colours { get; set; }

            protected override bool OnClick(ClickEvent e)
            {
                RequestPlacement?.Invoke(component.GetType());
                return true;
            }

            protected override bool OnHover(HoverEvent e)
            {
                box.FadeColour(colours.Yellow, 100);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                box.FadeColour(Color4.Black, 100);
                base.OnHoverLost(e);
            }
        }
    }
}
