// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Rulesets;
using osu.Game.Screens.Edit.Components;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays.SkinEditor
{
    public partial class SkinComponentToolbox : EditorSidebarSection
    {
        public Action<Type>? RequestPlacement;

        private readonly ISerialisableDrawableContainer target;

        private readonly RulesetInfo? ruleset;

        private FillFlowContainer fill = null!;

        /// <summary>
        /// Create a new component toolbox for the specified taget.
        /// </summary>
        /// <param name="target">The target. This is mainly used as a dependency source to find candidate components.</param>
        /// <param name="ruleset">A ruleset to filter components by. If null, only components which are not ruleset-specific will be included.</param>
        public SkinComponentToolbox(ISerialisableDrawableContainer target, RulesetInfo? ruleset)
            : base(ruleset == null ? SkinEditorStrings.Components : LocalisableString.Interpolate($"{SkinEditorStrings.Components} ({ruleset.Name})"))
        {
            this.target = target;
            this.ruleset = ruleset;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = fill = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(EditorSidebar.PADDING)
            };

            reloadComponents();
        }

        private void reloadComponents()
        {
            fill.Clear();

            var skinnableTypes = SerialisedDrawableInfo.GetAllAvailableDrawables(ruleset);
            foreach (var type in skinnableTypes)
                attemptAddComponent(type);
        }

        private void attemptAddComponent(Type type)
        {
            try
            {
                Drawable instance = (Drawable)Activator.CreateInstance(type)!;

                if (!((ISerialisableDrawable)instance).IsPlaceable) return;

                fill.Add(new ToolboxComponentButton(instance, (CompositeDrawable)target)
                {
                    RequestPlacement = t => RequestPlacement?.Invoke(t),
                    Expanding = contractOtherButtons,
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

        private void contractOtherButtons(ToolboxComponentButton obj)
        {
            foreach (var b in fill.OfType<ToolboxComponentButton>())
            {
                if (b == obj)
                    continue;

                b.Contract();
            }
        }

        public partial class ToolboxComponentButton : OsuButton
        {
            public Action<Type>? RequestPlacement;
            public Action<ToolboxComponentButton>? Expanding;

            private readonly Drawable component;
            private readonly CompositeDrawable? dependencySource;

            private Container innerContainer = null!;

            private ScheduledDelegate? expandContractAction;

            private const float contracted_size = 60;
            private const float expanded_size = 120;

            public ToolboxComponentButton(Drawable component, CompositeDrawable? dependencySource)
            {
                this.component = component;
                this.dependencySource = dependencySource;

                Enabled.Value = true;

                RelativeSizeAxes = Axes.X;
                Height = contracted_size;
            }

            private const double animation_duration = 500;

            protected override bool OnHover(HoverEvent e)
            {
                expandContractAction?.Cancel();
                expandContractAction = Scheduler.AddDelayed(() =>
                {
                    this.ResizeHeightTo(expanded_size, animation_duration, Easing.OutQuint);
                    Expanding?.Invoke(this);
                }, 100);

                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                expandContractAction?.Cancel();
                // If no other component is selected for too long, force a contract.
                // Otherwise we will generally contract when Contract() is called from outside.
                expandContractAction = Scheduler.AddDelayed(Contract, 1000);
            }

            public void Contract()
            {
                // Cheap debouncing to avoid stacking animations.
                // The only place this is nulled is at the end of this method.
                if (expandContractAction == null)
                    return;

                this.ResizeHeightTo(contracted_size, animation_duration, Easing.OutQuint);

                expandContractAction?.Cancel();
                expandContractAction = null;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
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

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

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

        public partial class DependencyBorrowingContainer : Container
        {
            protected override bool ShouldBeConsideredForInput(Drawable child) => false;

            public override bool PropagateNonPositionalInputSubTree => false;

            private readonly CompositeDrawable? donor;

            public DependencyBorrowingContainer(CompositeDrawable? donor)
            {
                this.donor = donor;
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
                new DependencyContainer(donor?.Dependencies ?? base.CreateChildDependencies(parent));
        }
    }
}
