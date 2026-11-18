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

        [SettingSource("Flipped axes")]
        public Bindable<MirrorType> Reflection { get; } = new Bindable<MirrorType>();

        [SettingSource("Reflect stack direction")]
        public BindableBool MirrorStacks { get; } = new BindableBool();

        public bool MirrorStackingDirection => MirrorStacks.Value;
        public override bool Ranked => MirrorStackingDirection;

        public OsuModMirror()
        {
            MirrorStacks.Value = MirrorStacks.Default = false;
        }

        public void ApplyToHitObject(HitObject hitObject)
        {
            var osuObject = (OsuHitObject)hitObject;

            switch (Reflection.Value)
            {
                case MirrorType.Horizontal:
                    OsuHitObjectGenerationUtils.ReflectHorizontallyAlongPlayfield(osuObject, MirrorStackingDirection);
                    break;

                case MirrorType.Vertical:
                    OsuHitObjectGenerationUtils.ReflectVerticallyAlongPlayfield(osuObject, MirrorStackingDirection);
                    break;

                case MirrorType.Both:
                    OsuHitObjectGenerationUtils.ReflectHorizontallyAlongPlayfield(osuObject, MirrorStackingDirection);
                    OsuHitObjectGenerationUtils.ReflectVerticallyAlongPlayfield(osuObject, MirrorStackingDirection);
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
