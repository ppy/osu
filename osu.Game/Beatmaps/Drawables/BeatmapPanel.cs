// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Beatmaps.Drawables
{
    public class BeatmapPanel : Panel, IHasContextMenu
    {
        public BeatmapInfo Beatmap;
        private readonly Sprite background;

        public Action<BeatmapPanel> GainedSelection;
        public Action<BeatmapPanel> StartRequested;
        public Action<BeatmapPanel> EditRequested;
        public Action<BeatmapInfo> HideRequested;

        private readonly Triangles triangles;
        private readonly StarCounter starCounter;

        protected override void Selected()
        {
            base.Selected();

            GainedSelection?.Invoke(this);

            background.Colour = ColourInfo.GradientVertical(
                new Color4(20, 43, 51, 255),
                new Color4(40, 86, 102, 255));

            triangles.Colour = Color4.White;
        }

        protected override void Deselected()
        {
            base.Deselected();

            background.Colour = new Color4(20, 43, 51, 255);
            triangles.Colour = OsuColour.Gray(0.5f);
        }

        protected override bool OnClick(InputState state)
        {
            if (State == PanelSelectedState.Selected)
                StartRequested?.Invoke(this);

            return base.OnClick(state);
        }

        protected override void ApplyState(PanelSelectedState last = PanelSelectedState.Hidden)
        {
            if (!IsLoaded) return;

            base.ApplyState(last);

            if (last == PanelSelectedState.Hidden && State != last)
                starCounter.ReplayAnimation();
        }

        public BeatmapPanel(BeatmapInfo beatmap)
        {
            if (beatmap == null)
                throw new ArgumentNullException(nameof(beatmap));

            Beatmap = beatmap;
            Height *= 0.60f;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                triangles = new Triangles
                {
                    TriangleScale = 2,
                    RelativeSizeAxes = Axes.Both,
                    ColourLight = OsuColour.FromHex(@"3a7285"),
                    ColourDark = OsuColour.FromHex(@"123744")
                },
                new FillFlowContainer
                {
                    Padding = new MarginPadding(5),
                    Direction = FillDirection.Horizontal,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Children = new Drawable[]
                    {
                        new DifficultyIcon(beatmap)
                        {
                            Scale = new Vector2(1.8f),
                        },
                        new FillFlowContainer
                        {
                            Padding = new MarginPadding { Left = 5 },
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(4, 0),
                                    AutoSizeAxes = Axes.Both,
                                    Children = new[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Font = @"Exo2.0-Medium",
                                            Text = beatmap.Version,
                                            TextSize = 20,
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft
                                        },
                                        new OsuSpriteText
                                        {
                                            Font = @"Exo2.0-Medium",
                                            Text = "mapped by",
                                            TextSize = 16,
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft
                                        },
                                        new OsuSpriteText
                                        {
                                            Font = @"Exo2.0-MediumItalic",
                                            Text = $"{(beatmap.Metadata ?? beatmap.BeatmapSet.Metadata).Author.Username}",
                                            TextSize = 16,
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft
                                        },
                                    }
                                },
                                starCounter = new StarCounter
                                {
                                    CountStars = (float)beatmap.StarDifficulty,
                                    Scale = new Vector2(0.8f),
                                }
                            }
                        }
                    }
                }
            };
        }

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("Play", MenuItemType.Highlighted, () => StartRequested?.Invoke(this)),
            new OsuMenuItem("Edit", MenuItemType.Standard, () => EditRequested?.Invoke(this)),
            new OsuMenuItem("Hide", MenuItemType.Destructive, () => HideRequested?.Invoke(Beatmap)),
        };
    }
}
