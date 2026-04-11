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
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    /// <summary>
    /// An overlay allowing the room host to override score multipliers for individual mods.
    /// Changes are purely cosmetic within the lobby and do not affect ranked score submission.
    /// </summary>
    public partial class MultiplayerModMultiplierOverlay : OsuFocusedOverlayContainer
    {
        protected override bool BlockNonPositionalInput => true;

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.InQuint);

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private IRulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private readonly Room room;
        private FillFlowContainer rowsContainer = null!;
        private OsuSpriteText noModsText = null!;

        public MultiplayerModMultiplierOverlay(Room room)
        {
            this.room = room;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 520,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 10,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background3,
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(20),
                        Spacing = new Vector2(0, 12),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = ModMultiplierStrings.ModMultipliersTitle,
                                Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                            },
                            new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(size: 13))
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Text = ModMultiplierStrings.ModMultipliersDescription,
                                Colour = colourProvider.Content2,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 1,
                                Colour = colourProvider.Background5,
                            },
                            rowsContainer = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 8),
                            },
                            noModsText = new OsuSpriteText
                            {
                                Text = ModMultiplierStrings.NoModsSelected,
                                Colour = colourProvider.Content2,
                                Font = OsuFont.GetFont(size: 14),
                                Alpha = 0,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 1,
                                Colour = colourProvider.Background5,
                            },
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(8, 0),
                                Children = new Drawable[]
                                {
                                    new RoundedButton
                                    {
                                        Text = ModMultiplierStrings.ResetAll,
                                        Width = 120,
                                        Height = 36,
                                        Action = resetAll,
                                    },
                                    new RoundedButton
                                    {
                                        Text = @"Close",
                                        Width = 120,
                                        Height = 36,
                                        Action = Hide,
                                    },
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            client.RoomUpdated += onRoomUpdated;
            refreshRows();
        }

        private void onRoomUpdated() => Schedule(refreshRows);

        private void refreshRows()
        {
            rowsContainer.Clear();

            var currentItem = client.Room?.CurrentPlaylistItem;
            if (currentItem == null)
            {
                noModsText.Show();
                return;
            }

            var ruleset = rulesets.GetRuleset(currentItem.RulesetID)?.CreateInstance();
            if (ruleset == null)
            {
                noModsText.Show();
                return;
            }

            // Include both required mods and allowed mods (free mods)
            var allMods = currentItem.RequiredMods
                                     .Concat(currentItem.AllowedMods)
                                     .Select(m => m.ToMod(ruleset))
                                     .Where(m => m is not UnknownMod)
                                     .DistinctBy(m => m.Acronym)
                                     .ToList();

            if (allMods.Count == 0)
            {
                noModsText.Show();
                return;
            }

            noModsText.Hide();

            var currentMultipliers = client.Room?.Settings.ModMultipliers ?? new Dictionary<string, double>();

            foreach (var mod in allMods)
            {
                double current = currentMultipliers.TryGetValue(mod.Acronym, out double v) ? v : mod.ScoreMultiplier;
                rowsContainer.Add(new ModMultiplierRow(mod, current)
                {
                    RelativeSizeAxes = Axes.X,
                    OnMultiplierChanged = (acronym, value) => applyMultiplier(acronym, value),
                    OnReset = acronym => resetMultiplier(acronym),
                });
            }
        }

        private void applyMultiplier(string acronym, double value)
        {
            var room = client.Room;
            if (room == null) return;

            value = Math.Clamp(value, MultiplayerModMultiplierApplicator.MIN_MULTIPLIER, MultiplayerModMultiplierApplicator.MAX_MULTIPLIER);

            var updated = new Dictionary<string, double>(room.Settings.ModMultipliers)
            {
                [acronym] = value
            };

            client.ChangeSettings(modMultipliers: updated).ContinueWith(t =>
            {
                if (!t.IsCompletedSuccessfully)
                {
                    Schedule(() =>
                    {
                        // Revert slider on error
                        refreshRows();
                    });
                }
            });
        }

        private void resetMultiplier(string acronym)
        {
            var room = client.Room;
            if (room == null) return;

            var updated = new Dictionary<string, double>(room.Settings.ModMultipliers);
            updated.Remove(acronym);

            client.ChangeSettings(modMultipliers: updated).ContinueWith(t =>
            {
                if (!t.IsCompletedSuccessfully)
                {
                    Schedule(refreshRows);
                }
            });
        }

        private void resetAll()
        {
            var room = client.Room;
            if (room == null) return;

            client.ChangeSettings(modMultipliers: new Dictionary<string, double>()).ContinueWith(t =>
            {
                if (!t.IsCompletedSuccessfully)
                {
                    Schedule(refreshRows);
                }
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            client.RoomUpdated -= onRoomUpdated;
        }

        /// <summary>
        /// A single row showing a mod's name, its original multiplier, a slider for the custom value, and a reset button.
        /// </summary>
        private partial class ModMultiplierRow : CompositeDrawable
        {
            public Action<string, double>? OnMultiplierChanged;
            public Action<string>? OnReset;

            private readonly Mod mod;
            private readonly double initialValue;

            private readonly BindableDouble sliderValue = new BindableDouble
            {
                MinValue = MultiplayerModMultiplierApplicator.MIN_MULTIPLIER,
                MaxValue = MultiplayerModMultiplierApplicator.MAX_MULTIPLIER,
                Precision = 0.01,
            };

            private OsuSpriteText valueText = null!;
            private ScheduledDelegate? pendingUpdate;

            public ModMultiplierRow(Mod mod, double initialValue)
            {
                this.mod = mod;
                this.initialValue = initialValue;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding { Vertical = 4 };

                sliderValue.Value = initialValue;

                InternalChild = new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 120),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 70),
                        new Dimension(GridSizeMode.Absolute, 60),
                    },
                    RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Text = mod.Name,
                                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                                    },
                                    new OsuSpriteText
                                    {
                                        Text = $"({mod.Acronym}) base: {mod.ScoreMultiplier:0.00}x",
                                        Font = OsuFont.GetFont(size: 11),
                                        Colour = colourProvider.Content2,
                                    },
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Padding = new MarginPadding { Horizontal = 8 },
                                Child = new RoundedSliderBar<double>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Current = sliderValue,
                                }
                            },
                            valueText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.GetFont(size: 13, weight: FontWeight.SemiBold),
                            },
                            new RoundedButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = ModMultiplierStrings.Reset,
                                Height = 28,
                                Width = 52,
                                Action = () => OnReset?.Invoke(mod.Acronym),
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                sliderValue.BindValueChanged(v =>
                {
                    valueText.Text = $"{v.NewValue:0.00}x";

                    // Debounce to avoid spamming RPC calls while dragging
                    pendingUpdate?.Cancel();
                    pendingUpdate = Scheduler.AddDelayed(() =>
                    {
                        OnMultiplierChanged?.Invoke(mod.Acronym, v.NewValue);
                    }, 300);
                }, true);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                pendingUpdate?.Cancel();
            }
        }
    }
}
