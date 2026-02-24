// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text;
using BenchmarkDotNet.Attributes;
using NUnit.Framework;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Rendering.Dummy;
using osu.Framework.IO.Stores;
using osu.Framework.Text;
using osu.Game.Graphics;
using osu.Game.Resources;

namespace osu.Game.Benchmarks
{
    public class BenchmarkEmojiStore : BenchmarkTest
    {
        private TestEmojiStore store = null!;
        private ITexturedCharacterGlyph? glyph = null;

        public override void SetUp()
        {
            store = new TestEmojiStore(new DummyRenderer(), new ResourceStore<byte[]>(new DllResourceStore(OsuResources.ResourceAssembly)));
        }

        [Benchmark]
        [Arguments(1)]
        [Arguments(10)]
        [Arguments(100)]
        public void BenchmarkToGetResource(int n)
        {
            for (int i = 0; i < n; i++)
            {
                // from 🐀 (U+1F400) we have enough consecutive emojis to test
                glyph = store.Get(null, (Grapheme)new Rune(0x1f400 + i));
                // clear texture cache, for consistent results
                store.Purge(glyph!);
            }
        }

        [Benchmark]
        public void BenchmarkMissingEmoji()
        {
            glyph = store.Get(null, new Grapheme(" "));
            Assert.Null(glyph);
        }

        private class TestEmojiStore : EmojiStore
        {
            public TestEmojiStore(IRenderer renderer, ResourceStore<byte[]> resourceStore)
                : base(renderer, resourceStore) { }

            public void Purge(ITexturedCharacterGlyph glyph)
            {
                Purge(glyph.Texture);
            }
        }
    }
}
