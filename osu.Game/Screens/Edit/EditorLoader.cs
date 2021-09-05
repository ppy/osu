// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Transition screen for the editor.
    /// Used to avoid backing out to main menu/song select when switching difficulties from within the editor.
    /// </summary>
    public class EditorLoader : ScreenWithBeatmapBackground
    {
        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            PushEditor();
        }

        public void PushEditor([CanBeNull] BeatmapInfo beatmapInfo = null)
        {
            if (beatmapInfo != null)
                Beatmap.Value = beatmapManager.GetWorkingBeatmap(beatmapInfo);

            this.Push(new Editor(this));
            ValidForResume = false;
        }
    }
}
