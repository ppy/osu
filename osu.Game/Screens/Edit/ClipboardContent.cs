// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.IO.Serialization.Converters;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit
{
    public class ClipboardContent
    {
        [JsonConverter(typeof(TypedListConverter<HitObject>))]
        public IList<HitObject> HitObjects;

        public const string CLIPBOARD_FORMAT = "osu/hitobjects";

        public ClipboardContent()
        {
        }

        public ClipboardContent(EditorBeatmap editorBeatmap)
        {
            HitObjects = editorBeatmap.SelectedHitObjects.ToList();
        }
    }
}
