// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class Info : Container
    {
        public const float HEIGHT = 156;

        public Action OnStart;

        private readonly OsuSpriteText availabilityStatus;

        private OsuColour colours;

        public readonly Bindable<string> Name = new Bindable<string>();
        public readonly Bindable<RoomAvailability> Availability = new Bindable<RoomAvailability>();
        public readonly Bindable<RoomStatus> Status = new Bindable<RoomStatus>();
        public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();
        public readonly Bindable<GameType> Type = new Bindable<GameType>();
        public readonly Bindable<IEnumerable<Mod>> Mods = new Bindable<IEnumerable<Mod>>();

        public Info()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            BeatmapTypeInfo beatmapTypeInfo;
            OsuSpriteText name;
            ModDisplay modDisplay;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"28242d"),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Padding = new MarginPadding { Vertical = 20 },
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        name = new OsuSpriteText { TextSize = 30 },
                                        availabilityStatus = new OsuSpriteText { TextSize = 14 },
                                    },
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        beatmapTypeInfo = new BeatmapTypeInfo(),
                                        modDisplay = new ModDisplay
                                        {
                                            Scale = new Vector2(0.75f),
                                            DisplayUnrankedText = false
                                        },
                                    }
                                }

                            },
                        },
                        new ReadyButton
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Y,
                            Size = new Vector2(200, 1),
                            Padding = new MarginPadding { Vertical = 10 },
                            Action = () => OnStart?.Invoke()
                        },
                    },
                },
            };

            beatmapTypeInfo.Beatmap.BindTo(Beatmap);
            beatmapTypeInfo.Type.BindTo(Type);
            modDisplay.Current.BindTo(Mods);

            Availability.BindValueChanged(_ => updateAvailabilityStatus());
            Status.BindValueChanged(_ => updateAvailabilityStatus());
            Name.BindValueChanged(n => name.Text = n);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            this.colours = colours;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateAvailabilityStatus();
        }

        private void updateAvailabilityStatus()
        {
            if (!IsLoaded)
                return;

            if (Status.Value != null)
            {
                availabilityStatus.FadeColour(Status.Value.GetAppropriateColour(colours), 100);
                availabilityStatus.Text = $"{Availability.Value.GetDescription()}, {Status.Value.Message}";
            }
        }
    }
}
