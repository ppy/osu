// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModMirror : ModMirror, IApplicableToHitObject
    {
        public override string Description => "在所选方向上旋转物件。";
        public override Type[] IncompatibleMods => new[] { typeof(ModHardRock) };

        [SettingSource("旋转方向", "选择物件要旋转的方向。")]
        public Bindable<MirrorType> Reflection { get; } = new Bindable<MirrorType>();

        public void ApplyToHitObject(HitObject hitObject)
        {
            var osuObject = (OsuHitObject)hitObject;

            switch (Reflection.Value)
            {
                case MirrorType.Horizontal:
                    OsuHitObjectGenerationUtils.ReflectHorizontally(osuObject);
                    break;

                case MirrorType.Vertical:
                    OsuHitObjectGenerationUtils.ReflectVertically(osuObject);
                    break;

                case MirrorType.Both:
                    OsuHitObjectGenerationUtils.ReflectHorizontally(osuObject);
                    OsuHitObjectGenerationUtils.ReflectVertically(osuObject);
                    break;
            }
        }

        public enum MirrorType
        {
            Horizontal,
            Vertical,
            Both
        }
    }
}
