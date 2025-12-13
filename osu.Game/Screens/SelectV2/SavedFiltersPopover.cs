// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Rulesets;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class SavedFiltersPopover : OsuPopover
    {
        private readonly Action<SavedBeatmapFilter> onSelect;
        private readonly Action<string> onSave;

        [Resolved]
        private RealmAccess? realm { get; set; }

        private readonly RulesetInfo ruleset;

        public SavedFiltersPopover(Action<SavedBeatmapFilter> onSelect, Action<string> onSave, RulesetInfo ruleset)
        {
            this.onSelect = onSelect;
            this.onSave = onSave;
            this.ruleset = ruleset;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Content.Padding = new MarginPadding(5);

            var flow = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                Width = 250,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0, 5),
            };

            Child = flow;

            // Add saved filters
            var savedFilters = realm?.Realm.All<SavedBeatmapFilter>().Where(f => f.RulesetShortName == ruleset.ShortName);

            var filtersFlow = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };

            bool hasFilters = false;

            if (savedFilters != null)
            {
                foreach (var filter in savedFilters)
                {
                    hasFilters = true;

                    filtersFlow.Add(new SavedFilterItem(filter, onSelect, () =>
                    {
                        realm?.Write(r => r.Remove(filter));
                        Hide();
                    }));
                }
            }

            if (hasFilters)
            {
                flow.Add(filtersFlow);

                flow.Add(new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Colour = colours.Gray4,
                });
            }
            else
            {
                flow.Add(new OsuSpriteText
                {
                    Text = "No saved filters",
                    Colour = colours.Gray8,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                });
            }

            // Add save section
            var nameTextBox = new OsuTextBox
            {
                PlaceholderText = "Filter name",
                RelativeSizeAxes = Axes.X,
                Height = 30
            };

            flow.Add(nameTextBox);

            flow.Add(new RoundedButton
            {
                Text = "Save current filter",
                RelativeSizeAxes = Axes.X,
                Action = () =>
                {
                    string name = nameTextBox.Text.Trim();

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        nameTextBox.FlashColour(Color4.Red, 500);
                        nameTextBox.Shake();
                        return;
                    }

                    if (string.IsNullOrEmpty(ruleset.ShortName))
                    {
                        nameTextBox.FlashColour(Color4.Red, 500);
                        nameTextBox.Shake();
                        return;
                    }

                    if (name.Length > 40)
                    {
                        nameTextBox.FlashColour(Color4.Red, 500);
                        nameTextBox.Shake();
                        return;
                    }

                    if (realm?.Realm.All<SavedBeatmapFilter>().Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && f.RulesetShortName == ruleset.ShortName) == true)
                    {
                        nameTextBox.FlashColour(Color4.Red, 500);
                        nameTextBox.Shake();
                        return;
                    }

                    onSave(name);
                    Hide();
                }
            });
        }

        private partial class SavedFilterItem : ClickableContainer
        {
            private readonly Box background;
            private readonly SpriteIcon arrow;

            public SavedFilterItem(SavedBeatmapFilter filter, Action<SavedBeatmapFilter> action, Action onDelete)
            {
                RelativeSizeAxes = Axes.X;
                Height = 20;
                Action = () => action(filter);
                CornerRadius = 5;
                Masking = true;

                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                    },
                    arrow = new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.ChevronRight,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(8),
                        X = 0,
                        Alpha = 0,
                        Margin = new MarginPadding { Left = 3, Right = 3 },
                    },
                    new OsuSpriteText
                    {
                        Text = filter.Name,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        X = 15,
                    },
                    new IconButton
                    {
                        Icon = FontAwesome.Solid.Trash,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Action = onDelete,
                        Scale = new Vector2(0.55f),
                        X = -5,
                    }
                };
            }

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            [Resolved(CanBeNull = true)]
            private OverlayColourProvider? colourProvider { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                background.Colour = colourProvider?.Light4 ?? colours.BlueDark;
                arrow.Colour = colourProvider?.Background5 ?? Color4.Black;

                AddInternal(new HoverSounds());
            }

            protected override bool OnHover(HoverEvent e)
            {
                background.FadeIn(100, Easing.OutQuint);
                arrow.FadeIn(400, Easing.OutQuint);
                arrow.MoveToX(3, 400, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                background.FadeOut(600, Easing.OutQuint);
                arrow.FadeOut(200);
                arrow.MoveToX(0, 200, Easing.In);
                base.OnHoverLost(e);
            }
        }
    }
}
