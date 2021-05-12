// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class DefaultSkin : Skin
    {
        public DefaultSkin(IStorageResourceProvider resources)
            : this(SkinInfo.Default, resources)
        {
        }

        public DefaultSkin(SkinInfo skin, IStorageResourceProvider resources)
            : base(skin, resources)
        {
            Configuration = new DefaultSkinConfiguration();
        }

        public override Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => null;

        public override ISample GetSample(ISampleInfo sampleInfo) => null;

        public override Drawable GetDrawableComponent(ISkinComponent component)
        {
            if (base.GetDrawableComponent(component) is Drawable c)
                return c;

            switch (component)
            {
                case SkinnableTargetComponent target:
                    switch (target.Target)
                    {
                        case SkinnableTarget.MainHUDComponents:
                            var skinnableTargetWrapper = new SkinnableTargetWrapper(container =>
                            {
                                var score = container.OfType<DefaultScoreCounter>().FirstOrDefault();
                                var accuracy = container.OfType<DefaultAccuracyCounter>().FirstOrDefault();
                                var combo = container.OfType<DefaultComboCounter>().FirstOrDefault();

                                if (score != null)
                                {
                                    score.Anchor = Anchor.TopCentre;
                                    score.Origin = Anchor.TopCentre;

                                    // elements default to beneath the health bar
                                    const float vertical_offset = 30;

                                    const float horizontal_padding = 20;

                                    score.Position = new Vector2(0, vertical_offset);

                                    if (accuracy != null)
                                    {
                                        accuracy.Position = new Vector2(-accuracy.ScreenSpaceDeltaToParentSpace(score.ScreenSpaceDrawQuad.Size).X / 2 - horizontal_padding, vertical_offset + 5);
                                        accuracy.Origin = Anchor.TopRight;
                                        accuracy.Anchor = Anchor.TopCentre;
                                    }

                                    if (combo != null)
                                    {
                                        combo.Position = new Vector2(accuracy.ScreenSpaceDeltaToParentSpace(score.ScreenSpaceDrawQuad.Size).X / 2 + horizontal_padding, vertical_offset + 5);
                                        combo.Anchor = Anchor.TopCentre;
                                    }
                                }
                            })
                            {
                                Children = new[]
                                {
                                    GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.ComboCounter)),
                                    GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.ScoreCounter)),
                                    GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.AccuracyCounter)),
                                    GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.HealthDisplay)),
                                }
                            };

                            return skinnableTargetWrapper;
                    }

                    return null;

                case HUDSkinComponent hudComponent:
                {
                    switch (hudComponent.Component)
                    {
                        case HUDSkinComponents.ComboCounter:
                            return new DefaultComboCounter();

                        case HUDSkinComponents.ScoreCounter:
                            return new DefaultScoreCounter();

                        case HUDSkinComponents.AccuracyCounter:
                            return new DefaultAccuracyCounter();

                        case HUDSkinComponents.HealthDisplay:
                            return new DefaultHealthDisplay();
                    }

                    return null;
                }
            }

            return base.GetDrawableComponent(component);
        }

        public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            switch (lookup)
            {
                // todo: this code is pulled from LegacySkin and should not exist.
                // will likely change based on how databased storage of skin configuration goes.
                case GlobalSkinColours global:
                    switch (global)
                    {
                        case GlobalSkinColours.ComboColours:
                            return SkinUtils.As<TValue>(new Bindable<IReadOnlyList<Color4>>(Configuration.ComboColours));
                    }

                    break;
            }

            return null;
        }
    }
}
