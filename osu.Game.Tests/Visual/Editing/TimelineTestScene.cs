// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editing
{
    public abstract class TimelineTestScene : EditorClockTestScene
    {
        protected TimelineArea TimelineArea { get; private set; }

        protected HitObjectComposer Composer { get; private set; }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            Beatmap.Value = new WaveformTestBeatmap(audio);

            var playable = Beatmap.Value.GetPlayableBeatmap(Beatmap.Value.BeatmapInfo.Ruleset);
            var editorBeatmap = new EditorBeatmap(playable);

            Dependencies.Cache(editorBeatmap);
            Dependencies.CacheAs<IBeatSnapProvider>(editorBeatmap);

            Composer = playable.BeatmapInfo.Ruleset.CreateInstance().CreateHitObjectComposer().With(d => d.Alpha = 0);

            AddRange(new Drawable[]
            {
                editorBeatmap,
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
                TimelineArea = new TimelineArea
                {
                    Child = CreateTestComponent(),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(0.8f, 100),
                }
            });
        }

        public abstract Drawable CreateTestComponent();

        private class AudioVisualiser : CompositeDrawable
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

        private class StartStopButton : OsuButton
        {
            [Resolved]
            private EditorClock editorClock { get; set; }

            private bool started;

            public StartStopButton()
            {
                BackgroundColour = Color4.SlateGray;
                Size = new Vector2(100, 50);
                Text = "Start";

                Action = onClick;
            }

            private void onClick()
            {
                if (started)
                {
                    editorClock.Stop();
                    Text = "Start";
                }
                else
                {
                    editorClock.Start();
                    Text = "Stop";
                }

                started = !started;
            }
        }
    }
}
