// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Overlays.FirstRunSetup
{
    [LocalisableDescription(typeof(GraphicsSettingsStrings), nameof(GraphicsSettingsStrings.UIScaling))]
    public class ScreenUIScale : FirstRunSetupScreen
    {
        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Content.Children = new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 24))
                {
                    Text = FirstRunSetupOverlayStrings.UIScaleDescription,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                new SettingsSlider<float, UIScaleSlider>
                {
                    LabelText = GraphicsSettingsStrings.UIScaling,
                    Current = config.GetBindable<float>(OsuSetting.UIScale),
                    KeyboardStep = 0.01f,
                },
                new InverseScalingDrawSizePreservingFillContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(960, 960 / 16f * 9 / 2),
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new SampleScreenContainer(new PinnedMainMenu()),
                                    new SampleScreenContainer(new NestedSongSelect()),
                                },
                                // TODO: add more screens here in the future (gameplay / results)
                                // requires a bit more consideration to isolate their behaviour from the "parent" game.
                            }
                        }
                    }
                }
            };
        }

        private class InverseScalingDrawSizePreservingFillContainer : ScalingContainer.ScalingDrawSizePreservingFillContainer
        {
            private Vector2 initialSize;

            public InverseScalingDrawSizePreservingFillContainer()
                : base(true)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                initialSize = Size;
            }

            protected override void Update()
            {
                Size = initialSize / CurrentScale;
            }
        }

        private class NestedSongSelect : PlaySongSelect
        {
            protected override bool ControlGlobalMusic => false;
        }

        private class PinnedMainMenu : MainMenu
        {
            public override void OnEntering(ScreenTransitionEvent e)
            {
                base.OnEntering(e);

                Buttons.ReturnToTopOnIdle = false;
                Buttons.State = ButtonSystemState.TopLevel;
            }
        }

        private class UIScaleSlider : OsuSliderBar<float>
        {
            public override LocalisableString TooltipText => base.TooltipText + "x";
        }

        private class SampleScreenContainer : CompositeDrawable
        {
            // Minimal isolation from main game.

            [Cached]
            [Cached(typeof(IBindable<RulesetInfo>))]
            protected readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

            [Cached]
            [Cached(typeof(IBindable<WorkingBeatmap>))]
            protected Bindable<WorkingBeatmap> Beatmap { get; private set; } = new Bindable<WorkingBeatmap>();

            public override bool HandlePositionalInput => false;
            public override bool HandleNonPositionalInput => false;
            public override bool PropagatePositionalInputSubTree => false;
            public override bool PropagateNonPositionalInputSubTree => false;

            [BackgroundDependencyLoader]
            private void load(AudioManager audio, TextureStore textures, RulesetStore rulesets)
            {
                Beatmap.Value = new DummyWorkingBeatmap(audio, textures);
                Beatmap.Value.LoadTrack();

                Ruleset.Value = rulesets.AvailableRulesets.First();
            }

            public SampleScreenContainer(Screen screen)
            {
                OsuScreenStack stack;
                RelativeSizeAxes = Axes.Both;

                OsuLogo logo;

                Padding = new MarginPadding(5);

                InternalChildren = new Drawable[]
                {
                    new DependencyProvidingContainer
                    {
                        CachedDependencies = new (Type, object)[]
                        {
                            (typeof(OsuLogo), logo = new OsuLogo
                            {
                                RelativePositionAxes = Axes.Both,
                                Position = new Vector2(0.5f),
                            })
                        },
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new ScalingContainer.ScalingDrawSizePreservingFillContainer(true)
                            {
                                Masking = true,
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    stack = new OsuScreenStack(),
                                    logo
                                },
                            },
                        }
                    },
                };

                stack.Push(screen);
            }
        }
    }
}
