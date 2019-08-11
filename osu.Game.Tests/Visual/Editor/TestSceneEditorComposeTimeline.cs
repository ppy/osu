// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editor
{
    [TestFixture]
    public class TestSceneEditorComposeTimeline : EditorClockTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(TimelineArea),
            typeof(Timeline),
            typeof(TimelineButton),
            typeof(CentreMarker)
        };

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            Beatmap.Value = new WaveformTestBeatmap(audio);

            Children = new Drawable[]
            {
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
                new TimelineArea
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(0.8f, 100)
                }
            };
        }

        private class AudioVisualiser : CompositeDrawable
        {
            private readonly Drawable marker;

            private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();
            private IAdjustableClock adjustableClock;

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

            [BackgroundDependencyLoader]
            private void load(IAdjustableClock adjustableClock, IBindable<WorkingBeatmap> beatmap)
            {
                this.adjustableClock = adjustableClock;
                this.beatmap.BindTo(beatmap);
            }

            protected override void Update()
            {
                base.Update();

                if (beatmap.Value.Track.IsLoaded)
                    marker.X = (float)(adjustableClock.CurrentTime / beatmap.Value.Track.Length);
            }
        }

        private class StartStopButton : Button
        {
            private IAdjustableClock adjustableClock;
            private bool started;

            public StartStopButton()
            {
                BackgroundColour = Color4.SlateGray;
                Size = new Vector2(100, 50);
                Text = "Start";

                Action = onClick;
            }

            [BackgroundDependencyLoader]
            private void load(IAdjustableClock adjustableClock)
            {
                this.adjustableClock = adjustableClock;
            }

            private void onClick()
            {
                if (started)
                {
                    adjustableClock.Stop();
                    Text = "Start";
                }
                else
                {
                    adjustableClock.Start();
                    Text = "Stop";
                }

                started = !started;
            }
        }
    }
}
