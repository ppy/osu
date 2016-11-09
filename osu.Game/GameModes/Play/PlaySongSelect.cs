//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Framework.Audio.Track;
using osu.Game.Beatmaps.Drawable;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Framework.GameModes;
using osu.Framework.Allocation;
using osu.Framework.Audio;

namespace osu.Game.GameModes.Play
{
    public class PlaySongSelect : OsuGameMode
    {
        private Bindable<PlayMode> playMode;
        private BeatmapDatabase database;
        private BeatmapGroup selectedBeatmapGroup;
        private BeatmapInfo selectedBeatmapInfo;
        // TODO: use currently selected track as bg
        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");
        private ScrollContainer scrollContainer;
        private FlowContainer beatmapSetFlow;
        private TrackManager trackManager;
        private Container wedgeContainer;

        /// <param name="database">Optionally provide a database to use instead of the OsuGame one.</param>
        public PlaySongSelect(BeatmapDatabase database = null)
        {
            this.database = database;

            const float scrollWidth = 640;
            const float bottomToolHeight = 50;
            Children = new Drawable[]
            {
                wedgeContainer = new Container
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
                            EdgeSmoothness = new Vector2(2, 0),
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            RelativePositionAxes = Axes.Y,
                            Size = new Vector2(1, -0.5f),
                            Position = new Vector2(0, 1),
                            Colour = new Color4(0, 0, 0, 0.5f),
                            Shear = new Vector2(-0.15f, 0),
                            EdgeSmoothness = new Vector2(2, 0),
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
                        beatmapSetFlow = new FlowContainer
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
                            Action = () => Push(new Player
                            {
                                BeatmapInfo = selectedBeatmapGroup.SelectedPanel.Beatmap,
                                PreferredPlayMode = playMode.Value
                            })
                        },
                    }
                }
            };
        }

        [Initializer]
        private void Load(BeatmapDatabase beatmaps, AudioManager audio, BaseGame game)
        {
            // TODO: Load(..., [PermitNull] OsuGame osuGame) or some such
            var osuGame = game as OsuGame;
            if (osuGame != null)
            {
                playMode = osuGame.PlayMode;
                playMode.ValueChanged += playMode_ValueChanged;
                // Temporary:
                scrollContainer.Padding = new MarginPadding { Top = osuGame.Toolbar.Height };
            }

            if (database == null)
                database = beatmaps;

            database.BeatmapSetAdded += s => Schedule(() => addBeatmapSet(s));

            trackManager = audio.Track;

            Task.Factory.StartNew(addBeatmapSets);
        }

        protected override void OnEntering(GameMode last)
        {
            base.OnEntering(last);
            ensurePlayingSelected();
            wedgeContainer.FadeInFromZero(250);
        }

        protected override void OnResuming(GameMode last)
        {
            ensurePlayingSelected();
            base.OnResuming(last);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (playMode != null)
                playMode.ValueChanged -= playMode_ValueChanged;
        }

        private void playMode_ValueChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// The global Beatmap was changed.
        /// </summary>
        protected override void OnBeatmapChanged(WorkingBeatmap beatmap)
        {
            base.OnBeatmapChanged(beatmap);
            selectBeatmap(beatmap.BeatmapInfo);
        }

        private void selectBeatmap(BeatmapInfo beatmap)
        {
            if (beatmap.Equals(selectedBeatmapInfo))
                return;

            //this is VERY temporary logic.
            beatmapSetFlow.Children.Cast<BeatmapGroup>().Any(b =>
            {
                var panel = b.BeatmapPanels.FirstOrDefault(p => p.Beatmap.Equals(beatmap));
                if (panel != null)
                {
                    panel.State = PanelSelectedState.Selected;
                    return true;
                }

                return false;
            });
        }

        /// <summary>
        /// selection has been changed as the result of interaction with the carousel.
        /// </summary>
        private void selectionChanged(BeatmapGroup group, BeatmapInfo beatmap)
        {
            selectedBeatmapInfo = beatmap;

            if (!beatmap.Equals(Beatmap?.BeatmapInfo))
            {
                Beatmap = database.GetWorkingBeatmap(beatmap, Beatmap);
            }

            ensurePlayingSelected();

            if (selectedBeatmapGroup == group)
                return;

            if (selectedBeatmapGroup != null)
                selectedBeatmapGroup.State = BeatmapGroupState.Collapsed;

            selectedBeatmapGroup = group;
        }

        private async Task ensurePlayingSelected()
        {
            AudioTrack track = null;

            await Task.Run(() => track = Beatmap?.Track);

            Schedule(delegate
            {
                if (track != null)
                {
                    trackManager.SetExclusive(track);
                    track.Start();
                }
            });
        }

        private void addBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            beatmapSet = database.GetWithChildren<BeatmapSetInfo>(beatmapSet.BeatmapSetID);
            beatmapSet.Beatmaps.ForEach(b => database.GetChildren(b));
            beatmapSet.Beatmaps = beatmapSet.Beatmaps.OrderBy(b => b.BaseDifficulty.OverallDifficulty).ToList();

            var working = database.GetWorkingBeatmap(beatmapSet.Beatmaps.FirstOrDefault());

            var group = new BeatmapGroup(beatmapSet, working) { SelectionChanged = selectionChanged };

            group.Preload(Game, g =>
            {
                beatmapSetFlow.Add(group);

                if (Beatmap == null)
                {
                    if (beatmapSetFlow.Children.Count() == 1)
                    {
                        group.State = BeatmapGroupState.Expanded;
                        return;
                    }
                }
                else
                {
                    if (selectedBeatmapInfo?.Equals(Beatmap.BeatmapInfo) != true)
                    {
                        var panel = group.BeatmapPanels.FirstOrDefault(p => p.Beatmap.Equals(Beatmap.BeatmapInfo));
                        if (panel != null)
                        {
                            panel.State = PanelSelectedState.Selected;
                            return;
                        }
                    }
                }

                group.State = BeatmapGroupState.Collapsed;
            });
        }

        private void addBeatmapSets()
        {
            foreach (var beatmapSet in database.Query<BeatmapSetInfo>())
                addBeatmapSet(beatmapSet);
        }
    }
}
