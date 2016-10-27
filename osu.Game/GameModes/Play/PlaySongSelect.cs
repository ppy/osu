//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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

namespace osu.Game.GameModes.Play
{
    public class PlaySongSelect : OsuGameMode
    {
        private Bindable<PlayMode> playMode;
        private BeatmapDatabase beatmaps;
        private BeatmapGroup selectedBeatmapGroup;
        private BeatmapInfo selectedBeatmap;
        // TODO: use currently selected track as bg
        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");
        private ScrollContainer scrollContainer;
        private FlowContainer setList;

        public PlaySongSelect()
        {
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
                            Action = () => Push(new Player { Beatmap = beatmaps.GetBeatmap(selectedBeatmap) }),
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

            beatmaps = (game as OsuGameBase).Beatmaps;
            beatmaps.BeatmapSetAdded += bset => Scheduler.Add(() => addBeatmapSet(bset));
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
                selectedBeatmapGroup.State = BeatmapGroup.GroupState.Collapsed;

            selectedBeatmapGroup = group;
            selectedBeatmap = beatmap;
        }

        private void addBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            beatmapSet = beatmaps.GetWithChildren<BeatmapSetInfo>(beatmapSet.BeatmapSetID);
            beatmapSet.Beatmaps.ForEach(b => beatmaps.GetChildren(b));
            beatmapSet.Beatmaps = beatmapSet.Beatmaps.OrderBy(b => b.BaseDifficulty.OverallDifficulty).ToList();
            Schedule(() =>
            {
                var group = new BeatmapGroup(beatmapSet) { SelectionChanged = selectBeatmap };
                setList.Add(group);
                if (setList.Children.Count() == 1)
                {
                    group.State = BeatmapGroup.GroupState.Expanded;
                }
            });
        }

        private void addBeatmapSets()
        {
            foreach (var beatmapSet in beatmaps.Query<BeatmapSetInfo>())
                addBeatmapSet(beatmapSet);
        }
    }
}
