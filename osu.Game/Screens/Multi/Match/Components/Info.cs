// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multi.Components;
using osuTK;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class Info : Container
    {
        public Action OnStart;

        private readonly OsuSpriteText availabilityStatus;

        private OsuColour colours;

        private readonly RoomBindings bindings = new RoomBindings();

        public Info(Room room)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            ReadyButton readyButton;
            ViewBeatmapButton viewBeatmapButton;
            OsuSpriteText name;
            EndDateInfo endDate;
            HostInfo hostInfo;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"28242d"),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 10),
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
                                        endDate = new EndDateInfo { TextSize = 14 }
                                    }
                                },
                                hostInfo = new HostInfo(),
                            },
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            AutoSizeAxes = Axes.X,
                            Height = 70,
                            Spacing = new Vector2(10, 0),
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                viewBeatmapButton = new ViewBeatmapButton(),
                                readyButton = new ReadyButton(room)
                                {
                                    Action = () => OnStart?.Invoke()
                                }
                            }
                        }
                    },
                },
            };

            viewBeatmapButton.Beatmap.BindTo(bindings.CurrentBeatmap);
            readyButton.Beatmap.BindTo(bindings.CurrentBeatmap);
            hostInfo.Host.BindTo(bindings.Host);

            bindings.Availability.BindValueChanged(_ => updateAvailabilityStatus());
            bindings.Status.BindValueChanged(_ => updateAvailabilityStatus());
            bindings.Name.BindValueChanged(n => name.Text = n);
            bindings.EndDate.BindValueChanged(d => endDate.Date = d);

            bindings.Room = room;
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

            if (bindings.Status.Value != null)
            {
                availabilityStatus.FadeColour(bindings.Status.Value.GetAppropriateColour(colours), 100);
                availabilityStatus.Text = $"{bindings.Availability.Value.GetDescription()}, {bindings.Status.Value.Message}";
            }
        }
    }
}
