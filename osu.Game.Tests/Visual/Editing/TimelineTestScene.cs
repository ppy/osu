// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Storyboards;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editing
{
    public abstract partial class TimelineTestScene : EditorClockTestScene
    {
        protected TimelineArea TimelineArea { get; private set; }

        protected HitObjectComposer Composer { get; private set; }

        protected EditorBeatmap EditorBeatmap { get; private set; }

        [Resolved]
        private AudioManager audio { get; set; }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null) => new WaveformTestBeatmap(audio);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var playable = Beatmap.Value.GetPlayableBeatmap(Beatmap.Value.BeatmapInfo.Ruleset);
            EditorBeatmap = new EditorBeatmap(playable);

            Dependencies.Cache(EditorBeatmap);
            Dependencies.CacheAs<IBeatSnapProvider>(EditorBeatmap);

            Composer = playable.BeatmapInfo.Ruleset.CreateInstance().CreateHitObjectComposer();
            Debug.Assert(Composer != null);

            Composer.Alpha = 0;

            Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    EditorBeatmap,
                    Composer,
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Children = new Drawable[]
                        {
                            new StartStopButton(),
                            new AudioVisualiser(),
                        }
                    },
                    TimelineArea = new TimelineArea(CreateTestComponent())
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddUntilStep("wait for track loaded", () => MusicController.TrackLoaded);
            AddStep("seek forward", () => EditorClock.Seek(2500));
        }

        public abstract Drawable CreateTestComponent();

        private partial class AudioVisualiser : CompositeDrawable
        {
            private readonly Drawable marker;

            [Resolved]
            private IBindable<WorkingBeatmap> beatmap { get; set; }

            [Resolved]
            private EditorClock editorClock { get; set; }

            public AudioVisualiser()
            {
                Size = new Vector2(250, 25);

                InternalChildren = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.25f,
                    },
                    marker = new Box
                    {
                        RelativePositionAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Width = 2,
                    }
                };
            }

            protected override void Update()
            {
                base.Update();

                if (beatmap.Value.Track.IsLoaded)
                    marker.X = (float)(editorClock.CurrentTime / beatmap.Value.Track.Length);
            }
        }

        private partial class StartStopButton : OsuButton
        {
            [Resolved]
            private EditorClock editorClock { get; set; }

            public StartStopButton()
            {
                BackgroundColour = Color4.SlateGray;
                Size = new Vector2(100, 50);
                Text = "Start";

                Action = onClick;
            }

            private void onClick()
            {
                if (editorClock.IsRunning)
                    editorClock.Stop();
                else
                    editorClock.Start();
            }

            protected override void Update()
            {
                base.Update();

                Text = editorClock.IsRunning ? "Stop" : "Start";
            }
        }
    }
}
