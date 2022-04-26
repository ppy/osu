// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Testing.Input;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneGameplayCursor : OsuSkinnableTestScene
    {
        [Cached]
        private GameplayState gameplayState;

        private OsuCursorContainer lastContainer;

        [Resolved]
        private OsuConfigManager config { get; set; }

        private Drawable background;

        public TestSceneGameplayCursor()
        {
            var ruleset = new OsuRuleset();
            gameplayState = new GameplayState(CreateBeatmap(ruleset.RulesetInfo), ruleset, Array.Empty<Mod>());

            AddStep("change background colour", () =>
            {
                background?.Expire();

                Add(background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                    Colour = new Colour4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1)
                });
            });

            AddSliderStep("circle size", 0f, 10f, 0f, val =>
            {
                config.SetValue(OsuSetting.AutoCursorSize, true);
                gameplayState.Beatmap.Difficulty.CircleSize = val;
                Scheduler.AddOnce(loadContent);
            });

            AddStep("test cursor container", () => loadContent(false));
        }

        [TestCase(1, 1)]
        [TestCase(5, 1)]
        [TestCase(10, 1)]
        [TestCase(1, 1.5f)]
        [TestCase(5, 1.5f)]
        [TestCase(10, 1.5f)]
        public void TestSizing(int circleSize, float userScale)
        {
            AddStep($"set user scale to {userScale}", () => config.SetValue(OsuSetting.GameplayCursorSize, userScale));
            AddStep($"adjust cs to {circleSize}", () => gameplayState.Beatmap.Difficulty.CircleSize = circleSize);
            AddStep("turn on autosizing", () => config.SetValue(OsuSetting.AutoCursorSize, true));

            AddStep("load content", loadContent);

            AddUntilStep("cursor size correct", () => lastContainer.ActiveCursor.Scale.X == OsuCursorContainer.GetScaleForCircleSize(circleSize) * userScale);

            AddStep("set user scale to 1", () => config.SetValue(OsuSetting.GameplayCursorSize, 1f));
            AddUntilStep("cursor size correct", () => lastContainer.ActiveCursor.Scale.X == OsuCursorContainer.GetScaleForCircleSize(circleSize));

            AddStep("turn off autosizing", () => config.SetValue(OsuSetting.AutoCursorSize, false));
            AddUntilStep("cursor size correct", () => lastContainer.ActiveCursor.Scale.X == 1);

            AddStep($"set user scale to {userScale}", () => config.SetValue(OsuSetting.GameplayCursorSize, userScale));
            AddUntilStep("cursor size correct", () => lastContainer.ActiveCursor.Scale.X == userScale);
        }

        [Test]
        public void TestTopLeftOrigin()
        {
            AddStep("load content", () => loadContent(false, () => new SkinProvidingContainer(new TopLeftCursorSkin())));
        }

        private void loadContent() => loadContent(false);

        private void loadContent(bool automated, Func<SkinProvidingContainer> skinProvider = null)
        {
            SetContents(_ =>
            {
                var inputManager = automated ? (InputManager)new MovingCursorInputManager() : new OsuInputManager(new OsuRuleset().RulesetInfo);
                var skinContainer = skinProvider?.Invoke() ?? new SkinProvidingContainer(null);

                lastContainer = automated ? new ClickingCursorContainer() : new OsuCursorContainer();

                return inputManager.WithChild(skinContainer.WithChild(lastContainer));
            });
        }

        private class TopLeftCursorSkin : ISkin
        {
            public Drawable GetDrawableComponent(ISkinComponent component) => null;
            public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => null;
            public ISample GetSample(ISampleInfo sampleInfo) => null;

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
            {
                switch (lookup)
                {
                    case OsuSkinConfiguration osuLookup:
                        if (osuLookup == OsuSkinConfiguration.CursorCentre)
                            return SkinUtils.As<TValue>(new BindableBool());

                        break;
                }

                return null;
            }
        }

        private class ClickingCursorContainer : OsuCursorContainer
        {
            private bool pressed;

            public bool Pressed
            {
                set
                {
                    if (value == pressed)
                        return;

                    pressed = value;
                    if (value)
                        OnPressed(new KeyBindingPressEvent<OsuAction>(GetContainingInputManager().CurrentState, OsuAction.LeftButton));
                    else
                        OnReleased(new KeyBindingReleaseEvent<OsuAction>(GetContainingInputManager().CurrentState, OsuAction.LeftButton));
                }
            }

            protected override void Update()
            {
                base.Update();
                Pressed = ((int)(Time.Current / 1000)) % 2 == 0;
            }
        }

        private class MovingCursorInputManager : ManualInputManager
        {
            public MovingCursorInputManager()
            {
                UseParentInput = false;
                ShowVisualCursorGuide = false;
            }

            protected override void Update()
            {
                base.Update();

                const double spin_duration = 5000;
                double currentTime = Time.Current;

                double angle = (currentTime % spin_duration) / spin_duration * 2 * Math.PI;
                Vector2 rPos = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                MoveMouseTo(ToScreenSpace(DrawSize / 2 + DrawSize / 3 * rPos));
            }
        }
    }
}
