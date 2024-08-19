// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online;
using osu.Game.Online.Chat;

namespace osu.Game.Screens.SelectV2.Wedge
{
    public partial class LocalDifficultyNameContent : DifficultyNameContent
    {
        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private ILinkHandler? linkHandler { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(b =>
            {
                DifficultyName.Text = b.NewValue.BeatmapInfo.DifficultyName;

                // TODO: should be the mapper of the guest difficulty, but that isn't stored correctly yet (see https://github.com/ppy/osu/issues/12965)
                MapperName.Text = b.NewValue.Metadata.Author.Username;
                MapperLink.Action = () => linkHandler?.HandleLink(new LinkDetails(LinkAction.OpenUserProfile, b.NewValue.Metadata.Author));
            }, true);
        }
    }
}
