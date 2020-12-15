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
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Import
{
    public class FileImportScreen : OsuScreen
    {
        public override bool HideOverlaysOnEnter => true;

        private FileSelector fileSelector;
        private Container contentContainer;
        private TextFlowContainer currentFileText;

        private TriangleButton importButton;

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
                    fileSelector = new FileSelector(validFileExtensions: game.HandledExtensions.ToArray())
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
                                Child = new OsuScrollContainer
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
                                    ScrollContent =
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                    }
                                },
                            },
                            importButton = new TriangleButton
                            {
                                Text = "Import",
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                RelativeSizeAxes = Axes.X,
                                Height = button_height,
                                Width = 0.9f,
                                Margin = new MarginPadding { Vertical = button_vertical_margin },
                                Action = () => startImport(fileSelector.CurrentFile.Value?.FullName)
                            }
                        }
                    }
                }
            };

            fileSelector.CurrentFile.BindValueChanged(fileChanged, true);
            fileSelector.CurrentPath.BindValueChanged(directoryChanged);
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            contentContainer.ScaleTo(0.95f).ScaleTo(1, duration, Easing.OutQuint);
            this.FadeInFromZero(duration);
        }

        public override bool OnExiting(IScreen next)
        {
            contentContainer.ScaleTo(0.95f, duration, Easing.OutQuint);
            this.FadeOut(duration, Easing.OutQuint);

            return base.OnExiting(next);
        }

        private void directoryChanged(ValueChangedEvent<DirectoryInfo> _)
        {
            // this should probably be done by the selector itself, but let's do it here for now.
            fileSelector.CurrentFile.Value = null;
        }

        private void fileChanged(ValueChangedEvent<FileInfo> selectedFile)
        {
            importButton.Enabled.Value = selectedFile.NewValue != null;
            currentFileText.Text = selectedFile.NewValue?.Name ?? "Select a file";
        }

        private void startImport(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            Task.Factory.StartNew(async () =>
            {
                await game.Import(path);

                // some files will be deleted after successful import, so we want to refresh the view.
                Schedule(() =>
                {
                    // should probably be exposed as a refresh method.
                    fileSelector.CurrentPath.TriggerChange();
                });
            }, TaskCreationOptions.LongRunning);
        }
    }
}
