// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseFacadeContainer : ScreenTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(PlayerLoader),
            typeof(Player),
            typeof(Facade),
            typeof(FacadeContainer),
            typeof(ButtonSystem),
            typeof(ButtonSystemState),
            typeof(Menu),
            typeof(MainMenu)
        };

        [Cached]
        private OsuLogo logo;

        private readonly Bindable<float> uiScale = new Bindable<float>();

        public TestCaseFacadeContainer()
        {
            Add(logo = new OsuLogo());
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.UIScale, uiScale);
            AddSliderStep("Adjust scale", 1f, 1.5f, 1f, v => uiScale.Value = v);
        }

        [Test]
        public void IsolatedTest()
        {
            bool randomPositions = false;
            AddToggleStep("Toggle move continuously", b => randomPositions = b);
            AddStep("Move facade to random position", () => LoadScreen(new TestScreen(randomPositions)));
        }

        [Test]
        public void PlayerLoaderTest()
        {
            AddToggleStep("Toggle mods", b => { Beatmap.Value.Mods.Value = b ? Beatmap.Value.Mods.Value.Concat(new[] { new OsuModNoFail() }) : Enumerable.Empty<Mod>(); });
            AddStep("Add new playerloader", () => LoadScreen(new TestPlayerLoader(() => new TestPlayer
            {
                AllowPause = false,
                AllowLeadIn = false,
                AllowResults = false,
                Ready = false
            })));
        }

        private class TestFacadeContainer : FacadeContainer
        {
            protected override Facade CreateFacade() => new Facade
            {
                Colour = Color4.Tomato,
                Alpha = 0.35f,
                Child = new Box
                {
                    Colour = Color4.Tomato,
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        private class TestScreen : OsuScreen
        {
            private TestFacadeContainer facadeContainer;
            private FacadeFlowComponent facadeFlowComponent;
            private readonly bool randomPositions;

            public TestScreen(bool randomPositions = false)
            {
                this.randomPositions = randomPositions;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = facadeContainer = new TestFacadeContainer
                {
                    Child = facadeFlowComponent = new FacadeFlowComponent
                    {
                        AutoSizeAxes = Axes.Both
                    }
                };
            }

            protected override void LogoArriving(OsuLogo logo, bool resuming)
            {
                base.LogoArriving(logo, resuming);
                logo.FadeIn(350);
                logo.ScaleTo(new Vector2(0.15f), 350, Easing.In);
                facadeContainer.SetLogo(logo, 0.3f, 1000, Easing.InOutQuint);
                facadeContainer.Tracking = true;
                moveLogoFacade();
            }

            protected override void LogoExiting(OsuLogo logo)
            {
                base.LogoExiting(logo);
                facadeContainer.Tracking = false;
            }

            private void moveLogoFacade()
            {
                Random random = new Random();
                if (facadeFlowComponent.Transforms.Count == 0)
                {
                    facadeFlowComponent.Delay(500).MoveTo(new Vector2(random.Next(0, (int)DrawWidth), random.Next(0, (int)DrawHeight)), 300);
                }

                if (randomPositions)
                    Schedule(moveLogoFacade);
            }
        }

        private class FacadeFlowComponent : FillFlowContainer
        {
            [BackgroundDependencyLoader]
            private void load(Facade facade)
            {
                facade.Anchor = Anchor.TopCentre;
                facade.Origin = Anchor.TopCentre;
                Child = facade;
            }
        }

        private class TestPlayerLoader : PlayerLoader
        {
            public TestPlayerLoader(Func<Player> player)
                : base(player)
            {
            }

            protected override FacadeContainer CreateFacadeContainer() => new TestFacadeContainer();
        }

        private class TestPlayer : Player
        {
            public bool Ready;

            [BackgroundDependencyLoader]
            private void load()
            {
                // Never finish loading
                while (!Ready)
                    Thread.Sleep(1);
            }
        }
    }
}
