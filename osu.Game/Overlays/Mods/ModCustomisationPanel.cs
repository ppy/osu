// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class ModCustomisationPanel : OverlayContainer, IKeyBindingHandler<GlobalAction>
    {
        private const float header_height = 42f;
        private const float content_vertical_padding = 20f;

        private Container content = null!;
        private OsuScrollContainer scrollContainer = null!;
        private FillFlowContainer sectionsFlow = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public readonly BindableBool Enabled = new BindableBool();

        public readonly BindableBool Expanded = new BindableBool();

        public Bindable<IReadOnlyList<Mod>> SelectedMods { get; } = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        // Handle{Non}PositionalInput controls whether the panel should act as a blocking layer on the screen. only block when the panel is expanded.
        // These properties are used because they correctly handle blocking/unblocking hover when mouse is pointing at a drawable outside
        // (returning Expanded.Value to OnHover or overriding Block{Non}PositionalInput doesn't work).
        public override bool HandlePositionalInput => Expanded.Value;
        public override bool HandleNonPositionalInput => Expanded.Value;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new ModCustomisationHeader
                {
                    Depth = float.MinValue,
                    RelativeSizeAxes = Axes.X,
                    Height = header_height,
                    Enabled = { BindTarget = Enabled },
                    Expanded = { BindTarget = Expanded },
                },
                content = new FocusGrabbingContainer
                {
                    RelativeSizeAxes = Axes.X,
                    BorderColour = colourProvider.Dark3,
                    BorderThickness = 2f,
                    CornerRadius = 10f,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Offset = new Vector2(0f, 5f),
                        Radius = 20f,
                        Roundness = 5f,
                    },
                    Expanded = { BindTarget = Expanded },
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
                            // The +2f is a workaround for masking issues (see https://github.com/ppy/osu-framework/issues/1675#issuecomment-910023157)
                            // Note that this actually causes the full scroll range to be reduced by 2px at the bottom, but it's not really noticeable.
                            Margin = new MarginPadding { Top = header_height + 2f },
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

            Expanded.BindValueChanged(_ => updateDisplay(), true);
            SelectedMods.BindValueChanged(_ => updateMods(), true);

            FinishTransforms(true);
        }

        protected override void PopIn() => this.FadeIn(300, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(300, Easing.OutQuint);

        protected override bool OnClick(ClickEvent e)
        {
            Expanded.Value = false;
            return base.OnClick(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e) => true;

        protected override bool OnScroll(ScrollEvent e) => true;

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Back:
                    Expanded.Value = false;
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

            if (Expanded.Value)
            {
                content.AutoSizeDuration = 400;
                content.AutoSizeEasing = Easing.OutQuint;
                content.AutoSizeAxes = Axes.Y;
                content.FadeEdgeEffectTo(0.25f, 120, Easing.OutQuint);
            }
            else
            {
                content.AutoSizeAxes = Axes.None;
                content.ResizeHeightTo(header_height, 400, Easing.OutQuint);
                content.FadeEdgeEffectTo(0f, 400, Easing.OutQuint);
            }
        }

        private void updateMods()
        {
            Expanded.Value = false;
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
            public IBindable<bool> Expanded { get; } = new BindableBool();

            public override bool RequestsFocus => Expanded.Value;
            public override bool AcceptsFocus => Expanded.Value;
        }
    }
}
