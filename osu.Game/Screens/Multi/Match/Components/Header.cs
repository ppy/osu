// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class Header : Container
    {
        public const float HEIGHT = 200;

        private readonly RoomBindings bindings = new RoomBindings();

        private readonly Box tabStrip;

        public readonly MatchTabControl Tabs;

        public Action OnRequestSelectBeatmap;

        public Header(Room room)
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            bindings.Room = room;

            BeatmapTypeInfo beatmapTypeInfo;
            BeatmapSelectButton beatmapButton;
            UpdateableBeatmapBackgroundSprite background;
            ModDisplay modDisplay;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        background = new HeaderBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.4f), Color4.Black.Opacity(0.6f)),
                        },
                    }
                },
                tabStrip = new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING + OsuScreen.HORIZONTAL_OVERFLOW_PADDING },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 20 },
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
                        },
                        new Container
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = 200,
                            Padding = new MarginPadding { Vertical = 10 },
                            Child = beatmapButton = new BeatmapSelectButton(room)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Height = 1,
                            },
                        },
                        Tabs = new MatchTabControl(room)
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X
                        },
                    },
                },
            };

            beatmapTypeInfo.Beatmap.BindTo(bindings.CurrentBeatmap);
            beatmapTypeInfo.Ruleset.BindTo(bindings.CurrentRuleset);
            beatmapTypeInfo.Type.BindTo(bindings.Type);
            background.Beatmap.BindTo(bindings.CurrentBeatmap);
            bindings.CurrentMods.BindValueChanged(m => modDisplay.Current.Value = m, true);

            beatmapButton.Action = () => OnRequestSelectBeatmap?.Invoke();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            tabStrip.Colour = colours.Yellow;
        }

        private class BeatmapSelectButton : HeaderButton
        {
            private readonly IBindable<int?> roomIDBind = new Bindable<int?>();

            public BeatmapSelectButton(Room room)
            {
                Text = "Select beatmap";

                roomIDBind.BindTo(room.RoomID);
                roomIDBind.BindValueChanged(v => this.FadeTo(v.HasValue ? 0 : 1), true);
            }
        }

        private class HeaderBeatmapBackgroundSprite : UpdateableBeatmapBackgroundSprite
        {
            protected override double FadeDuration => 200;
        }
    }
}
