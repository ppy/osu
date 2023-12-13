// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Text;

namespace osu.Game.Graphics
{
    public static class HexaconsIcons
    {
        public const string FONT_NAME = "Icons/Hexacons";

        public static IconUsage BeatmapPacks => get(HexaconsMapping.beatmap_packs);
        public static IconUsage Beatmap => get(HexaconsMapping.beatmap);
        public static IconUsage Calendar => get(HexaconsMapping.calendar);
        public static IconUsage Chart => get(HexaconsMapping.chart);
        public static IconUsage Community => get(HexaconsMapping.community);
        public static IconUsage Contests => get(HexaconsMapping.contests);
        public static IconUsage Devtools => get(HexaconsMapping.devtools);
        public static IconUsage Download => get(HexaconsMapping.download);
        public static IconUsage Editor => get(HexaconsMapping.editor);
        public static IconUsage FeaturedArtist => get(HexaconsMapping.featured_artist);
        public static IconUsage Home => get(HexaconsMapping.home);
        public static IconUsage Messaging => get(HexaconsMapping.messaging);
        public static IconUsage Music => get(HexaconsMapping.music);
        public static IconUsage News => get(HexaconsMapping.news);
        public static IconUsage Notification => get(HexaconsMapping.notification);
        public static IconUsage Profile => get(HexaconsMapping.profile);
        public static IconUsage Rankings => get(HexaconsMapping.rankings);
        public static IconUsage Search => get(HexaconsMapping.search);
        public static IconUsage Settings => get(HexaconsMapping.settings);
        public static IconUsage Social => get(HexaconsMapping.social);
        public static IconUsage Store => get(HexaconsMapping.store);
        public static IconUsage Tournament => get(HexaconsMapping.tournament);
        public static IconUsage Wiki => get(HexaconsMapping.wiki);

        private static IconUsage get(HexaconsMapping icon) => new IconUsage((char)icon, FONT_NAME);

        // Basically just converting to something we can use in a `char` lookup for FontStore/GlyphStore compatibility.
        // Names should match filenames in resources.
        private enum HexaconsMapping
        {
            beatmap_packs,
            beatmap,
            calendar,
            chart,
            community,
            contests,
            devtools,
            download,
            editor,
            featured_artist,
            home,
            messaging,
            music,
            news,
            notification,
            profile,
            rankings,
            search,
            settings,
            social,
            store,
            tournament,
            wiki,
        }

        public class HexaconsStore : ITextureStore, ITexturedGlyphLookupStore
        {
            private readonly TextureStore textures;

            public HexaconsStore(TextureStore textures)
            {
                this.textures = textures;
            }

            public void Dispose()
            {
                textures.Dispose();
            }

            public ITexturedCharacterGlyph? Get(string? fontName, char character)
            {
                if (fontName == FONT_NAME)
                    return new Glyph(textures.Get($"{fontName}/{((HexaconsMapping)character).ToString().Replace("_", "-")}"));

                return null;
            }

            public Task<ITexturedCharacterGlyph?> GetAsync(string fontName, char character) => Task.Run(() => Get(fontName, character));

            public Texture? Get(string name, WrapMode wrapModeS, WrapMode wrapModeT) => null;

            public Texture Get(string name) => throw new NotImplementedException();

            public Task<Texture> GetAsync(string name, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public Stream GetStream(string name) => throw new NotImplementedException();

            public IEnumerable<string> GetAvailableResources() => throw new NotImplementedException();

            public Task<Texture?> GetAsync(string name, WrapMode wrapModeS, WrapMode wrapModeT, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public class Glyph : ITexturedCharacterGlyph
            {
                public float XOffset => default;
                public float YOffset => default;
                public float XAdvance => default;
                public float Baseline => default;
                public char Character => default;

                public float GetKerning<T>(T lastGlyph) where T : ICharacterGlyph => throw new NotImplementedException();

                public Texture Texture { get; }
                public float Width => Texture.Width;
                public float Height => Texture.Height;

                public Glyph(Texture texture)
                {
                    Texture = texture;
                }
            }
        }
    }
}
