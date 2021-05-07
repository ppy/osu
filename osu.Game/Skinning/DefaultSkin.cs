// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Screens.Play.HUD;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class DefaultSkin : Skin
    {
        public DefaultSkin()
            : base(SkinInfo.Default)
        {
            Configuration = new DefaultSkinConfiguration();
        }

        public override Drawable GetDrawableComponent(ISkinComponent component)
        {
            switch (component)
            {
                case SkinnableTargetComponent target:
                    switch (target.Target)
                    {
                        case SkinnableTarget.MainHUDComponents:
                            var infos = JsonConvert.DeserializeObject<IEnumerable<SkinnableInfo>>(File.ReadAllText("/Users/Dean/json-out.json")).Where(i => i.Target == SkinnableTarget.MainHUDComponents);

                            var container = new SkinnableTargetWrapper(target.Target)
                            {
                                ChildrenEnumerable = infos.Select(i => i.CreateInstance())
                            };

                            return container;
                    }

                    break;
            }

            return null;
        }

        public override Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => null;

        public override ISample GetSample(ISampleInfo sampleInfo) => null;

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

    public class SkinnableTargetWrapper : Container, ISkinnableTarget
    {
        public SkinnableTarget Target { get; }

        public SkinnableTargetWrapper(SkinnableTarget target)
        {
            Target = target;
        }
    }
}
