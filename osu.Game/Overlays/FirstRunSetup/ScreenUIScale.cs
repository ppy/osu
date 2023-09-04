// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Overlays.FirstRunSetup
{
    [LocalisableDescription(typeof(GraphicsSettingsStrings), nameof(GraphicsSettingsStrings.UIScaling))]
    public partial class ScreenUIScale : FirstRunSetupScreen
    {
        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            const float screen_width = 640;

            Content.Children = new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
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
                    Size = new Vector2(screen_width, screen_width / 16f * 9),
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Content = new[]
                            {
                                new Drawable[]
                                {
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

        private partial class InverseScalingDrawSizePreservingFillContainer : ScalingContainer.ScalingDrawSizePreservingFillContainer
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

        private partial class NestedSongSelect : PlaySongSelect
        {
            protected override bool ControlGlobalMusic => false;

            public override bool? ApplyModTrackAdjustments => false;
        }

        private partial class UIScaleSlider : RoundedSliderBar<float>
        {
            public override LocalisableString TooltipText => base.TooltipText + "x";
        }

        private partial class SampleScreenContainer : CompositeDrawable
        {
            private readonly OsuScreen screen;

            // Minimal isolation from main game.

            [Cached]
            [Cached(typeof(IBindable<RulesetInfo>))]
            protected readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

            [Cached]
            [Cached(typeof(IBindable<WorkingBeatmap>))]
            protected Bindable<WorkingBeatmap> Beatmap { get; private set; } = new Bindable<WorkingBeatmap>();

            [Cached]
            [Cached(typeof(IBindable<IReadOnlyList<Mod>>))]
            protected Bindable<IReadOnlyList<Mod>> SelectedMods { get; private set; } = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

            public override bool HandlePositionalInput => false;
            public override bool HandleNonPositionalInput => false;
            public override bool PropagatePositionalInputSubTree => false;
            public override bool PropagateNonPositionalInputSubTree => false;

            public SampleScreenContainer(OsuScreen screen)
            {
                this.screen = screen;
                RelativeSizeAxes = Axes.Both;
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
                new DependencyContainer(new DependencyIsolationContainer(base.CreateChildDependencies(parent)));

            [BackgroundDependencyLoader]
            private void load(AudioManager audio, TextureStore textures, RulesetStore rulesets)
            {
                Beatmap.Value = new DummyWorkingBeatmap(audio, textures);

                Ruleset.Value = rulesets.AvailableRulesets.First();

                OsuScreenStack stack;
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

                // intentionally load synchronously so it is included in the initial load of the first run screen.
                stack.PushSynchronously(screen);
            }
        }

        private class DependencyIsolationContainer : IReadOnlyDependencyContainer
        {
            private readonly IReadOnlyDependencyContainer parentDependencies;

            private readonly Type[] isolatedTypes =
            {
                typeof(OsuGame)
            };

            public DependencyIsolationContainer(IReadOnlyDependencyContainer parentDependencies)
            {
                this.parentDependencies = parentDependencies;
            }

            public object Get(Type type)
            {
                if (isolatedTypes.Contains(type))
                    return null;

                return parentDependencies.Get(type);
            }

            public object Get(Type type, CacheInfo info)
            {
                if (isolatedTypes.Contains(type))
                    return null;

                return parentDependencies.Get(type, info);
            }

            public void Inject<T>(T instance) where T : class, IDependencyInjectionCandidate
            {
                parentDependencies.Inject(instance);
            }
        }
    }
}
