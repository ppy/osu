// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Configuration;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseBackgroundScreenBeatmap : ScreenTestCase
    {
        private DummySongSelect songSelect;
        protected Player Player;
        public TestCaseBackgroundScreenBeatmap()
        {
            AddStep("Load Song Select", () =>
            {
                LoadComponentAsync(new DummySongSelect(), p =>
                {
                    songSelect = p;
                    LoadScreen(p);
                });
            });
            AddUntilStep(() => songSelect?.IsLoaded ?? false, "Wait for song select to load");
            AddStep("Create beatmap", () =>
            {
                Beatmap.Value = new TestWorkingBeatmap(new Beatmap<OsuHitObject>
                {
                    HitObjects =
                    {
                        new HitCircle
                        {
                            StartTime = 3000,
                            Position = new Vector2(0, 0),
                        },
                        new HitCircle
                        {
                            StartTime = 15000,
                            Position = new Vector2(0, 0),
                        }
                    },
                });
            });
            AddStep("Load Player", () =>
            {
                var p = new DimAccessiblePlayer();
                songSelect.Push(Player = p);
            });

            AddUntilStep(() => Player?.IsLoaded ?? false, "Wait for player to load");
            AddStep("Update bindables", () => ((DimAccessiblePlayer)Player).UpdateBindables());
        }

        /// <summary>
        /// Check if the fade container is properly being reset when screen dim is disabled.
        /// </summary>
        [Test]
        public void DisableUserDimTest()
        {
            AddStep("Test User Undimming", () => ((DimAccessiblePlayer)Player).DimEnabled.Value = false);
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is undimmed", () => ((DimAccessiblePlayer)Player).AssertUndimmed());
        }

        /// <summary>
        /// Check if the fade container is properly being faded when screen dim is enabled.
        /// </summary>
        [Test]
        public void EnableUserDimTest()
        {
            AddStep("Test User Dimming", () => ((DimAccessiblePlayer)Player).DimEnabled.Value = true);
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is dimmed", () => ((DimAccessiblePlayer)Player).AssertDimmed());
        }

        /// <summary>
        /// Check if the fade container retains dim when pausing
        /// </summary>
        [Test]
        public void PauseTest()
        {
            AddStep("Transition to Pause", () => ((DimAccessiblePlayer)Player).Exit());
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is dimmed", () => ((DimAccessiblePlayer)Player).AssertDimmed());
        }

        /// <summary>
        /// Check if the fade container removes user dim when suspending player for results
        /// </summary>
        [Test]
        public void TransitionTest()
        {
            AddStep("Transition to Results", () => Player.Push(new FadeAccesibleResults(new ScoreInfo { User = new User { Username = "osu!" }})));
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is undimmed", () => ((DimAccessiblePlayer)Player).AssertUndimmed());
        }

        /// <summary>
        /// Check if background gets undimmed when leaving the player for the previous screen
        /// </summary>
        [Test]
        public void TransitionOutTest()
        {
            AddStep("Exit player", () =>
            {
                Player.MakeCurrent();
                Player.Exit();
            });
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is undimmed", () => ((DimAccessiblePlayer)Player).AssertUndimmed());
        }

        private class DummySongSelect : OsuScreen
        {
            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground();
        }

        private class FadeAccesibleResults : SoloResults
        {
            public FadeAccesibleResults(ScoreInfo score) : base(score)
            {
            }

            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground();
        }

        private class DimAccessiblePlayer : Player
        {
            public Bindable<bool> DimEnabled;

            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground();

            public void UpdateBindables()
            {
                DimEnabled = Background.UpdateDim;
            }

            public bool AssertDimmed()
            {
                return ((FadeAccessibleBackground)Background).AssertDimmed();
            }

            public bool AssertUndimmed()
            {
                return ((FadeAccessibleBackground)Background).AssertUndimmed();
            }
        }

        private class FadeAccessibleBackground : BackgroundScreenBeatmap
        {
            public bool AssertDimmed()
            {
                return FadeContainer.Colour == OsuColour.Gray(1 - (float)DimLevel);
            }

            public bool AssertUndimmed()
            {
                return FadeContainer.Colour == Color4.White;
            }
        }
    }
}
