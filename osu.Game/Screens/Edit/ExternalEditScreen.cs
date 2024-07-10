// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osuTK;

namespace osu.Game.Screens.Edit
{
    internal partial class ExternalEditScreen : OsuScreen
    {
        private readonly Task<ExternalEditOperation<BeatmapSetInfo>> fileMountOperation;

        [Resolved]
        private GameHost gameHost { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private readonly Editor editor;

        public ExternalEditOperation<BeatmapSetInfo>? EditOperation;

        private double timeLoaded;

        private FillFlowContainer flow = null!;

        public ExternalEditScreen(Task<ExternalEditOperation<BeatmapSetInfo>> fileMountOperation, Editor editor)
        {
            this.fileMountOperation = fileMountOperation;
            this.editor = editor;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Container
            {
                Masking = true,
                CornerRadius = 20,
                AutoSizeAxes = Axes.Both,
                AutoSizeDuration = 500,
                AutoSizeEasing = Easing.OutQuint,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = colourProvider.Background5,
                        RelativeSizeAxes = Axes.Both,
                    },
                    flow = new FillFlowContainer
                    {
                        Margin = new MarginPadding(20),
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Spacing = new Vector2(15),
                    }
                }
            };

            showSpinner("Exporting for edit...");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            timeLoaded = Time.Current;

            fileMountOperation.ContinueWith(t =>
            {
                EditOperation = t.GetResultSafely();

                Scheduler.AddDelayed(() =>
                {
                    flow.Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "Beatmap is mounted externally",
                            Font = OsuFont.Default.With(size: 30),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                        new OsuTextFlowContainer
                        {
                            Padding = new MarginPadding(5),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Width = 350,
                            AutoSizeAxes = Axes.Y,
                            Text = "Any changes made to the exported folder will be imported to the game, including file additions, modifications and deletions.",
                        },
                        new PurpleRoundedButton
                        {
                            Text = "Open folder",
                            Width = 350,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Action = open,
                            Enabled = { Value = false }
                        },
                        new DangerousRoundedButton
                        {
                            Text = "Finish editing and import changes",
                            Width = 350,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Action = finish,
                            Enabled = { Value = false }
                        }
                    };

                    Scheduler.AddDelayed(() =>
                    {
                        foreach (var b in flow.ChildrenOfType<RoundedButton>())
                            b.Enabled.Value = true;
                        open();
                    }, 1000);
                }, Math.Max(0, 1000 - (Time.Current - timeLoaded)));
            });
        }

        private void open()
        {
            if (EditOperation == null)
                return;

            // Ensure the trailing separator is present in order to show the folder contents.
            gameHost.OpenFileExternally(EditOperation.MountedPath.TrimDirectorySeparator() + Path.DirectorySeparatorChar);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (!fileMountOperation.IsCompleted)
                return true;

            if (EditOperation != null)
            {
                finish();
                return true;
            }

            return base.OnExiting(e);
        }

        private void finish()
        {
            string originalDifficulty = editor.Beatmap.Value.Beatmap.BeatmapInfo.DifficultyName;

            showSpinner("Cleaning up...");

            EditOperation!.Finish().ContinueWith(t =>
            {
                Schedule(() =>
                {
                    // Setting to null will allow exit to succeed.
                    EditOperation = null;

                    Live<BeatmapSetInfo>? beatmap = t.GetResultSafely();

                    if (beatmap == null)
                        this.Exit();
                    else
                    {
                        var closestMatchingBeatmap =
                            beatmap.Value.Beatmaps.FirstOrDefault(b => b.DifficultyName == originalDifficulty)
                            ?? beatmap.Value.Beatmaps.First();

                        editor.SwitchToDifficulty(closestMatchingBeatmap);
                    }
                });
            });
        }

        private void showSpinner(string text)
        {
            foreach (var b in flow.ChildrenOfType<RoundedButton>())
                b.Enabled.Value = false;

            flow.Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = text,
                    Font = OsuFont.Default.With(size: 30),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                },
                new LoadingSpinner
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    State = { Value = Visibility.Visible }
                },
            };
        }
    }
}
