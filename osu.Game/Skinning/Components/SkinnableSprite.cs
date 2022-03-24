// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;

namespace osu.Game.Skinning.Components
{
    /// <summary>
    /// Intended to be a test bed for skinning. May be removed at some point in the future.
    /// </summary>
    [UsedImplicitly]
    public class SkinSprite : CompositeDrawable, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Sprite name", "The filename of the sprite", SettingControlType = typeof(SpriteSelectorControl))]
        public Bindable<string> SpriteName { get; } = new Bindable<string>(string.Empty);

        [Resolved]
        private ISkinSource source { get; set; }

        public IEnumerable<string> AvailableFiles => (source.AllSources.First() as Skin)?.SkinInfo.PerformRead(s => s.Files.Select(f => f.Filename));

        public SkinSprite()
        {
            AutoSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SpriteName.BindValueChanged(spriteName =>
            {
                InternalChildren = new Drawable[]
                {
                    new Sprite
                    {
                        Texture = source.GetTexture(SpriteName.Value),
                    }
                };
            }, true);
        }

        public class SpriteSelectorControl : SettingsDropdown<string>
        {
            public SkinSprite Source { get; set; }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Items = Source.AvailableFiles;
            }
        }
    }
}
