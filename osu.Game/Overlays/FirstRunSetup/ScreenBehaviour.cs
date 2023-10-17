// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections;

namespace osu.Game.Overlays.FirstRunSetup
{
    [LocalisableDescription(typeof(FirstRunSetupOverlayStrings), nameof(FirstRunSetupOverlayStrings.Behaviour))]
    public partial class ScreenBehaviour : FirstRunSetupScreen
    {
        private SearchContainer<SettingsSection> searchContainer;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Content.Children = new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
                {
                    Text = FirstRunSetupOverlayStrings.BehaviourDescription,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 10),
                        new Dimension(),
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new[]
                        {
                            new RoundedButton
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Text = FirstRunSetupOverlayStrings.NewDefaults,
                                RelativeSizeAxes = Axes.X,
                                Action = applyStandard,
                            },
                            Empty(),
                            new RoundedButton
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                BackgroundColour = colours.DangerousButtonColour,
                                Text = FirstRunSetupOverlayStrings.ClassicDefaults,
                                RelativeSizeAxes = Axes.X,
                                Action = applyClassic
                            }
                        },
                    },
                },
                searchContainer = new SearchContainer<SettingsSection>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new SettingsSection[]
                    {
                        // This list should be kept in sync with SettingsOverlay.
                        new GeneralSection(),
                        new SkinSection(),
                        // InputSection is intentionally omitted for now due to its sub-panel being a pain to set up.
                        new UserInterfaceSection(),
                        new GameplaySection(),
                        new RulesetSection(),
                        new AudioSection(),
                        new GraphicsSection(),
                        new OnlineSection(),
                        new MaintenanceSection(),
                        new DebugSection(),
                    },
                    SearchTerm = SettingsItem<bool>.CLASSIC_DEFAULT_SEARCH_TERM,
                }
            };
        }

        private void applyClassic()
        {
            foreach (var i in searchContainer.ChildrenOfType<ISettingsItem>().Where(s => s.HasClassicDefault))
                i.ApplyClassicDefault();
        }

        private void applyStandard()
        {
            foreach (var i in searchContainer.ChildrenOfType<ISettingsItem>().Where(s => s.HasClassicDefault))
                i.ApplyDefault();
        }
    }
}
