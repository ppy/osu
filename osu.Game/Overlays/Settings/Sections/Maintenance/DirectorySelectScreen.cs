// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using System.IO;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Screens;
using osuTK;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public abstract class DirectorySelectScreen : OsuScreen
    {
        private TriangleButton selectionButton;

        private OsuDirectorySelector directorySelector;

        /// <summary>
        /// Text to display in the header to inform the user of what they are selecting.
        /// </summary>
        public abstract LocalisableString HeaderText { get; }

        /// <summary>
        /// Called upon selection of a directory by the user.
        /// </summary>
        /// <param name="directory">The selected directory</param>
        protected abstract void OnSelection(DirectoryInfo directory);

        /// <summary>
        /// Whether the current directory is considered to be valid and can be selected.
        /// </summary>
        /// <param name="info">The current directory.</param>
        /// <returns>Whether the selected directory is considered valid.</returns>
        protected virtual bool IsValidDirectory(DirectoryInfo info) => true;

        /// <summary>
        /// The path at which to start selection from.
        /// </summary>
        protected virtual DirectoryInfo InitialPath => null;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChild = new Container
            {
                Masking = true,
                CornerRadius = 10,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(0.5f, 0.8f),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.GreySeaFoamDark
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(),
                            new Dimension(GridSizeMode.AutoSize),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new OsuTextFlowContainer(cp =>
                                {
                                    cp.Font = OsuFont.Default.With(size: 24);
                                })
                                {
                                    Text = HeaderText,
                                    TextAnchor = Anchor.TopCentre,
                                    Margin = new MarginPadding(10),
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                }
                            },
                            new Drawable[]
                            {
                                directorySelector = new OsuDirectorySelector
                                {
                                    RelativeSizeAxes = Axes.Both,
                                }
                            },
                            new Drawable[]
                            {
                                selectionButton = new TriangleButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Width = 300,
                                    Margin = new MarginPadding(10),
                                    Text = MaintenanceSettingsStrings.SelectDirectory,
                                    Action = () => OnSelection(directorySelector.CurrentPath.Value)
                                },
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            if (InitialPath != null)
                directorySelector.CurrentPath.Value = InitialPath;

            directorySelector.CurrentPath.BindValueChanged(e => selectionButton.Enabled.Value = e.NewValue != null && IsValidDirectory(e.NewValue), true);
            base.LoadComplete();
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);

            this.FadeOut(250);
        }
    }
}
