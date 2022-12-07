// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Tests.Beatmaps
{
    public abstract partial class LegacyBeatmapSkinColourTest : ScreenTestScene
    {
        protected readonly Bindable<bool> BeatmapSkins = new Bindable<bool>();
        protected readonly Bindable<bool> BeatmapColours = new Bindable<bool>();

        protected ExposedPlayer TestPlayer;

        private WorkingBeatmap testBeatmap;

        protected void PrepareBeatmap(Func<WorkingBeatmap> createBeatmap) => AddStep("prepare beatmap", () => testBeatmap = createBeatmap());

        protected void ConfigureTest(bool useBeatmapSkin, bool useBeatmapColours, bool userHasCustomColours)
        {
            configureSettings(useBeatmapSkin, useBeatmapColours);
            AddStep("load beatmap", () => TestPlayer = LoadBeatmap(userHasCustomColours));
            AddUntilStep("wait for player load", () => TestPlayer.IsLoaded);
        }

        private void configureSettings(bool beatmapSkins, bool beatmapColours)
        {
            AddStep($"{(beatmapSkins ? "enable" : "disable")} beatmap skins", () =>
            {
                BeatmapSkins.Value = beatmapSkins;
            });
            AddStep($"{(beatmapColours ? "enable" : "disable")} beatmap colours", () =>
            {
                BeatmapColours.Value = beatmapColours;
            });
        }

        protected virtual ExposedPlayer LoadBeatmap(bool userHasCustomColours)
        {
            ExposedPlayer player;

            Beatmap.Value = testBeatmap;

            LoadScreen(player = CreateTestPlayer(userHasCustomColours));

            return player;
        }

        protected virtual ExposedPlayer CreateTestPlayer(bool userHasCustomColours) => new ExposedPlayer(userHasCustomColours);

        protected partial class ExposedPlayer : TestPlayer
        {
            protected readonly bool UserHasCustomColours;

            public ExposedPlayer(bool userHasCustomColours)
                : base(false, false)
            {
                UserHasCustomColours = userHasCustomColours;
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
                dependencies.CacheAs<ISkinSource>(new TestSkin(UserHasCustomColours));
                return dependencies;
            }

            public IReadOnlyList<Color4> UsableComboColours =>
                GameplayClockContainer.ChildrenOfType<BeatmapSkinProvidingContainer>()
                                      .First()
                                      .GetConfig<GlobalSkinColours, IReadOnlyList<Color4>>(GlobalSkinColours.ComboColours)?.Value;
        }

        protected class CustomSkinWorkingBeatmap : ClockBackedTestWorkingBeatmap
        {
            public readonly bool HasColours;

            public CustomSkinWorkingBeatmap(IBeatmap beatmap, AudioManager audio, bool hasColours)
                : base(beatmap, null, null, audio)
            {
                HasColours = hasColours;
            }

            protected internal override ISkin GetSkin() => new TestBeatmapSkin(BeatmapInfo, HasColours);
        }

        protected class TestBeatmapSkin : LegacyBeatmapSkin
        {
            public static Color4[] Colours { get; } =
            {
                new Color4(50, 100, 150, 255),
                new Color4(40, 80, 120, 255),
                new Color4(25, 50, 75, 255),
                new Color4(10, 20, 30, 255),
            };

            public static readonly Color4 HYPER_DASH_COLOUR = Color4.DarkBlue;

            public static readonly Color4 HYPER_DASH_AFTER_IMAGE_COLOUR = Color4.DarkCyan;

            public static readonly Color4 HYPER_DASH_FRUIT_COLOUR = Color4.DarkGoldenrod;

            public TestBeatmapSkin(BeatmapInfo beatmapInfo, bool hasColours)
                : base(beatmapInfo, null)
            {
                if (hasColours)
                {
                    Configuration.CustomComboColours = Colours.ToList();
                    Configuration.CustomColours.Add("HyperDash", HYPER_DASH_COLOUR);
                    Configuration.CustomColours.Add("HyperDashAfterImage", HYPER_DASH_AFTER_IMAGE_COLOUR);
                    Configuration.CustomColours.Add("HyperDashFruit", HYPER_DASH_FRUIT_COLOUR);
                }
            }
        }

        protected class TestSkin : LegacySkin, ISkinSource
        {
            public static Color4[] Colours { get; } =
            {
                new Color4(150, 100, 50, 255),
                new Color4(20, 20, 20, 255),
                new Color4(75, 50, 25, 255),
                new Color4(80, 80, 80, 255),
            };

            public static readonly Color4 HYPER_DASH_COLOUR = Color4.LightBlue;

            public static readonly Color4 HYPER_DASH_AFTER_IMAGE_COLOUR = Color4.LightCoral;

            public static readonly Color4 HYPER_DASH_FRUIT_COLOUR = Color4.LightCyan;

            public TestSkin(bool hasCustomColours)
                : base(new SkinInfo(), null, null)
            {
                if (hasCustomColours)
                {
                    Configuration.CustomComboColours = Colours.ToList();
                    Configuration.CustomColours.Add("HyperDash", HYPER_DASH_COLOUR);
                    Configuration.CustomColours.Add("HyperDashAfterImage", HYPER_DASH_AFTER_IMAGE_COLOUR);
                    Configuration.CustomColours.Add("HyperDashFruit", HYPER_DASH_FRUIT_COLOUR);
                }
            }

            public event Action SourceChanged
            {
                add { }
                remove { }
            }

            public ISkin FindProvider(Func<ISkin, bool> lookupFunction) => lookupFunction(this) ? this : null;

            public IEnumerable<ISkin> AllSources => new[] { this };
        }
    }
}
