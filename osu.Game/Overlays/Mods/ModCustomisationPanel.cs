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
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class ModCustomisationPanel : VisibilityContainer
    {
        private const float header_height = 42f;
        private const float content_vertical_padding = 20f;

        private Container content = null!;
        private OsuScrollContainer scrollContainer = null!;
        private FillFlowContainer sectionsFlow = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public readonly BindableBool Expanded = new BindableBool();

        public Bindable<IReadOnlyList<Mod>> SelectedMods { get; } = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

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
                    Expanded = { BindTarget = Expanded },
                },
                content = new InputBlockingContainer
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
                            ScrollbarOverlapsContent = false,
                            Margin = new MarginPadding { Top = header_height },
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

            Expanded.BindValueChanged(_ => updateDisplay(), true);
            SelectedMods.BindValueChanged(_ => updateMods(), true);

            FinishTransforms(true);
        }

        protected override void PopIn() => this.FadeIn(300, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(300, Easing.OutQuint);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (Expanded.Value && !content.ReceivePositionalInputAt(e.ScreenSpaceMousePosition))
                Expanded.Value = false;

            return base.OnMouseDown(e);
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
    }
}
