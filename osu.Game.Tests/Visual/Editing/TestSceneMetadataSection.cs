// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Setup;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneMetadataSection : OsuTestScene
    {
        [Cached]
        private EditorBeatmap editorBeatmap = new EditorBeatmap(new Beatmap());

        private TestMetadataSection metadataSection;

        [Test]
        public void TestMinimalMetadata()
        {
            AddStep("set metadata", () =>
            {
                editorBeatmap.Metadata.Artist = "Example Artist";
                editorBeatmap.Metadata.ArtistUnicode = string.Empty;

                editorBeatmap.Metadata.Title = "Example Title";
                editorBeatmap.Metadata.TitleUnicode = string.Empty;
            });

            createSection();

            assertArtist("Example Artist");
            assertRomanisedArtist("Example Artist", false);

            assertTitle("Example Title");
            assertRomanisedTitle("Example Title", false);
        }

        [Test]
        public void TestInitialisationFromNonRomanisedVariant()
        {
            AddStep("set metadata", () =>
            {
                editorBeatmap.Metadata.ArtistUnicode = "＊なみりん";
                editorBeatmap.Metadata.Artist = string.Empty;

                editorBeatmap.Metadata.TitleUnicode = "コイシテイク・プラネット";
                editorBeatmap.Metadata.Title = string.Empty;
            });

            createSection();

            assertArtist("＊なみりん");
            assertRomanisedArtist(string.Empty, true);

            assertTitle("コイシテイク・プラネット");
            assertRomanisedTitle(string.Empty, true);
        }

        [Test]
        public void TestInitialisationPreservesOriginalValues()
        {
            AddStep("set metadata", () =>
            {
                editorBeatmap.Metadata.ArtistUnicode = "＊なみりん";
                editorBeatmap.Metadata.Artist = "*namirin";

                editorBeatmap.Metadata.TitleUnicode = "コイシテイク・プラネット";
                editorBeatmap.Metadata.Title = "Koishiteiku Planet";
            });

            createSection();

            assertArtist("＊なみりん");
            assertRomanisedArtist("*namirin", true);

            assertTitle("コイシテイク・プラネット");
            assertRomanisedTitle("Koishiteiku Planet", true);
        }

        [Test]
        public void TestValueTransfer()
        {
            AddStep("set metadata", () =>
            {
                editorBeatmap.Metadata.ArtistUnicode = "＊なみりん";
                editorBeatmap.Metadata.Artist = string.Empty;

                editorBeatmap.Metadata.TitleUnicode = "コイシテイク・プラネット";
                editorBeatmap.Metadata.Title = string.Empty;
            });

            createSection();

            AddStep("set romanised artist name", () => metadataSection.ArtistTextBox.Current.Value = "*namirin");
            assertArtist("*namirin");
            assertRomanisedArtist("*namirin", false);

            AddStep("set native artist name", () => metadataSection.ArtistTextBox.Current.Value = "＊なみりん");
            assertArtist("＊なみりん");
            assertRomanisedArtist("*namirin", true);

            AddStep("set romanised title", () => metadataSection.TitleTextBox.Current.Value = "Hitokoto no kyori");
            assertTitle("Hitokoto no kyori");
            assertRomanisedTitle("Hitokoto no kyori", false);

            AddStep("set native title", () => metadataSection.TitleTextBox.Current.Value = "ヒトコトの距離");
            assertTitle("ヒトコトの距離");
            assertRomanisedTitle("Hitokoto no kyori", true);
        }

        private void createSection()
            => AddStep("create metadata section", () => Child = metadataSection = new TestMetadataSection());

        private void assertArtist(string expected)
            => AddAssert($"artist is {expected}", () => metadataSection.ArtistTextBox.Current.Value == expected);

        private void assertRomanisedArtist(string expected, bool editable)
        {
            AddAssert($"romanised artist is {expected}", () => metadataSection.RomanisedArtistTextBox.Current.Value == expected);
            AddAssert($"romanised artist is {(editable ? "" : "not ")}editable", () => metadataSection.RomanisedArtistTextBox.ReadOnly == !editable);
        }

        private void assertTitle(string expected)
            => AddAssert($"title is {expected}", () => metadataSection.TitleTextBox.Current.Value == expected);

        private void assertRomanisedTitle(string expected, bool editable)
        {
            AddAssert($"romanised title is {expected}", () => metadataSection.RomanisedTitleTextBox.Current.Value == expected);
            AddAssert($"romanised title is {(editable ? "" : "not ")}editable", () => metadataSection.RomanisedTitleTextBox.ReadOnly == !editable);
        }

        private class TestMetadataSection : MetadataSection
        {
            public new LabelledTextBox ArtistTextBox => base.ArtistTextBox;
            public new LabelledTextBox RomanisedArtistTextBox => base.RomanisedArtistTextBox;

            public new LabelledTextBox TitleTextBox => base.TitleTextBox;
            public new LabelledTextBox RomanisedTitleTextBox => base.RomanisedTitleTextBox;
        }
    }
}
