// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Components;
using osuTK;

namespace osu.Game.Skinning.Editor
{
    public class SkinComponentToolbox : EditorSidebarSection
    {
        public Action<Type> RequestPlacement;

        private readonly CompositeDrawable target;

        public SkinComponentToolbox(CompositeDrawable target = null)
            : base("Components")
        {
            this.target = target;
        }

        private FillFlowContainer fill;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = fill = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(2)
            };

            reloadComponents();
        }

        private void reloadComponents()
        {
            fill.Clear();

            var skinnableTypes = typeof(OsuGame).Assembly.GetTypes()
                                                .Where(t => !t.IsInterface)
                                                .Where(t => typeof(ISkinnableDrawable).IsAssignableFrom(t))
                                                .OrderBy(t => t.Name)
                                                .ToArray();

            foreach (var type in skinnableTypes)
                attemptAddComponent(type);
        }

        private void attemptAddComponent(Type type)
        {
            try
            {
                var instance = (Drawable)Activator.CreateInstance(type);

                Debug.Assert(instance != null);

                if (!((ISkinnableDrawable)instance).IsEditable) return;

                fill.Add(new ToolboxComponentButton(instance, target)
                {
                    RequestPlacement = t => RequestPlacement?.Invoke(t)
                });
            }
            catch (DependencyNotRegisteredException)
            {
                // This loading code relies on try-catching any dependency injection errors to know which components are valid for the current target screen.
                // If a screen can't provide the required dependencies, a skinnable component should not be displayed in the list.
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Skin component {type} could not be loaded in the editor component list due to an error");
            }
        }

        private class ToolboxComponentButton : OsuButton
        {
            protected override bool ShouldBeConsideredForInput(Drawable child) => false;

            public override bool PropagateNonPositionalInputSubTree => false;

            private readonly Drawable component;
            private readonly CompositeDrawable dependencySource;

            public Action<Type> RequestPlacement;

            private Container innerContainer;

            private const float contracted_size = 60;
            private const float expanded_size = 120;

            public ToolboxComponentButton(Drawable component, CompositeDrawable dependencySource)
            {
                this.component = component;
                this.dependencySource = dependencySource;

                Enabled.Value = true;

                RelativeSizeAxes = Axes.X;
                Height = contracted_size;
            }

            protected override bool OnHover(HoverEvent e)
            {
                this.Delay(300).ResizeHeightTo(expanded_size, 500, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                this.ResizeHeightTo(contracted_size, 500, Easing.OutQuint);
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider, OsuColour colours)
            {
                BackgroundColour = colourProvider.Background3;

                AddRange(new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(10) { Bottom = 20 },
                        Masking = true,
                        Child = innerContainer = new DependencyBorrowingContainer(dependencySource)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = component
                        },
                    },
                    new OsuSpriteText
                    {
                        Text = component.GetType().Name,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Margin = new MarginPadding(5),
                    },
                });

                // adjust provided component to fit / display in a known state.
                component.Anchor = Anchor.Centre;
                component.Origin = Anchor.Centre;
            }

            protected override void Update()
            {
                base.Update();

                if (component.DrawSize != Vector2.Zero)
                {
                    float bestScale = Math.Min(
                        innerContainer.DrawWidth / component.DrawWidth,
                        innerContainer.DrawHeight / component.DrawHeight);

                    innerContainer.Scale = new Vector2(bestScale);
                }
            }

            protected override bool OnClick(ClickEvent e)
            {
                RequestPlacement?.Invoke(component.GetType());
                return true;
            }
        }

        public class DependencyBorrowingContainer : Container
        {
            private readonly CompositeDrawable donor;

            public DependencyBorrowingContainer(CompositeDrawable donor)
            {
                this.donor = donor;
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
                new DependencyContainer(donor?.Dependencies ?? base.CreateChildDependencies(parent));
        }
    }
}
