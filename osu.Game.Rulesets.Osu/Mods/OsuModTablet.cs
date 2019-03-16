// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Drawing;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Input.StateChanges;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTablet : Mod, IUpdatableByPlayfield, IApplicableToRulesetContainer<OsuHitObject>, IReadFromConfig
    {
        public override string Name => "Tablet";
        public override string Acronym => "T";
        public override string Description => "Tablet";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override ModType Type => ModType.DifficultyReduction;
        public override Type[] IncompatibleMods => new[] { typeof(ModHardRock) };

        Bindable<WindowMode> windowMode;
        Bindable<Size> sizeFullscreen;
        Bindable<Size> windowedSize;

        public void Update(Playfield playfield)
        {
            Size size;

            if (windowMode.Value == WindowMode.Windowed)
                size = windowedSize.Value;
            else
                size = sizeFullscreen.Value;

            Vector2 max = new Vector2(size.Width * 2 / 10, size.Height * 2 / 10);
            Vector2 pos = new Vector2(size.Width * 7 / 10, size.Height * 7 / 10);

            float x = osuInputManager.CurrentState.Mouse.Position.X;
            float y = osuInputManager.CurrentState.Mouse.Position.Y;

            if (x < pos.X || x > pos.X + max.X || y < pos.Y || y > pos.Y + max.Y)
                return;

            x = (x - pos.X) * size.Width / max.X - x;
            y = (y - pos.Y) * size.Height / max.Y - y;
            
            var state = new MousePositionRelativeInput
            {
                Delta = new Vector2(x, y)
            };

            state.Apply(osuInputManager.CurrentState, osuInputManager);
        }

        private OsuInputManager osuInputManager;

        public void ApplyToRulesetContainer(RulesetContainer<OsuHitObject> rulesetContainer)
        {
            // grab the input manager for future use.
            osuInputManager = (OsuInputManager)rulesetContainer.KeyBindingInputManager;
        }

        public void ReadFromConfig(FrameworkConfigManager config)
        {
            windowMode = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode);
            sizeFullscreen = config.GetBindable<Size>(FrameworkSetting.SizeFullscreen);
            windowedSize = config.GetBindable<Size>(FrameworkSetting.WindowedSize);
        }
    }
}
