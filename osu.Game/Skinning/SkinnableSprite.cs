// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A skinnable element which uses a single texture backing.
    /// </summary>
    public partial class SkinnableSprite : SkinnableDrawable, ISerialisableDrawable
    {
        protected override bool ApplySizeRestrictionsToDefault => true;

        [Resolved]
        private TextureStore textures { get; set; } = null!;

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.SpriteName), nameof(SkinnableComponentStrings.SpriteNameDescription),
            SettingControlType = typeof(SpriteSelectorControl))]
        public Bindable<string> SpriteName { get; } = new Bindable<string>(string.Empty);

        [Resolved]
        private ISkinSource source { get; set; } = null!;

        public SkinnableSprite(string textureName, Vector2? maxSize = null, ConfineMode confineMode = ConfineMode.NoScaling)
            : base(new SpriteComponentLookup(textureName, maxSize), confineMode)
        {
            SpriteName.Value = textureName;
        }

        public SkinnableSprite()
            : base(new SpriteComponentLookup(string.Empty), ConfineMode.NoScaling)
        {
            RelativeSizeAxes = Axes.None;
            AutoSizeAxes = Axes.Both;

            SpriteName.BindValueChanged(name =>
            {
                ((SpriteComponentLookup)Lookup).LookupName = name.NewValue ?? string.Empty;
                if (IsLoaded)
                    SkinChanged(CurrentSkin);
            });
        }

        protected override Drawable CreateDefault(ISkinComponentLookup lookup)
        {
            var spriteLookup = (SpriteComponentLookup)lookup;
            var texture = textures.Get(spriteLookup.LookupName);

            if (texture == null)
                return new SpriteNotFound(spriteLookup.LookupName);

            if (spriteLookup.MaxSize != null)
                texture = texture.WithMaximumSize(spriteLookup.MaxSize.Value);

            return new Sprite { Texture = texture };
        }

        public bool UsesFixedAnchor { get; set; }

        public bool IsEditable => false;

        internal class SpriteComponentLookup : ISkinComponentLookup
        {
            public string LookupName { get; set; }
            public Vector2? MaxSize { get; set; }

            public SpriteComponentLookup(string textureName, Vector2? maxSize = null)
            {
                LookupName = textureName;
                MaxSize = maxSize;
            }

            bool IEquatable<ISkinComponentLookup>.Equals(ISkinComponentLookup? other)
                => other is SpriteComponentLookup lookup && LookupName == lookup.LookupName && MaxSize == lookup.MaxSize;

            object ISkinComponentLookup.Target => LookupName;

            RulesetInfo? ISkinComponentLookup.Ruleset => null;
        }

        public partial class SpriteSelectorControl : SettingsDropdown<string>
        {
            protected override void LoadComplete()
            {
                base.LoadComplete();

                // Round-about way of getting the user's skin to find available resources.
                // In the future we'll probably want to allow access to resources from the fallbacks, or potentially other skins
                // but that requires further thought.
                var highestPrioritySkin = getHighestPriorityUserSkin(((SkinnableSprite)SettingSourceObject).source.AllSources) as Skin;

                string[]? availableFiles = highestPrioritySkin?.SkinInfo.PerformRead(s => s.Files
                                                                                           .Where(f => f.Filename.EndsWith(".png", StringComparison.Ordinal)
                                                                                                       || f.Filename.EndsWith(".jpg", StringComparison.Ordinal))
                                                                                           .Select(f => f.Filename).Distinct()).ToArray();

                if (availableFiles?.Length > 0)
                    Items = availableFiles;

                static ISkin? getHighestPriorityUserSkin(IEnumerable<ISkin> skins)
                {
                    foreach (var skin in skins)
                    {
                        if (skin is ISkinTransformer transformer && isUserSkin(transformer.Skin))
                            return transformer.Skin;

                        if (isUserSkin(skin))
                            return skin;
                    }

                    return null;
                }

                // Temporarily used to exclude undesirable ISkin implementations
                static bool isUserSkin(ISkin skin)
                    => skin.GetType() == typeof(TrianglesSkin)
                       || skin.GetType() == typeof(ArgonProSkin)
                       || skin.GetType() == typeof(ArgonSkin)
                       || skin.GetType() == typeof(DefaultLegacySkin)
                       || skin.GetType() == typeof(LegacySkin);
            }
        }

        public partial class SpriteNotFound : CompositeDrawable
        {
            public SpriteNotFound(string lookup)
            {
                AutoSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Size = new Vector2(50),
                        Icon = FontAwesome.Solid.QuestionCircle
                    },
                    new OsuSpriteText
                    {
                        Position = new Vector2(25, 50),
                        Text = $"missing: {lookup}",
                        Origin = Anchor.TopCentre,
                    }
                };
            }
        }
    }
}
