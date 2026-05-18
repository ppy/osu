// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFirstPerson : Mod, IApplicableToPlayer, IUpdatableByPlayfield
    {
        public override string Name => "First Person";
        public override string Acronym => "FP";
        public override LocalisableString Description => "Catch, from the catcher's perspective!";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => OsuIcon.ModMovingFast;
        public override Type[] IncompatibleMods => new[] { typeof(ModCinema), typeof(ModNoScope) };

        [SettingSource("Centred background", "Have the background follow.")] // From the perspective of the player
        public BindableBool CentredBackground { get; } = new BindableBool();

        [SettingSource("Centred storyboard / video", "Have the storyboard / video follow.")] // From the perspective of the player
        public BindableBool CentredStoryboard { get; } = new BindableBool();

        private DimmableStoryboard dimmableStoryboard = null!;
        private Action<float> setBackgroundX = null!;

        private const float shift_x_factor = 1.6f; // Bruteforced, is magical, todo: may need more intricate calculation

        public void ApplyToPlayer(Player player)
        {
            dimmableStoryboard = player.DimmableStoryboard;
            setBackgroundX = x => player.ApplyToBackground(b => b.X = x);
        }

        public void Update(Playfield playfield)
        {
            var catchPlayfield = (CatchPlayfield)playfield;

            catchPlayfield.X = CatchPlayfield.CENTER_X - catchPlayfield.Catcher.X;
            float miscellaneousXOffset = shift_x_factor * catchPlayfield.X;

            if (!CentredBackground.Value)
                setBackgroundX(miscellaneousXOffset); // TODO: Fix crashing when exiting replay (from Auto) and the leaving background at the same X when exiting until there is a background refresh
            if (!CentredStoryboard.Value)
                dimmableStoryboard.Children.Where(c => c is DrawableStoryboard).ForEach(ds => ds.X = miscellaneousXOffset);
        }
    }
}
