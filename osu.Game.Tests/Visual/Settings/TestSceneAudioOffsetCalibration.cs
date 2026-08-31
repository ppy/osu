// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Settings.Sections.Audio;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.UI;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Settings
{
    public partial class TestSceneAudioOffsetCalibration : OsuManualInputManagerTestScene
    {
        private readonly BindableDouble savedOffset = new BindableDouble(37) { MinValue = -500, MaxValue = 500, Precision = 1 };
        private DialogOverlay overlay = null!;
        private AudioOffsetCalibrationDialog dialog = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private AudioManager audio { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset offset and overlay", () =>
            {
                savedOffset.Disabled = false;
                savedOffset.Value = 37;
                config.SetValue(OsuSetting.MouseDisableButtons, false);
                Child = overlay = new DialogOverlay();
            });
        }

        [TestCase("osu")]
        [TestCase("taiko")]
        public void TestRemappedGameplayInput(string shortName)
        {
            int action = shortName == "osu" ? (int)OsuAction.LeftButton : (int)TaikoAction.LeftCentre;
            AddStep("set custom gameplay binding", () => realm.Write(r =>
            {
                r.RemoveRange(r.All<RealmKeyBinding>().Where(b => b.RulesetName == shortName && b.Variant == 0));
                r.Add(new RealmKeyBinding(action, new KeyCombination(InputKey.A)) { RulesetName = shortName, Variant = 0 });
            }));
            openDialog();
            AddStep("choose gameplay preview", () => dialog.PreviewRuleset.Value = rulesets.GetRuleset(shortName)!);
            AddUntilStep("reference playing", () => dialog.IsPlaying);
            AddStep("hover preview", () => InputManager.MoveMouseTo(dialog.ChildrenOfType<AudioOffsetCalibrationVisualisation>().Single()));
            AddStep("unbound keys", () =>
            {
                InputManager.Key(Key.Z);
                InputManager.Key(Key.F);
            });
            AddAssert("unbound keys ignored", () => dialog.TapEstimator.Count, () => Is.Zero);
            AddStep("custom gameplay key", () => InputManager.Key(Key.A));
            AddAssert("tap recorded", () => dialog.TapEstimator.Count, () => Is.EqualTo(1));
            AddAssert("tap visualised", () => dialog.ChildrenOfType<Drawable>().Any(d => d.Name == "Tap timing"));
            AddAssert("saved offset unchanged", () => savedOffset.Value, () => Is.EqualTo(37));
            AddStep("remove custom binding", () => realm.Write(r => r.RemoveRange(r.All<RealmKeyBinding>().Where(b => b.RulesetName == shortName && b.Variant == 0))));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestMouseButtons(bool disabled)
        {
            openDialog();
            AddStep("select osu and set mouse preference", () =>
            {
                dialog.PreviewRuleset.Value = rulesets.GetRuleset("osu")!;
                config.SetValue(OsuSetting.MouseDisableButtons, disabled);
            });
            AddUntilStep("reference playing", () => dialog.IsPlaying);
            AddStep("click preview", () =>
            {
                InputManager.MoveMouseTo(dialog.ChildrenOfType<AudioOffsetCalibrationVisualisation>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("mouse preference honoured", () => dialog.TapEstimator.Count, () => Is.EqualTo(disabled ? 0 : 1));
        }

        [TestCase(37, 60, -23)]
        [TestCase(490, -60, 500)]
        [TestCase(-490, 60, -500)]
        public void TestApplyTapEstimateAndReset(double initialOffset, double tapError, double expectedOffset)
        {
            openDialog();
            AddStep("supply eight taps", () =>
            {
                dialog.PreviewOffset.Value = initialOffset;
                for (int i = 0; i < 8; i++)
                    dialog.TapEstimator.AddTap(250 + i * 1000 + tapError, i * 1000, 1000, out _);
                dialog.ApplyTapEstimate();
            });
            AddAssert("preview adjusts within bounds", () => dialog.PreviewOffset.Value, () => Is.EqualTo(expectedOffset));
            AddAssert("not saved", () => savedOffset.Value, () => Is.EqualTo(37));
            AddAssert("old taps cleared", () => dialog.TapEstimator.Count, () => Is.Zero);
            AddStep("tap then change tempo", () =>
            {
                dialog.TapEstimator.AddTap(250, 0, 1000, out _);
                dialog.SlowerTempo.Value = false;
            });
            AddAssert("tempo clears stale taps", () => dialog.TapEstimator.Count, () => Is.Zero);
            AddUntilStep("reference still playing", () => dialog.IsPlaying);
            AddStep("dismiss", () => overlay.Hide());
            AddUntilStep("reference stops", () => !dialog.IsPlaying);
        }

        private void openDialog()
        {
            AddStep("open calibration", () => overlay.Push(dialog = new AudioOffsetCalibrationDialog(savedOffset)));
            AddUntilStep("dialog loaded", () => dialog.IsLoaded);
        }

        [TestCase("osu")]
        [TestCase("taiko")]
        public void TestGameplayCueFollowsOffset(string shortName)
        {
            AudioOffsetCalibrationBeatDisplay display = null!;
            var clock = new StopwatchClock();
            var offset = new BindableDouble();
            AddStep("create gameplay display", () =>
            {
                clock.Seek(150 - FramedBeatmapClock.GetPlatformOffset(audio.UseExperimentalWasapi.Value));
                Child = display = new AudioOffsetCalibrationBeatDisplay(clock, offset, 1000, rulesets.GetRuleset(shortName)!.CreateInstance());
            });
            AddUntilStep("note visible before beat", () => display.ChildrenOfType<DrawableHitObject>().SingleOrDefault()?.Parent?.Alpha, () => Is.EqualTo(1));
            AddStep("advance cue with offset", () => offset.Value = 150);
            AddUntilStep("note disappears on beat", () => display.ChildrenOfType<DrawableHitObject>().Single().Parent!.Alpha, () => Is.Zero);
            AddAssert("no judgement or hitsound", () => !display.ChildrenOfType<DrawableHitObject>().Single().Result.HasResult);
            AddStep("move cue back", () => offset.Value = -100);
            AddUntilStep("note visible again", () => display.ChildrenOfType<DrawableHitObject>().Single().Parent!.Alpha, () => Is.EqualTo(1));
            AddStep("loop reference", () => clock.Seek(clock.CurrentTime + 16000));
            AddUntilStep("same cue after loop", () => display.ChildrenOfType<DrawableHitObject>().Single().Parent!.Alpha, () => Is.EqualTo(1));
        }

        [Test]
        public void TestSave()
        {
            openDialog();
            AddAssert("starts with saved offset", () => dialog.PreviewOffset.Value, () => Is.EqualTo(37));
            AddStep("preview adjustment", () => dialog.PreviewOffset.Value = -23);
            AddAssert("saved offset unchanged", () => savedOffset.Value, () => Is.EqualTo(37));
            AddStep("save", () => dialog.PerformOkAction());
            AddUntilStep("dialog dismissed", () => overlay.CurrentDialog == null);
            AddAssert("offset saved", () => savedOffset.Value, () => Is.EqualTo(-23));
        }

        [Test]
        public void TestCancel([Values] bool useButton)
        {
            openDialog();
            AddStep("preview adjustment", () => dialog.PreviewOffset.Value = 99);
            AddStep("cancel", () =>
            {
                if (useButton)
                    dialog.PerformAction<PopupDialogCancelButton>();
                else
                    overlay.Hide();
            });
            AddUntilStep("dialog dismissed", () => overlay.CurrentDialog == null);
            AddAssert("saved offset unchanged", () => savedOffset.Value, () => Is.EqualTo(37));
        }

        [Test]
        public void TestDismissBeforeLoading()
        {
            AddStep("open and immediately dismiss", () =>
            {
                overlay.Push(dialog = new AudioOffsetCalibrationDialog(savedOffset));
                dialog.Hide();
            });
            AddWaitStep("wait past playback start", 5);
            AddAssert("saved offset unchanged", () => savedOffset.Value, () => Is.EqualTo(37));
            AddAssert("no dialog", () => overlay.CurrentDialog == null);
        }

        [Test]
        public void TestOffsetBoundsAndPrecision()
        {
            openDialog();
            AddStep("set above maximum", () => dialog.PreviewOffset.Value = 600);
            AddAssert("clamped to maximum", () => dialog.PreviewOffset.Value, () => Is.EqualTo(500));
            AddStep("set below minimum", () => dialog.PreviewOffset.Value = -600);
            AddAssert("clamped to minimum", () => dialog.PreviewOffset.Value, () => Is.EqualTo(-500));
            AddStep("set fractional offset", () => dialog.PreviewOffset.Value = 12.4);
            AddAssert("one millisecond precision", () => dialog.PreviewOffset.Value, () => Is.EqualTo(12));
            AddAssert("saved offset unchanged", () => savedOffset.Value, () => Is.EqualTo(37));
        }

        [Test]
        public void TestMusicRestoredOnDismissal()
        {
            openDialog();
            AddUntilStep("reference playing", () => dialog.IsPlaying);
            AddUntilStep("background music muted", () => beatmaps.BeatmapTrackStore.AggregateVolume.Value, () => Is.Zero);
            AddStep("dismiss", () => overlay.Hide());
            AddUntilStep("reference stopped", () => !dialog.IsPlaying);
            AddUntilStep("background music restored", () => beatmaps.BeatmapTrackStore.AggregateVolume.Value, () => Is.GreaterThan(0));
        }

        [Test]
        public void TestReplacedDialogStopsPlayback()
        {
            openDialog();
            AddUntilStep("reference playing", () => dialog.IsPlaying);
            AddStep("change preview", () => dialog.PreviewOffset.Value = -42);
            AddStep("replace dialog", () => overlay.Push(new ConfirmDialog("Another dialog", () => { })));
            AddUntilStep("reference stopped", () => !dialog.IsPlaying);
            AddUntilStep("background music restored", () => beatmaps.BeatmapTrackStore.AggregateVolume.Value, () => Is.GreaterThan(0));
            AddAssert("saved offset unchanged", () => savedOffset.Value, () => Is.EqualTo(37));
        }

        [Test]
        public void TestDismissDuringStartDelay()
        {
            AddStep("open and cancel before scheduled start", () =>
            {
                overlay.Push(dialog = new AudioOffsetCalibrationDialog(savedOffset));
                dialog.OnLoadComplete += _ => dialog.Hide();
            });
            AddWaitStep("wait past start delay", 5);
            AddAssert("reference never started", () => !dialog.IsPlaying);
            AddAssert("saved offset unchanged", () => savedOffset.Value, () => Is.EqualTo(37));
        }

        [Test]
        public void TestOffsetDirectionAndLoopSeam()
        {
            AudioOffsetCalibrationBeatDisplay display = null!;
            var offset = new BindableDouble();
            var clock = new StopwatchClock();
            AddStep("create beat display", () =>
            {
                clock.Seek(AudioOffsetCalibrationTrackStore.FIRST_BEAT - FramedBeatmapClock.GetPlatformOffset(audio.UseExperimentalWasapi.Value));
                Child = display = new AudioOffsetCalibrationBeatDisplay(clock, offset)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 0.8f,
                };
            });
            AddUntilStep("beat at centre", () => display.ChildrenOfType<CircularContainer>().Single(c => c.Name == "Beat marker").X, () => Is.EqualTo(0.5f).Within(0.001));
            AddStep("positive offset", () => offset.Value = 100);
            AddUntilStep("marker advances", () => display.ChildrenOfType<CircularContainer>().Single(c => c.Name == "Beat marker").X, () => Is.EqualTo(0.7f).Within(0.001));
            AddStep("negative offset", () => offset.Value = -100);
            AddUntilStep("marker moves back", () => display.ChildrenOfType<CircularContainer>().Single(c => c.Name == "Beat marker").X, () => Is.EqualTo(0.3f).Within(0.001));
            AddStep("advance a loop", () => clock.Seek(clock.CurrentTime + 8000));
            AddUntilStep("same phase after loop", () => display.ChildrenOfType<CircularContainer>().Single(c => c.Name == "Beat marker").X, () => Is.EqualTo(0.3f).Within(0.001));
        }
    }
}
