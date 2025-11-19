// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.SelectV2
{
    public partial class FooterButtonOptions
    {
        public partial class Popover : OsuPopover
        {
            private FillFlowContainer buttonFlow = null!;
            private readonly FooterButtonOptions footerButton;

            private readonly BeatmapInfo beatmap;

            // Can't use DI for these due to popover being initialised from a footer button which ends up being on the global
            // PopoverContainer.
            public ISongSelect? SongSelect { get; init; }
            public required OverlayColourProvider ColourProvider { get; init; }

            public Popover(FooterButtonOptions footerButton, BeatmapInfo beatmap)
            {
                this.footerButton = footerButton;
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Content.Padding = new MarginPadding(5);

                Child = buttonFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(3),
                };

                addHeader(CommonStrings.General);
                addButton(CollectionsStrings.ManageCollections, FontAwesome.Solid.Book, () => SongSelect?.ManageCollections());

                Debug.Assert(beatmap.BeatmapSet != null);
                addHeader(SongSelectStrings.ForAllDifficulties, beatmap.BeatmapSet.ToString());
                addButton(SongSelectStrings.DeleteBeatmap, FontAwesome.Solid.Trash, () => SongSelect?.Delete(beatmap.BeatmapSet), colours.Red1);

                addHeader(SongSelectStrings.ForSelectedDifficulty, beatmap.DifficultyName);

                if (SongSelect == null) return;

                foreach (OsuMenuItem item in SongSelect.GetForwardActions(beatmap))
                {
                    // We can't display menus with child items here, so just ignore them.
                    if (item.Items.Any())
                        continue;

                    if (item is OsuMenuItemSpacer)
                    {
                        buttonFlow.Add(new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 10,
                        });
                        continue;
                    }

                    addButton(item.Text.Value, item.Icon, item.Action.Value, item.Type == MenuItemType.Destructive ? colours.Red1 : null);
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ScheduleAfterChildren(() => GetContainingFocusManager()!.ChangeFocus(this));
            }

            protected override void UpdateState(ValueChangedEvent<Visibility> state)
            {
                base.UpdateState(state);
                footerButton.OverlayState.Value = state.NewValue;
            }

            private void addHeader(LocalisableString text, string? context = null)
            {
                var textFlow = new OsuTextFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding(10),
                };

                textFlow.AddText(text, t => t.Font = OsuFont.Default.With(weight: FontWeight.SemiBold));

                if (context != null)
                {
                    textFlow.NewLine();
                    textFlow.AddText(context, t =>
                    {
                        t.Colour = ColourProvider.Content2;
                        t.Font = t.Font.With(size: 13);
                    });
                }

                buttonFlow.Add(textFlow);
            }

            private void addButton(LocalisableString text, IconUsage? icon, Action? action, Color4? colour = null)
            {
                var button = new OptionButton
                {
                    Text = text,
                    Icon = icon ?? new IconUsage(),
                    BackgroundColour = ColourProvider.Background3,
                    TextColour = colour,
                    Action = () =>
                    {
                        Scheduler.AddDelayed(Hide, 50);
                        action?.Invoke();
                    },
                };

                buttonFlow.Add(button);
            }

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                // don't absorb control as ToolbarRulesetSelector uses control + number to navigate
                if (e.ControlPressed) return false;

                if (!e.Repeat && e.Key >= Key.Number1 && e.Key <= Key.Number9)
                {
                    int requested = e.Key - Key.Number1;

                    OptionButton? found = buttonFlow.Children.OfType<OptionButton>().ElementAtOrDefault(requested);

                    if (found != null)
                    {
                        found.TriggerClick();
                        return true;
                    }
                }

                return base.OnKeyDown(e);
            }

            private partial class OptionButton : OsuButton
            {
                public IconUsage Icon { get; init; }
                public Color4? TextColour { get; init; }

                public OptionButton()
                {
                    Size = new Vector2(265, 50);
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    SpriteText.Colour = TextColour ?? Color4.White;
                    Content.CornerRadius = 10;

                    Add(new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(17),
                        X = 15,
                        Icon = Icon,
                        Colour = TextColour ?? Color4.White,
                    });
                }

                protected override SpriteText CreateText() => new OsuSpriteText
                {
                    Depth = -1,
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    X = 40
                };
            }
        }
    }
}
