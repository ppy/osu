﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Diagnostics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.GameModes.Backgrounds;
using osu.Framework;
using osu.Game.Database;
using osu.Framework.Graphics.Primitives;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.UserInterface;
using System.Threading.Tasks;
using osu.Game.Beatmaps.Drawable;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.GameModes.Play
{
    public class PlaySongSelect : OsuGameMode
    {
        private Bindable<PlayMode> playMode;
        private BeatmapDatabase database;
        private BeatmapGroup selectedBeatmapGroup;
        // TODO: use currently selected track as bg
        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");
        private ScrollContainer scrollContainer;
        private FlowContainer setList;

        /// <param name="database">Optionally provide a database to use instead of the OsuGame one.</param>
        public PlaySongSelect(BeatmapDatabase database = null)
        {
            this.database = database;

            const float scrollWidth = 640;
            const float bottomToolHeight = 50;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One,
                    Padding = new MarginPadding { Right = scrollWidth - 200 },
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(1, 0.5f),
                            Colour = new Color4(0, 0, 0, 0.5f),
                            Shear = new Vector2(0.15f, 0),
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            RelativePositionAxes = Axes.Y,
                            Size = new Vector2(1, -0.5f),
                            Position = new Vector2(0, 1),
                            Colour = new Color4(0, 0, 0, 0.5f),
                            Shear = new Vector2(-0.15f, 0),
                        },
                    }
                },
                scrollContainer = new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(scrollWidth, 1),
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Children = new Drawable[]
                    {
                        setList = new FlowContainer
                        {
                            Padding = new MarginPadding { Left = 25, Top = 25, Bottom = 25 + bottomToolHeight },
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FlowDirection.VerticalOnly,
                            Spacing = new Vector2(0, 5),
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = bottomToolHeight,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,
                            Colour = new Color4(0, 0, 0, 0.5f),
                        },
                        new Button
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = 100,
                            Text = "Play",
                            Colour = new Color4(238, 51, 153, 255),
                            Action = () => Push(new Player {
                                BeatmapInfo = selectedBeatmapGroup.SelectedPanel.Beatmap,
                                PlayMode = playMode.Value
                            }),
                        },
                    }
                }
            };
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            OsuGame osuGame = game as OsuGame;
            if (osuGame != null)
            {
                playMode = osuGame.PlayMode;
                playMode.ValueChanged += PlayMode_ValueChanged;
                // Temporary:
                scrollContainer.Padding = new MarginPadding { Top = osuGame.Toolbar.Height };
            }

            if (database == null)
                database = (game as OsuGameBase).Beatmaps;

            database.BeatmapSetAdded += s => Schedule(() => addBeatmapSet(s));

            Task.Factory.StartNew(addBeatmapSets);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (playMode != null)
                playMode.ValueChanged -= PlayMode_ValueChanged;
        }

        private void PlayMode_ValueChanged(object sender, EventArgs e)
        {
        }

        private void selectBeatmap(BeatmapGroup group, BeatmapInfo beatmap)
        {
            if (selectedBeatmapGroup == group)
                return;

            if (selectedBeatmapGroup != null)
                selectedBeatmapGroup.State = BeatmapGroupState.Collapsed;

            selectedBeatmapGroup = group;
        }

        private void addBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            beatmapSet = database.GetWithChildren<BeatmapSetInfo>(beatmapSet.BeatmapSetID);
            beatmapSet.Beatmaps.ForEach(b => database.GetChildren(b));
            beatmapSet.Beatmaps = beatmapSet.Beatmaps.OrderBy(b => b.BaseDifficulty.OverallDifficulty).ToList();
            Schedule(() =>
            {
                var group = new BeatmapGroup(beatmapSet) { SelectionChanged = selectBeatmap };
                setList.Add(group);
                if (setList.Children.Count() == 1)
                    group.State = BeatmapGroupState.Expanded;
            });
        }

        private void addBeatmapSets()
        {
            foreach (var beatmapSet in database.Query<BeatmapSetInfo>())
                addBeatmapSet(beatmapSet);
        }
    }
}
