// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Setup;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneMetadataSection : OsuManualInputManagerTestScene
    {
        [Cached]
        private EditorBeatmap editorBeatmap = new EditorBeatmap(new Beatmap
        {
            BeatmapInfo =
            {
                Ruleset = new OsuRuleset().RulesetInfo
            },
        });

        private TestMetadataSection metadataSection;

        [Test]
        public void TestUpdateViaTextBoxOnFocusLoss()
        {
            AddStep("set metadata", () =>
            {
                editorBeatmap.Metadata.Artist = "Example Artist";
                editorBeatmap.Metadata.ArtistUnicode = string.Empty;
            });

            createSection();

            TextBox textbox;

            AddStep("focus first textbox", () =>
            {
                textbox = metadataSection.ChildrenOfType<TextBox>().First();
                InputManager.MoveMouseTo(textbox);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("simulate changing textbox", () =>
            {
                // Can't simulate text input but this should work.
                InputManager.Keys(PlatformAction.SelectAll);
                InputManager.Keys(PlatformAction.Copy);
                InputManager.Keys(PlatformAction.Paste);
                InputManager.Keys(PlatformAction.Paste);
            });

            assertArtistMetadata("Example Artist");

            // It's important values are committed immediately on focus loss so the editor exit sequence detects them.
            AddAssert("value immediately changed on focus loss", () =>
            {
                InputManager.TriggerFocusContention(metadataSection);
                return editorBeatmap.Metadata.Artist;
            }, () => Is.EqualTo("Example ArtistExample Artist"));
        }

        [Test]
        public void TestUpdateViaTextBoxOnCommit()
        {
            AddStep("set metadata", () =>
            {
                editorBeatmap.Metadata.Artist = "Example Artist";
                editorBeatmap.Metadata.ArtistUnicode = string.Empty;
            });

            createSection();

            TextBox textbox;

            AddStep("focus first textbox", () =>
            {
                textbox = metadataSection.ChildrenOfType<TextBox>().First();
                InputManager.MoveMouseTo(textbox);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("simulate changing textbox", () =>
            {
                // Can't simulate text input but this should work.
                InputManager.Keys(PlatformAction.SelectAll);
                InputManager.Keys(PlatformAction.Copy);
                InputManager.Keys(PlatformAction.Paste);
                InputManager.Keys(PlatformAction.Paste);
            });

            assertArtistMetadata("Example Artist");

            AddStep("commit", () => InputManager.Key(Key.Enter));

            assertArtistMetadata("Example ArtistExample Artist");
        }

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

            assertArtistTextBox("Example Artist");
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

            assertArtistTextBox("＊なみりん");
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

            assertArtistTextBox("＊なみりん");
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
            assertArtistTextBox("*namirin");
            assertRomanisedArtist("*namirin", false);

            AddStep("set native artist name", () => metadataSection.ArtistTextBox.Current.Value = "＊なみりん");
            assertArtistTextBox("＊なみりん");
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

        private void assertArtistMetadata(string expected)
            => AddAssert($"artist metadata is {expected}", () => editorBeatmap.Metadata.Artist, () => Is.EqualTo(expected));

        private void assertArtistTextBox(string expected)
            => AddAssert($"artist textbox is {expected}", () => metadataSection.ArtistTextBox.Current.Value, () => Is.EqualTo(expected));

        private void assertRomanisedArtist(string expected, bool editable)
        {
            AddAssert($"romanised artist is {expected}", () => metadataSection.RomanisedArtistTextBox.Current.Value, () => Is.EqualTo(expected));
            AddAssert($"romanised artist is {(editable ? "" : "not ")}editable", () => metadataSection.RomanisedArtistTextBox.ReadOnly == !editable);
        }

        private void assertTitle(string expected)
            => AddAssert($"title is {expected}", () => metadataSection.TitleTextBox.Current.Value, () => Is.EqualTo(expected));

        private void assertRomanisedTitle(string expected, bool editable)
        {
            AddAssert($"romanised title is {expected}", () => metadataSection.RomanisedTitleTextBox.Current.Value, () => Is.EqualTo(expected));
            AddAssert($"romanised title is {(editable ? "" : "not ")}editable", () => metadataSection.RomanisedTitleTextBox.ReadOnly == !editable);
        }

        private partial class TestMetadataSection : MetadataSection
        {
            public new LabelledTextBox ArtistTextBox => base.ArtistTextBox;
            public new LabelledTextBox RomanisedArtistTextBox => base.RomanisedArtistTextBox;

            public new LabelledTextBox TitleTextBox => base.TitleTextBox;
            public new LabelledTextBox RomanisedTitleTextBox => base.RomanisedTitleTextBox;
        }
    }
}
