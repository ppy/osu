// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;
using osu.Game.Overlays;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Screens.Import
{
    public class FileImportScreen : OsuScreen
    {
        private Container contentContainer;
        private FileSelector fileSelector;
        private Container fileSelectContainer;

        public override bool HideOverlaysOnEnter => true;

        private string defaultPath;
        private readonly Bindable<FileInfo> currentFile = new Bindable<FileInfo>();
        private readonly IBindable<DirectoryInfo> currentDirectory = new Bindable<DirectoryInfo>();
        private TextFlowContainer currentFileText;
        private OsuScrollContainer fileNameScroll;
        private readonly OverlayColourProvider overlayColourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Resolved]
        private OsuGameBase gameBase { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(Storage storage)
        {
            storage.GetStorageForDirectory("imports");
            var originalPath = storage.GetFullPath("imports", true);
            string[] fileExtensions = { ".osk", ".osr", ".osz" };
            defaultPath = originalPath;
            var directory = currentDirectory.Value?.FullName ?? defaultPath;

            InternalChild = contentContainer = new Container
            {
                Masking = true,
                CornerRadius = 10,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(0.9f, 0.8f),
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = overlayColourProvider.Background5,
                        RelativeSizeAxes = Axes.Both,
                    },
                    fileSelectContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.65f,
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Child = fileSelector = new FileSelector(initialPath: directory, validFileExtensions: fileExtensions)
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.35f,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Masking = true,
                        CornerRadius = 10,
                        Children = new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                RowDimensions = new[]
                                {
                                    new Dimension(),
                                    new Dimension(GridSizeMode.AutoSize),
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Children = new Drawable[]
                                            {
                                                new Box
                                                {
                                                    Colour = overlayColourProvider.Background3,
                                                    RelativeSizeAxes = Axes.Both
                                                },
                                                fileNameScroll = new OsuScrollContainer
                                                {
                                                    Masking = false,
                                                    RelativeSizeAxes = Axes.Both,
                                                    Child = currentFileText = new TextFlowContainer(t => t.Font = OsuFont.Default.With(size: 30))
                                                    {
                                                        AutoSizeAxes = Axes.Y,
                                                        RelativeSizeAxes = Axes.X,
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        TextAnchor = Anchor.Centre
                                                    },
                                                },
                                            }
                                        },
                                    },
                                    new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Margin = new MarginPadding { Bottom = 15, Top = 15 },
                                            Children = new Drawable[]
                                            {
                                                new FillFlowContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Children = new Drawable[]
                                                    {
                                                        new TriangleButton
                                                        {
                                                            Text = "Import",
                                                            Anchor = Anchor.BottomCentre,
                                                            Origin = Anchor.BottomCentre,
                                                            RelativeSizeAxes = Axes.X,
                                                            Height = 50,
                                                            Width = 0.9f,
                                                            Action = () =>
                                                            {
                                                                var d = currentFile.Value?.FullName;
                                                                if (d != null)
                                                                    startImport(d);
                                                                else
                                                                    currentFileText.FlashColour(Color4.Red, 500);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            fileNameScroll.ScrollContent.Anchor = Anchor.Centre;
            fileNameScroll.ScrollContent.Origin = Anchor.Centre;

            currentFile.BindValueChanged(updateFileSelectionText, true);
            currentDirectory.BindValueChanged(_ =>
            {
                currentFile.Value = null;
            });

            currentDirectory.BindTo(fileSelector.CurrentPath);
            currentFile.BindTo(fileSelector.CurrentFile);
        }

        private void updateFileSelectionText(ValueChangedEvent<FileInfo> v)
        {
            currentFileText.Text = v.NewValue?.Name ?? "Select a file";
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            contentContainer.FadeOut().Then().ScaleTo(0.8f).RotateTo(-15).MoveToX(300)
                            .Then()
                            .ScaleTo(1, 1500, Easing.OutElastic)
                            .FadeIn(500)
                            .MoveToX(0, 500, Easing.OutQuint)
                            .RotateTo(0, 500, Easing.OutQuint);
        }

        public override bool OnExiting(IScreen next)
        {
            contentContainer.ScaleTo(0.8f, 500, Easing.OutExpo).RotateTo(-15, 500, Easing.OutExpo).MoveToX(300, 500, Easing.OutQuint).FadeOut(500);
            this.FadeOut(500, Easing.OutExpo);

            return base.OnExiting(next);
        }

        private void startImport(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (!File.Exists(path))
            {
                currentFileText.Text = "File not exist";
                currentFileText.FlashColour(Color4.Red, 500);
                return;
            }

            string[] paths = { path };

            Task.Factory.StartNew(() => gameBase.Import(paths), TaskCreationOptions.LongRunning);
        }
    }
}
