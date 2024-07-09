// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
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

        private readonly Editor? editor;

        private ExternalEditOperation<BeatmapSetInfo>? operation;

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
                    new FillFlowContainer
                    {
                        Margin = new MarginPadding(20),
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Spacing = new Vector2(15),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "Beatmap is mounted externally",
                                Font = OsuFont.Default.With(size: 30),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Spacing = new Vector2(15),
                                Children = new Drawable[]
                                {
                                }
                            },
                            new PurpleRoundedButton
                            {
                                Text = "Open folder",
                                Width = 350,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Action = open,
                            },
                            new DangerousRoundedButton
                            {
                                Text = "Finish editing and import changes",
                                Width = 350,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Action = finish,
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            fileMountOperation.ContinueWith(t =>
            {
                operation = t.GetResultSafely();
                Schedule(open);
            });
        }

        private void open()
        {
            if (operation == null)
                return;

            // Ensure the trailing separator is present in order to show the folder contents.
            gameHost.OpenFileExternally(operation.MountedPath.TrimDirectorySeparator() + Path.DirectorySeparatorChar);
        }

        private void finish()
        {
            fileMountOperation.GetResultSafely().Finish().ContinueWith(t =>
            {
                Schedule(() =>
                {
                    editor?.SwitchToDifficulty(t.GetResultSafely<Live<BeatmapSetInfo>>().Value.Detach().Beatmaps.First());
                });
            });
        }
    }
}
