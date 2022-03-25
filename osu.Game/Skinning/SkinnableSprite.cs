// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A skinnable element which uses a stable sprite and can therefore share implementation logic.
    /// </summary>
    public class SkinnableSprite : SkinnableDrawable, ISkinnableDrawable
    {
        protected override bool ApplySizeRestrictionsToDefault => true;

        [Resolved]
        private TextureStore textures { get; set; }

        [SettingSource("Sprite name", "The filename of the sprite", SettingControlType = typeof(SpriteSelectorControl))]
        public Bindable<string> SpriteName { get; } = new Bindable<string>(string.Empty);

        [Resolved]
        private ISkinSource source { get; set; }

        public IEnumerable<string> AvailableFiles => (source.AllSources.First() as Skin)?.SkinInfo.PerformRead(s => s.Files
                                                                                                                     .Where(f =>
                                                                                                                         f.Filename.EndsWith(".png", StringComparison.Ordinal)
                                                                                                                         || f.Filename.EndsWith(".jpg", StringComparison.Ordinal)
                                                                                                                     )
                                                                                                                     .Select(f => f.Filename).Distinct());

        public SkinnableSprite(string textureName, ConfineMode confineMode = ConfineMode.NoScaling)
            : base(new SpriteComponent(textureName), confineMode)
        {
            SpriteName.Value = textureName;
        }

        public SkinnableSprite()
            : base(new SpriteComponent(string.Empty), ConfineMode.NoScaling)
        {
            RelativeSizeAxes = Axes.None;
            AutoSizeAxes = Axes.Both;

            SpriteName.BindValueChanged(name =>
            {
                ((SpriteComponent)Component).LookupName = name.NewValue ?? string.Empty;
                if (IsLoaded)
                    SkinChanged(CurrentSkin);
            });
        }

        protected override Drawable CreateDefault(ISkinComponent component)
        {
            var texture = textures.Get(component.LookupName);

            if (texture == null)
                return null;

            return new Sprite { Texture = texture };
        }

        public bool UsesFixedAnchor { get; set; }

        private class SpriteComponent : ISkinComponent
        {
            public string LookupName { get; set; }

            public SpriteComponent(string textureName)
            {
                LookupName = textureName;
            }
        }

        public class SpriteSelectorControl : SettingsDropdown<string>
        {
            public SkinnableSprite Source { get; set; }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Items = Source.AvailableFiles;
            }
        }
    }
}
