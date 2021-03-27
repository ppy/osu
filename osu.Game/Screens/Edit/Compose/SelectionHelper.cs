// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Screens.Edit.Compose
{
    public class SelectionHelper : Component
    {
        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private EditorClock clock { get; set; }

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; }

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; }

        public void CopySelectionToClipboard()
        {
            host.GetClipboard().SetText(formatSelectionAsString());
        }

        private string formatSelectionAsString()
        {
            const string separator = " - ";
            var builder = new StringBuilder();

            if (!editorBeatmap.SelectedHitObjects.Any())
            {
                builder.Append($"{clock.CurrentTime.ToEditorFormattedString()}{separator}");
                return builder.ToString();
            }

            string hitObjects = composer != null ? string.Join(',', composer.ConvertSelectionToString()) : string.Empty;

            builder.Append(editorBeatmap.SelectedHitObjects.Min(h => h.StartTime).ToEditorFormattedString());
            builder.Append($" ({hitObjects}){separator}");
            return builder.ToString();
        }
    }
}
