// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public partial class SearchPopover : OsuPopover
    {
        private FillFlowContainer buttonFlow = null!;
        private readonly FooterButtonSearch footerButton;

        private readonly BeatmapInfo beatmap;
        private bool was1Pressed;
        private bool was2Pressed;
        private bool was3Pressed;
        private Visibility currentVisibility;
        private OsuSpriteText? searchModeText;

        public OverlayColourProvider ColourProvider { get; init; }
        public BeatmapListingOverlay BeatmapListing { get; init; }
        public ISongSelect? SongSelect { get; init; }

        public SearchPopover(FooterButtonSearch footerButton, BeatmapInfo beatmap, OverlayColourProvider colourProvider, BeatmapListingOverlay beatmapListing, ISongSelect? songSelect)
        {
            this.footerButton = footerButton;
            this.beatmap = beatmap;
            ColourProvider = colourProvider;
            BeatmapListing = beatmapListing;
            SongSelect = songSelect;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Content.Padding = new MarginPadding(5);

            searchModeText = new OsuSpriteText
            {
                Text = "Search online for beatmaps",
                Font = OsuFont.Default.With(size: 12, weight: FontWeight.Bold),
                Colour = Color4.White,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Margin = new MarginPadding { Bottom = 5 }
            };

            var hintText = new OsuSpriteText
            {
                Text = "Hold Shift for local search",
                Font = OsuFont.Default.With(size: 9),
                Colour = Color4.Gray,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Margin = new MarginPadding { Bottom = 10 }
            };

            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    searchModeText,
                    hintText,
                    buttonFlow = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10),
                    }
                }
            };

            addButton("By title", FontAwesome.Solid.Search, "title");
            addButton("By artist", FontAwesome.Solid.User, "artist");
            addButton("By source", FontAwesome.Solid.Tag, "source");
        }


        private void searchBy(string field)
        {
            if (beatmap?.Metadata == null) return;
            var meta = beatmap.Metadata;

            string query = field switch
            {
                "title" => !string.IsNullOrEmpty(meta.Title) ? $"title=={meta.Title}" : string.Empty,
                "artist" => !string.IsNullOrEmpty(meta.Artist) ? $"artist=={meta.Artist}" : string.Empty,
                "source" => !string.IsNullOrEmpty(meta.Source) ? $"source=={meta.Source}" : string.Empty,
                _ => string.Empty
            };


            if (!string.IsNullOrEmpty(query))
                BeatmapListing.ShowWithSearch(query);

            Hide();
        }

        private void performLocalSearch(string field)
        {
            if (beatmap?.Metadata == null) return;
            var meta = beatmap.Metadata;

            string query = field switch
            {
                "title" => !string.IsNullOrEmpty(meta.Title) ? $"title={meta.Title}" : string.Empty,
                "artist" => !string.IsNullOrEmpty(meta.Artist) ? $"artist={meta.Artist}" : string.Empty,
                "source" => !string.IsNullOrEmpty(meta.Source) ? $"source={meta.Source}" : string.Empty,
                _ => string.Empty
            };

            if (!string.IsNullOrEmpty(query))
                SongSelect?.Search(query);

            Hide();
        }

        private void addButton(LocalisableString text, IconUsage icon, string field)
        {
            var button = new OptionButton
            {
                Text = text,
                Icon = icon,
                BackgroundColour = ColourProvider.Background3,
                Action = () =>
                {
                    Scheduler.AddDelayed(Hide, 50);

                    var inputManager = GetContainingInputManager();
                    bool shiftPressed = inputManager?.CurrentState.Keyboard.ShiftPressed == true;

                    if (shiftPressed)
                        performLocalSearch(field);
                    else
                        searchBy(field);
                },
            };

            buttonFlow.Add(button);
        }

        private partial class OptionButton : OsuButton
        {
            public IconUsage Icon { get; init; }

            public OptionButton()
            {
                Size = new Vector2(100, 35);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Add(new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(14),
                    X = 10,
                    Icon = Icon,
                    Colour = Color4.White,
                });
            }

            protected override SpriteText CreateText() => new OsuSpriteText
            {
                Origin = Anchor.CentreLeft,
                Anchor = Anchor.CentreLeft,
                X = 30,
                Font = OsuFont.Default.With(size: 12)
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ScheduleAfterChildren(() => GetContainingFocusManager()?.ChangeFocus(this));
        }

        protected override void UpdateState(ValueChangedEvent<Visibility> state)
        {
            base.UpdateState(state);
            footerButton.OverlayState.Value = state.NewValue;
            currentVisibility = state.NewValue;
        }

        protected override void Update()
        {
            base.Update();

            if (currentVisibility != Visibility.Visible) return;

            var inputManager = GetContainingInputManager();
            if (inputManager == null) return;

            var keyboard = inputManager.CurrentState.Keyboard;

            if (searchModeText != null)
                searchModeText.Text = keyboard.ShiftPressed ? "Search in song select" : "Search online for beatmaps";

            bool pressed1 = keyboard.Keys.IsPressed(Key.Number1);
            if (pressed1 && !was1Pressed)
            {
                if (keyboard.ShiftPressed)
                    performLocalSearch("title");
                else
                    searchBy("title");
            }
            was1Pressed = pressed1;

            bool pressed2 = keyboard.Keys.IsPressed(Key.Number2);
            if (pressed2 && !was2Pressed)
            {
                if (keyboard.ShiftPressed)
                    performLocalSearch("artist");
                else
                    searchBy("artist");
            }
            was2Pressed = pressed2;

            bool pressed3 = keyboard.Keys.IsPressed(Key.Number3);
            if (pressed3 && !was3Pressed)
            {
                if (keyboard.ShiftPressed)
                    performLocalSearch("source");
                else
                    searchBy("source");
            }
            was3Pressed = pressed3;
        }
    }
}
