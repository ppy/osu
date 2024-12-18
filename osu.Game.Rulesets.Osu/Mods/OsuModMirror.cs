// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModMirror : ModMirror, IApplicableToHitObject
    {
        public override LocalisableString Description => "Flip objects on the chosen axes.";
        public override Type[] IncompatibleMods => new[] { typeof(ModHardRock) };

        [SettingSource("Mirrored axes", "Choose which axes objects are mirrored over.")]
        public Bindable<MirrorType> Reflection { get; } = new Bindable<MirrorType>();

        public void ApplyToHitObject(HitObject hitObject)
        {
            var osuObject = (OsuHitObject)hitObject;

            switch (Reflection.Value)
            {
                case MirrorType.Horizontal:
                    OsuHitObjectGenerationUtils.ReflectHorizontallyAlongPlayfield(osuObject);
                    break;

                case MirrorType.Vertical:
                    OsuHitObjectGenerationUtils.ReflectVerticallyAlongPlayfield(osuObject);
                    break;

                case MirrorType.Both:
                    OsuHitObjectGenerationUtils.ReflectHorizontallyAlongPlayfield(osuObject);
                    OsuHitObjectGenerationUtils.ReflectVerticallyAlongPlayfield(osuObject);
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
