// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.OnlinePlay
{
    [HeadlessTest]
    public partial class TestSceneOnlinePlaySubScreenStack : OnlinePlayTestScene
    {
        private ScreenStack stack = null!;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = stack = new OnlinePlaySubScreenStack
            {
                RelativeSizeAxes = Axes.Both
            };
        });

        [Test]
        public void TestBindablesDisabledWhenRequested()
        {
            AddAssert("bindables not disabled", () => Beatmap.Disabled || Ruleset.Disabled || SelectedMods.Disabled, () => Is.False);

            AddStep("push screen that disables bindables", () => stack.Push(new ScreenWithExternalBindableDisablement(true)));
            AddAssert("bindables disabled", () => Beatmap.Disabled && Ruleset.Disabled && SelectedMods.Disabled, () => Is.True);

            AddStep("push screen that does not disable bindables", () => stack.Push(new ScreenWithExternalBindableDisablement(false)));
            AddAssert("bindables not disabled", () => Beatmap.Disabled || Ruleset.Disabled || SelectedMods.Disabled, () => Is.False);

            AddStep("exit one screen", () => stack.Exit());
            AddAssert("bindables disabled", () => Beatmap.Disabled && Ruleset.Disabled && SelectedMods.Disabled, () => Is.True);
        }

        [Test]
        public void TestModsResetWhenExitToLounge()
        {
            AddStep("push lounge", () => stack.Push(new PlaylistsLoungeSubScreen()));

            AddStep("push screen with mod", () => stack.Push(new ScreenWithMod(new OsuModDoubleTime())));
            AddUntilStep("wait for screen to load", () => ((OsuScreen)stack.CurrentScreen).IsLoaded);
            AddAssert("mod set", () => SelectedMods.Value.Count, () => Is.GreaterThan(0));

            AddStep("exit to lounge", () => stack.Exit());
            AddAssert("mods reset", () => SelectedMods.Value.Count, () => Is.Zero);
        }

        private partial class ScreenWithExternalBindableDisablement : OsuScreen
        {
            public override bool DisallowExternalBeatmapRulesetChanges { get; }

            public ScreenWithExternalBindableDisablement(bool disableBindables)
            {
                DisallowExternalBeatmapRulesetChanges = disableBindables;
            }
        }

        private partial class ScreenWithMod : OsuScreen
        {
            private readonly Mod mod;

            public ScreenWithMod(Mod mod)
            {
                this.mod = mod;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Mods.Value = [mod];
            }
        }
    }
}
