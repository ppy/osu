// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework.Internal;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Audio;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    public abstract partial class HitSoundTrackTestScene : TimelineTestScene
    {
        protected abstract HitSoundTrackMode[] GetModes();

        private const float track_height = 100;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var modes = GetModes();
            var timeline = this.ChildrenOfType<Timeline>().FirstOrDefault();

            if (timeline == null)
                return;

            timeline.Height = 100 * modes.Length;
            timeline.Add(new FillFlowContainer
            {
                Height = 100 * modes.Length,
                RelativeSizeAxes = Axes.X,
                Direction = FillDirection.Vertical,
                Children = modes.ToList().ConvertAll(mode => new HitSoundTrackSamplePointBlueprintContainer(mode)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = track_height,
                    AlwaysPresent = true,
                }).ToArray(),
            });
        }

        public override Drawable CreateTestComponent()
        {
            return new Box();
        }

        public void AddHitCircles(int count, float startTime = 1000)
        {
            int i = 0;
            AddRepeatStep("add hit circles", () =>
            {
                EditorBeatmap.Add(new HitCircle { StartTime = startTime + (++i) * 200 });
            }, count);
        }

        public void SetHitCircles(int count, float startTime = 1000)
        {
            AddStep("clear all hit objects", EditorBeatmap.Clear);
            AddHitCircles(count, startTime);
        }

        public void AddSliders(int count, int repeatCount = 2, float startTime = 1000)
        {
            int i = 0;
            var random = Randomizer.CreateRandomizer();

            AddRepeatStep($"add sliders with {repeatCount} repeat", () =>
            {
                EditorBeatmap.Add(new Slider
                {
                    Position = new Vector2(128, 256),
                    Path = new SliderPath(PathType.LINEAR,
                        Enumerable
                            .Repeat(0, repeatCount)
                            .Select(_ => new Vector2(random.NextFloat(128, 256), random.NextFloat(128, 256)))
                            .ToArray()
                    ),
                    RepeatCount = repeatCount - 2,
                    Scale = 0.5f,
                    StartTime = startTime + (++i) * 500 * repeatCount,
                });
            }, count);
        }

        public void SetSliders(int count, int repeatCount = 2, float startTime = 1000)
        {
            AddStep("clear all hit objects", EditorBeatmap.Clear);
            AddSliders(count, repeatCount, startTime);
        }

        public void AddSpinners(int count, float startTime = 1000)
        {
            int i = 0;

            AddRepeatStep("add spinners", () =>
            {
                EditorBeatmap.Add(new Spinner
                {
                    Position = new Vector2(128, 256),
                    Duration = 100,
                    StartTime = startTime + (++i) * 200,
                });
            }, count);
        }

        public void SetSpinners(int count, float startTime = 1000)
        {
            AddStep("clear all hit objects", EditorBeatmap.Clear);
            AddSpinners(count, startTime);
        }
    }
}
