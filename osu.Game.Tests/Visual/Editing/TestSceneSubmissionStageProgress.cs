// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Submission;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneSubmissionStageProgress : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Resolved]
        private AudioManager audio { get; set; } = null!;

        private Sample? completeSample;

        [Test]
        public void TestAppearance()
        {
            float incrementingProgress = 0;

            SubmissionStageProgress progress = null!;

            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.8f),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = progress = new SubmissionStageProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    StageDescription = "Frobnicating the foobarator...",
                }
            });
            AddStep("not started", () => progress.SetNotStarted());
            AddStep("indeterminate progress", () => progress.SetInProgress());
            AddStep("increase progress to 100", () =>
            {
                incrementingProgress = 0;

                ScheduledDelegate? task = null;

                task = Scheduler.AddDelayed(() =>
                {
                    if (incrementingProgress >= 1)
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        task?.Cancel();
                        return;
                    }

                    if (RNG.NextDouble() < 0.01)
                        progress.SetInProgress(incrementingProgress += RNG.NextSingle(0.08f));
                }, 0, true);
            });
            AddStep("increase progress slowly then fail", () =>
            {
                incrementingProgress = 0;

                ScheduledDelegate? task = null;

                task = Scheduler.AddDelayed(() =>
                {
                    if (incrementingProgress >= 1)
                    {
                        progress.SetFailed("nope");
                        // ReSharper disable once AccessToModifiedClosure
                        task?.Cancel();
                        return;
                    }

                    progress.SetInProgress(incrementingProgress += RNG.NextSingle(0.001f));
                }, 0, true);
            });

            AddUntilStep("wait for completed", () => incrementingProgress >= 1);
            AddStep("completed", () => progress.SetCompleted());
            AddStep("failed", () => progress.SetFailed("the foobarator has defrobnicated"));
            AddStep("failed with long message", () => progress.SetFailed("this is a very very very very VERY VEEEEEEEEEEEEEEEEEEEEEEEEERY long error message like you would never believe"));
            AddStep("canceled", () => progress.SetCanceled());
        }

        [Test]
        public void TestAudioSequence()
        {
            SubmissionStageProgress[] stages = new SubmissionStageProgress[4];
            Container? cardContainer = null;

            AddStep("prepare", () =>
            {
                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.8f),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Children = new Drawable[]
                        {
                            stages[0] = new SubmissionStageProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                StageDescription = "Export...",
                                StageIndex = 0
                            },
                            stages[1] = new SubmissionStageProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                StageDescription = "CreateSet...",
                                StageIndex = 1
                            },
                            stages[2] = new SubmissionStageProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                StageDescription = "Upload...",
                                StageIndex = 2
                            },
                            stages[3] = new SubmissionStageProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                StageDescription = "Update...",
                                StageIndex = 3
                            },
                            cardContainer = new Container
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            }
                        }
                    }
                };

                completeSample = audio.Samples.Get(@"UI/bss-complete");
            });

            for (int i = 0; i < stages.Length; i++)
            {
                int step = i;
                AddStep($"{step}: not started", () => stages[step].SetNotStarted());
                AddStep($"{step}: indeterminate progress", () => stages[step].SetInProgress());
                AddStep($"{step}: 25% progress", () => stages[step].SetInProgress(0.25f));
                AddStep($"{step}: 70% progress", () => stages[step].SetInProgress(0.7f));
                AddStep($"{step}: completed", () => stages[step].SetCompleted());
            }

            AddWaitStep("pause for timing", 2);

            AddStep("Sequence Complete", () =>
            {
                var beatmapSet = CreateAPIBeatmapSet(Ruleset.Value);
                beatmapSet.Beatmaps = Enumerable.Repeat(beatmapSet.Beatmaps.First(), 100).ToArray();
                LoadComponentAsync(new BeatmapCardExtra(beatmapSet, false), loaded =>
                {
                    cardContainer?.Add(loaded);
                    completeSample?.Play();
                });
            });
        }
    }
}
