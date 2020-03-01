// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public abstract class TestSceneModSandbox : PlayerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(TestSceneModSandbox)
        };

        protected Mod Mod;
        private readonly TriangleButton button;

        protected TestSceneModSandbox(Ruleset ruleset, Mod mod = null)
            : base(ruleset)
        {
            Mod = mod ?? new SandboxMod();

            var props = Mod.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
            var hasSettings = props.Any(prop => prop.GetCustomAttribute<SettingSourceAttribute>(true) != null);

            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding(50),
                Margin = new MarginPadding { Bottom = 20 },
                Width = 0.4f,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Children = new Drawable[]
                {
                    new ModControlSection(Mod, Mod.CreateSettingsControls()),
                    button = new TriangleButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Width = 0.5f,
                        Text = "Start",
                        Action = () =>
                        {
                            button.Text = hasSettings ? "Apply Settings" : "Restart";
                            LoadPlayer();
                        }
                    }
                }
            };
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LocalConfig.GetBindable<bool>(OsuSetting.KeyOverlay).Value = true;
        }

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = SelectedMods.Value.Append(Mod).ToArray();

            return base.CreatePlayer(ruleset);
        }

        protected class SandboxMod : Mod
        {
            public override string Name => "Sandbox Test";
            public override string Acronym => "ST";
            public override double ScoreMultiplier => 1.0;

            [SettingSource("Test Setting")]
            public Bindable<bool> TestSetting1 { get; } = new BindableBool
            {
                Default = true,
                Value = true
            };

            [SettingSource("Test Setting 2")]
            public Bindable<float> TestSetting2 { get; } = new BindableFloat
            {
                Precision = 0.1f,
                MinValue = 0,
                MaxValue = 20
            };
        }
    }
}
