// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class Header : MultiplayerComposite
    {
        public const float HEIGHT = 200;

        public readonly BindableBool ShowBeatmapPanel = new BindableBool();

        public MatchTabControl Tabs { get; private set; }

        public Action RequestBeatmapSelection;

        private MatchBeatmapPanel beatmapPanel;

        public Header()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BeatmapSelectButton beatmapButton;
            ModDisplay modDisplay;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new HeaderBackgroundSprite { RelativeSizeAxes = Axes.Both },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.7f), Color4.Black.Opacity(0.8f)),
                        },
                        beatmapPanel = new MatchBeatmapPanel
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Margin = new MarginPadding { Right = 100 },
                        }
                    }
                },
                new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Colour = colours.Yellow
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
                                new BeatmapTypeInfo(),
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
                            Child = beatmapButton = new BeatmapSelectButton
                            {
                                RelativeSizeAxes = Axes.Both,
                                Height = 1,
                            },
                        },
                        Tabs = new MatchTabControl
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X
                        },
                    },
                },
            };

            CurrentItem.BindValueChanged(item => modDisplay.Current.Value = item.NewValue?.RequiredMods?.ToArray() ?? Array.Empty<Mod>(), true);

            beatmapButton.Action = () => RequestBeatmapSelection?.Invoke();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ShowBeatmapPanel.BindValueChanged(value => beatmapPanel.FadeTo(value.NewValue ? 1 : 0, 200, Easing.OutQuint), true);
        }

        private class BeatmapSelectButton : HeaderButton
        {
            [Resolved(typeof(Room), nameof(Room.RoomID))]
            private Bindable<int?> roomId { get; set; }

            public BeatmapSelectButton()
            {
                Text = "Select beatmap";
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                roomId.BindValueChanged(id => this.FadeTo(id.NewValue.HasValue ? 0 : 1), true);
            }
        }

        private class HeaderBackgroundSprite : MultiplayerBackgroundSprite
        {
            protected override UpdateableBeatmapBackgroundSprite CreateBackgroundSprite() => new BackgroundSprite { RelativeSizeAxes = Axes.Both };

            private class BackgroundSprite : UpdateableBeatmapBackgroundSprite
            {
                protected override double TransformDuration => 200;
            }
        }
    }
}
