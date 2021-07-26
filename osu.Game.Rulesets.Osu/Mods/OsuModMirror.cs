// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Utils;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModMirror : ModMirror, IApplicableToHitObject
    {
        public override string Description => "Reflect the playfield.";

        [SettingSource("Reflect Horizontally", "Reflect the playfield horizontally.")]
        public Bindable<bool> ReflectY { get; } = new BindableBool(true);
        [SettingSource("Reflect Vertically", "Reflect the playfield vertically.")]
        public Bindable<bool> ReflectX { get; } = new BindableBool(false);

        public void ApplyToHitObject(HitObject hitObject)
        {
            if (!(ReflectY.Value || ReflectX.Value))
                return; // TODO deselect the mod if possible so replays and submissions don't have purposeless mods attached.
            var osuObject = (OsuHitObject)hitObject;
            if (ReflectY.Value)
                OsuHitObjectGenerationUtils.ReflectOsuHitObjectHorizontally(osuObject);
            if (ReflectX.Value)
                OsuHitObjectGenerationUtils.ReflectOsuHitObjectVertically(osuObject);
        }
    }
}
