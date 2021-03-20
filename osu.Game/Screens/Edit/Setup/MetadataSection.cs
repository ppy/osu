// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    internal class MetadataSection : SetupSection
    {
        private LabelledTextBox artistTextBox;
        private LabelledTextBox titleTextBox;
        private LabelledTextBox creatorTextBox;
        private LabelledTextBox difficultyTextBox;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "谱面元数据"
                },
                artistTextBox = new LabelledTextBox
                {
                    Label = "艺术家",
                    Current = { Value = Beatmap.Metadata.Artist },
                    TabbableContentContainer = this
                },
                titleTextBox = new LabelledTextBox
                {
                    Label = "标题",
                    Current = { Value = Beatmap.Metadata.Title },
                    TabbableContentContainer = this
                },
                creatorTextBox = new LabelledTextBox
                {
                    Label = "谱师",
                    Current = { Value = Beatmap.Metadata.AuthorString },
                    TabbableContentContainer = this
                },
                difficultyTextBox = new LabelledTextBox
                {
                    Label = "难度名称",
                    Current = { Value = Beatmap.BeatmapInfo.Version },
                    TabbableContentContainer = this
                },
            };

            foreach (var item in Children.OfType<LabelledTextBox>())
                item.OnCommit += onCommit;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (string.IsNullOrEmpty(artistTextBox.Current.Value))
                GetContainingInputManager().ChangeFocus(artistTextBox);
        }

        private void onCommit(TextBox sender, bool newText)
        {
            if (!newText) return;

            // for now, update these on commit rather than making BeatmapMetadata bindables.
            // after switching database engines we can reconsider if switching to bindables is a good direction.
            Beatmap.Metadata.Artist = artistTextBox.Current.Value;
            Beatmap.Metadata.Title = titleTextBox.Current.Value;
            Beatmap.Metadata.AuthorString = creatorTextBox.Current.Value;
            Beatmap.BeatmapInfo.Version = difficultyTextBox.Current.Value;
        }
    }
}
