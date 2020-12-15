// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
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
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Screens.Import
{
    public class FileImportScreen : OsuScreen
    {
        public override bool HideOverlaysOnEnter => true;

        private readonly Bindable<FileInfo> currentFile = new Bindable<FileInfo>();
        private readonly IBindable<DirectoryInfo> currentDirectory = new Bindable<DirectoryInfo>();

        private FileSelector fileSelector;
        private Container contentContainer;
        private TextFlowContainer currentFileText;
        private OsuScrollContainer fileNameScroll;

        private const float duration = 300;
        private const float button_height = 50;
        private const float button_vertical_margin = 15;

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(Storage storage)
        {
            storage.GetStorageForDirectory("imports");
            var originalPath = storage.GetFullPath("imports", true);

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
                        Colour = colours.GreySeafoamDark,
                        RelativeSizeAxes = Axes.Both,
                    },
                    fileSelector = new FileSelector(originalPath, game.HandledExtensions.ToArray())
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.65f
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.35f,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = colours.GreySeafoamDarker,
                                RelativeSizeAxes = Axes.Both
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Bottom = button_height + button_vertical_margin * 2 },
                                Child = fileNameScroll = new OsuScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Child = currentFileText = new TextFlowContainer(t => t.Font = OsuFont.Default.With(size: 30))
                                    {
                                        AutoSizeAxes = Axes.Y,
                                        RelativeSizeAxes = Axes.X,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        TextAnchor = Anchor.Centre
                                    },
                                },
                            },
                            new TriangleButton
                            {
                                Text = "Import",
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                RelativeSizeAxes = Axes.X,
                                Height = button_height,
                                Width = 0.9f,
                                Margin = new MarginPadding { Vertical = button_vertical_margin },
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
            };
            fileNameScroll.ScrollContent.Anchor = Anchor.Centre;
            fileNameScroll.ScrollContent.Origin = Anchor.Centre;

            currentFile.BindValueChanged(updateFileSelectionText, true);
            currentDirectory.BindValueChanged(onCurrentDirectoryChanged);

            currentDirectory.BindTo(fileSelector.CurrentPath);
            currentFile.BindTo(fileSelector.CurrentFile);
        }

        private void onCurrentDirectoryChanged(ValueChangedEvent<DirectoryInfo> v)
        {
            currentFile.Value = null;
        }

        private void updateFileSelectionText(ValueChangedEvent<FileInfo> v)
        {
            currentFileText.Text = v.NewValue?.Name ?? "Select a file";
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            contentContainer.FadeOut().Then().ScaleTo(0.95f)
                            .Then()
                            .ScaleTo(1, duration, Easing.OutQuint)
                            .FadeIn(duration);
        }

        public override bool OnExiting(IScreen next)
        {
            contentContainer.ScaleTo(0.95f, duration, Easing.OutQuint);
            this.FadeOut(duration, Easing.OutQuint);

            return base.OnExiting(next);
        }

        private void startImport(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (!File.Exists(path))
            {
                currentFileText.Text = "No such file";
                currentFileText.FlashColour(colours.Red, duration);
                return;
            }

            string[] paths = { path };

            Task.Factory.StartNew(() => game.Import(paths), TaskCreationOptions.LongRunning);
        }
    }
}
