// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// A <see cref="SkinProvidingContainer"/> that fires <see cref="ISkinSource.SourceChanged"/> when users have made a change to the beatmap skin
    /// of the map being edited.
    /// </summary>
    public partial class EditorSkinProvidingContainer : RulesetSkinProvidingContainer
    {
        private readonly EditorBeatmapSkin? beatmapSkin;

        public EditorSkinProvidingContainer(EditorBeatmap editorBeatmap)
            : base(editorBeatmap.PlayableBeatmap.BeatmapInfo.Ruleset.CreateInstance(), editorBeatmap.PlayableBeatmap, editorBeatmap.BeatmapSkin?.Skin)
        {
            beatmapSkin = editorBeatmap.BeatmapSkin;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (beatmapSkin != null)
                beatmapSkin.BeatmapSkinChanged += TriggerSourceChanged;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmapSkin != null)
                beatmapSkin.BeatmapSkinChanged -= TriggerSourceChanged;
        }
    }
}
