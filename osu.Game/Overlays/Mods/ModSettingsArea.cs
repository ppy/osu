// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class ModSettingsArea : CompositeDrawable
    {
        public Bindable<IReadOnlyList<Mod>> SelectedMods { get; } = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        public const float HEIGHT = 250;

        private readonly Box background;
        private readonly FillFlowContainer modSettingsFlow;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public override bool AcceptsFocus => true;

        public ModSettingsArea()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new OsuScrollContainer(Direction.Horizontal)
                    {
                        RelativeSizeAxes = Axes.Both,
                        ScrollbarOverlapsContent = false,
                        ClampExtension = 100,
                        Child = modSettingsFlow = new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Vertical = 7, Horizontal = 70 },
                            Spacing = new Vector2(7),
                            Direction = FillDirection.Horizontal
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colourProvider.Dark3;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            SelectedMods.BindValueChanged(_ => updateMods(), true);
        }

        private void updateMods()
        {
            modSettingsFlow.Clear();

            foreach (var mod in SelectedMods.Value.AsOrdered())
            {
                var settings = mod.CreateSettingsControls().ToList();

                if (settings.Count > 0)
                {
                    if (modSettingsFlow.Any())
                    {
                        modSettingsFlow.Add(new Box
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = 2,
                            Colour = colourProvider.Dark4,
                        });
                    }

                    modSettingsFlow.Add(new ModSettingsColumn(mod, settings));
                }
            }
        }

        protected override bool OnMouseDown(MouseDownEvent e) => true;
        protected override bool OnHover(HoverEvent e) => true;

        private partial class ModSettingsColumn : CompositeDrawable
        {
            public ModSettingsColumn(Mod mod, IEnumerable<Drawable> settingsControls)
            {
                Width = 250;
                RelativeSizeAxes = Axes.Y;
                Padding = new MarginPadding { Bottom = 7 };

                InternalChild = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Absolute, 10),
                        new Dimension()
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(7),
                                Children = new Drawable[]
                                {
                                    new ModSwitchTiny(mod)
                                    {
                                        Active = { Value = true },
                                        Scale = new Vector2(0.6f),
                                        Origin = Anchor.CentreLeft,
                                        Anchor = Anchor.CentreLeft
                                    },
                                    new OsuSpriteText
                                    {
                                        Text = mod.Name,
                                        Font = OsuFont.Default.With(size: 16, weight: FontWeight.SemiBold),
                                        Origin = Anchor.CentreLeft,
                                        Anchor = Anchor.CentreLeft,
                                        Margin = new MarginPadding { Bottom = 2 }
                                    }
                                }
                            }
                        },
                        new[] { Empty() },
                        new Drawable[]
                        {
                            new OsuScrollContainer(Direction.Vertical)
                            {
                                RelativeSizeAxes = Axes.Both,
                                ClampExtension = 100,
                                Child = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Right = 7 },
                                    ChildrenEnumerable = settingsControls,
                                    Spacing = new Vector2(0, 7)
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}
