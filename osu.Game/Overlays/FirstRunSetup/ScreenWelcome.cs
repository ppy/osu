// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osuTK;

namespace osu.Game.Overlays.FirstRunSetup
{
    [LocalisableDescription(typeof(FirstRunSetupOverlayStrings), nameof(FirstRunSetupOverlayStrings.WelcomeTitle))]
    public partial class ScreenWelcome : FirstRunSetupScreen
    {
        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            Content.Children = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    RowDimensions = new[]
                    {
                        // Avoid height changes when changing language.
                        new Dimension(GridSizeMode.AutoSize, minSize: 100),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
                            {
                                Text = FirstRunSetupOverlayStrings.WelcomeDescription,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            },
                        },
                    }
                },
                new SettingsCheckbox
                {
                    LabelText = GeneralSettingsStrings.PreferOriginalMetadataLanguage,
                    Current = frameworkConfig.GetBindable<bool>(FrameworkSetting.ShowUnicode)
                },
                new LanguageSelectionFlow
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                }
            };
        }

        private partial class LanguageSelectionFlow : FillFlowContainer
        {
            private Bindable<Language> language = null!;

            private ScheduledDelegate? updateSelectedDelegate;

            [BackgroundDependencyLoader]
            private void load(OsuGameBase game)
            {
                Direction = FillDirection.Full;
                Spacing = new Vector2(5);

                ChildrenEnumerable = Enum.GetValues<Language>()
                                         .Select(l => new LanguageButton(l)
                                         {
                                             Action = () => language.Value = l,
                                         });

                language = game.CurrentLanguage.GetBoundCopy();
                language.BindValueChanged(v =>
                {
                    // Changing language may cause a short period of blocking the UI thread while the new glyphs are loaded.
                    // Scheduling ensures the button animation plays smoothly after any blocking operation completes.
                    // Note that a delay is required (the alternative would be a double-schedule; delay feels better).
                    updateSelectedDelegate?.Cancel();
                    updateSelectedDelegate = Scheduler.AddDelayed(() => updateSelectedStates(v.NewValue), 50);
                }, true);
            }

            private void updateSelectedStates(Language language)
            {
                foreach (var c in Children.OfType<LanguageButton>())
                    c.Selected = c.Language == language;
            }

            private partial class LanguageButton : OsuClickableContainer
            {
                public readonly Language Language;

                private Box backgroundBox = null!;

                private OsuSpriteText text = null!;

                [Resolved]
                private OverlayColourProvider colourProvider { get; set; } = null!;

                private bool selected;

                public bool Selected
                {
                    get => selected;
                    set
                    {
                        if (selected == value)
                            return;

                        selected = value;

                        if (IsLoaded)
                            updateState();
                    }
                }

                public LanguageButton(Language language)
                {
                    Language = language;

                    Size = new Vector2(160, 50);
                    Masking = true;
                    CornerRadius = 10;
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    AddRange(new Drawable[]
                    {
                        backgroundBox = new Box
                        {
                            Alpha = 0,
                            Colour = colourProvider.Background5,
                            RelativeSizeAxes = Axes.Both,
                        },
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = colourProvider.Light1,
                            Text = Language.GetDescription(),
                        }
                    });
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    updateState();
                }

                protected override bool OnHover(HoverEvent e)
                {
                    if (!selected)
                        updateState();
                    return base.OnHover(e);
                }

                protected override void OnHoverLost(HoverLostEvent e)
                {
                    if (!selected)
                        updateState();
                    base.OnHoverLost(e);
                }

                private void updateState()
                {
                    if (selected)
                    {
                        const double selected_duration = 1000;

                        backgroundBox.FadeTo(1, selected_duration, Easing.OutQuint);
                        backgroundBox.FadeColour(colourProvider.Background2, selected_duration, Easing.OutQuint);
                        text.FadeColour(colourProvider.Content1, selected_duration, Easing.OutQuint);
                        text.ScaleTo(1.2f, selected_duration, Easing.OutQuint);
                    }
                    else
                    {
                        const double duration = 500;

                        backgroundBox.FadeTo(IsHovered ? 1 : 0, duration, Easing.OutQuint);
                        backgroundBox.FadeColour(colourProvider.Background5, duration, Easing.OutQuint);
                        text.FadeColour(colourProvider.Light1, duration, Easing.OutQuint);
                        text.ScaleTo(1, duration, Easing.OutQuint);
                    }
                }
            }
        }
    }
}
