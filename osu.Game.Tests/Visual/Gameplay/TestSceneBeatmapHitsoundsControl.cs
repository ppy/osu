// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Localisation;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneBeatmapHitsoundsControl : OsuTestScene
    {

        private BeatmapHitsoundsControl hitsoundsControl = null!;
        private OsuConfigManager localConfig = null!;

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;
        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;


        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(localConfig = new OsuConfigManager(LocalStorage));
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset settings", () => localConfig.SetValue(OsuSetting.BeatmapHitsounds, true));

            AddStep("create working beatmap", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);
            });

            recreateControl();
        }


        [Test]
        public void TestLocalConfigSyncsWithControl()
        {
            AddStep("disable beatmap hitsounds globally", () => localConfig.SetValue(OsuSetting.BeatmapHitsounds, false));
            AddAssert("control shows hitsounds disabled", () => !hitsoundsControl.Current.Value);

            AddStep("enable beatmap hitsounds globally", () => localConfig.SetValue(OsuSetting.BeatmapHitsounds, true));
            AddAssert("control shows hitsounds enabled", () => hitsoundsControl.Current.Value);
        }


        [Test]
        public void TestLocalConfigNoLongerSyncsWithControlAfterChange()
        {

            AddStep("disable beatmap hitsounds globally", () => localConfig.SetValue(OsuSetting.BeatmapHitsounds, true));
            AddAssert("control shows hitsounds enabled", () => hitsoundsControl.Current.Value);

            AddStep("simulate mouse down", () =>
            {
                hitsoundsControl.TriggerEvent(new MouseDownEvent(GetContainingInputManager()?.CurrentState ?? new InputState(), osuTK.Input.MouseButton.Left));
            });

            AddStep("disable beatmap hitsounds globally", () => localConfig.SetValue(OsuSetting.BeatmapHitsounds, false));
            AddAssert("control shows hitsounds disabled", () => hitsoundsControl.Current.Value);



        }

        [Test]
        public void TestBeatmapHitsoundValueChangesInBeatmap()
        {

            AddStep("disable beatmap hitsounds globally", () => localConfig.SetValue(OsuSetting.BeatmapHitsounds, true));
            AddAssert("control shows hitsounds enabled", () => hitsoundsControl.Current.Value);

            AddStep("simulate mouse down", () =>
            {
                hitsoundsControl.TriggerEvent(new MouseDownEvent(GetContainingInputManager()?.CurrentState ?? new InputState(), osuTK.Input.MouseButton.Left));
            });
            AddStep("change the current value", () => hitsoundsControl.Current.Value = false);
            AddAssert("beatmaps hitsound value is false", () => Beatmap.Value.BeatmapInfo.UserSettings.Hitsounds == HitsoundsSetting.HitsoundsOff);


            AddStep("change the current value", () => hitsoundsControl.Current.Value = true);
            AddAssert("beatmaps hitsound value is true", () => Beatmap.Value.BeatmapInfo.UserSettings.Hitsounds == HitsoundsSetting.HitsoundsOn);


        }


        private void recreateControl()
        {
            AddStep("Create control", () =>
            {
                Child = new PlayerSettingsGroup("Some settings")
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        hitsoundsControl = new BeatmapHitsoundsControl { LabelText = SkinSettingsStrings.BeatmapHitsounds }
                    }
                };
            });
        }

    }
}
