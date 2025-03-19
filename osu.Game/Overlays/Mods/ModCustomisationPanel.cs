// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Mods
{
    public partial class ModCustomisationPanel : OverlayContainer, IKeyBindingHandler<GlobalAction>
    {
        private const float header_height = 42f;
        private const float content_vertical_padding = 20f;
        private const float content_border_thickness = 2f;

        private Container content = null!;
        private OsuScrollContainer scrollContainer = null!;
        private FillFlowContainer sectionsFlow = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public readonly BindableBool Enabled = new BindableBool();

        public readonly Bindable<ModCustomisationPanelState> ExpandedState = new Bindable<ModCustomisationPanelState>();

        public Bindable<IReadOnlyList<Mod>> SelectedMods { get; } = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        // Handle{Non}PositionalInput controls whether the panel should act as a blocking layer on the screen. only block when the panel is expanded.
        // These properties are used because they correctly handle blocking/unblocking hover when mouse is pointing at a drawable outside
        // (handling OnHover or overriding Block{Non}PositionalInput doesn't work).
        public override bool HandlePositionalInput => ExpandedState.Value != ModCustomisationPanelState.Collapsed;
        public override bool HandleNonPositionalInput => ExpandedState.Value != ModCustomisationPanelState.Collapsed;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new ModCustomisationHeader(this)
                {
                    Depth = float.MinValue,
                    RelativeSizeAxes = Axes.X,
                    Height = header_height,
                    Enabled = { BindTarget = Enabled },
                    ExpandedState = { BindTarget = ExpandedState },
                },
                content = new FocusGrabbingContainer(this)
                {
                    RelativeSizeAxes = Axes.X,
                    BorderColour = colourProvider.Dark3,
                    BorderThickness = content_border_thickness,
                    CornerRadius = 10f,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Offset = new Vector2(0f, 5f),
                        Radius = 20f,
                        Roundness = 5f,
                        Colour = Color4.Black.Opacity(0.25f),
                    },
                    ExpandedState = { BindTarget = ExpandedState },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Dark4,
                        },
                        scrollContainer = new OsuScrollContainer(Direction.Vertical)
                        {
                            RelativeSizeAxes = Axes.X,
                            Margin = new MarginPadding
                            {
                                Top = header_height + content_border_thickness,
                                Bottom = content_border_thickness
                            },
                            Child = sectionsFlow = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(0f, 40f),
                                Margin = new MarginPadding
                                {
                                    Top = content_vertical_padding,
                                    Bottom = 5f + content_vertical_padding
                                },
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Enabled.BindValueChanged(e =>
            {
                this.FadeColour(OsuColour.Gray(e.NewValue ? 1f : 0.6f), 300, Easing.OutQuint);
            }, true);

            ExpandedState.BindValueChanged(_ => updateDisplay(), true);
            SelectedMods.BindValueChanged(_ => updateMods(), true);

            FinishTransforms(true);
        }

        protected override void PopIn() => this.FadeIn(300, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(300, Easing.OutQuint);

        protected override bool OnClick(ClickEvent e)
        {
            ExpandedState.Value = ModCustomisationPanelState.Collapsed;
            return base.OnClick(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e) => true;

        protected override bool OnScroll(ScrollEvent e) => true;

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Back:
                    ExpandedState.Value = ModCustomisationPanelState.Collapsed;
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private void updateDisplay()
        {
            content.ClearTransforms();

            if (ExpandedState.Value != ModCustomisationPanelState.Collapsed)
            {
                content.AutoSizeDuration = 400;
                content.AutoSizeEasing = Easing.OutQuint;
                content.AutoSizeAxes = Axes.Y;
                content.FadeIn(120, Easing.OutQuint);
            }
            else
            {
                content.AutoSizeAxes = Axes.None;
                content.ResizeHeightTo(header_height, 400, Easing.OutQuint);
                content.FadeOut(400, Easing.OutSine);
            }
        }

        private void updateMods()
        {
            ExpandedState.Value = ModCustomisationPanelState.Collapsed;
            sectionsFlow.Clear();

            // Importantly, the selected mods bindable is already ordered by the mod select overlay (following the order of mod columns and panels).
            // Using AsOrdered produces a slightly different order (e.g. DT and NC no longer becoming adjacent),
            // which breaks user expectations when interacting with the overlay.
            foreach (var mod in SelectedMods.Value)
            {
                var settings = mod.CreateSettingsControls().ToList();

                if (settings.Count > 0)
                    sectionsFlow.Add(new ModCustomisationSection(mod, settings));
            }
        }

        protected override void Update()
        {
            base.Update();
            scrollContainer.Height = Math.Min(scrollContainer.AvailableContent, DrawHeight - header_height);
        }

        private partial class FocusGrabbingContainer : InputBlockingContainer
        {
            public readonly Bindable<ModCustomisationPanelState> ExpandedState = new Bindable<ModCustomisationPanelState>();

            public override bool RequestsFocus => panel.ExpandedState.Value != ModCustomisationPanelState.Collapsed;
            public override bool AcceptsFocus => panel.ExpandedState.Value != ModCustomisationPanelState.Collapsed;

            private readonly ModCustomisationPanel panel;

            public FocusGrabbingContainer(ModCustomisationPanel panel)
            {
                this.panel = panel;
            }

            private InputManager inputManager = null!;

            protected override void LoadComplete()
            {
                base.LoadComplete();
                inputManager = GetContainingInputManager()!;
            }

            private double timeUntilCollapse;

            private const double collapse_grace_time = 180;
            private const float collapse_grace_position = 40;

            protected override void Update()
            {
                base.Update();

                if (ExpandedState.Value == ModCustomisationPanelState.Expanded)
                {
                    bool canCollapse = !DrawRectangle.Inflate(new Vector2(collapse_grace_position)).Contains(ToLocalSpace(inputManager.CurrentState.Mouse.Position))
                                       && inputManager.DraggedDrawable == null;

                    if (canCollapse)
                    {
                        if (timeUntilCollapse <= 0)
                            ExpandedState.Value = ModCustomisationPanelState.Collapsed;
                        timeUntilCollapse -= Time.Elapsed;
                    }
                    else
                        timeUntilCollapse = collapse_grace_time;
                }
            }
        }

        public enum ModCustomisationPanelState
        {
            Collapsed = 0,
            Expanded = 1,
            ExpandedByMod = 2,
        }
    }
}
