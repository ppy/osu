// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class MetadataSectionUserTags : MetadataSection<string[]?>
    {
        private readonly Action<string>? searchAction;

        public MetadataSectionUserTags(Action<string>? searchAction = null)
            : base(MetadataType.UserTags, null)
        {
            this.searchAction = searchAction;
        }

        protected override void AddMetadata(string[]? tags, LinkFlowContainer loaded)
        {
            if (tags == null)
                return;

            for (int i = 0; i <= tags.Length - 1; i++)
            {
                string tag = tags[i];

                if (searchAction != null)
                    loaded.AddLink(tag, () => searchAction(tag));
                else
                    loaded.AddLink(tag, LinkAction.SearchBeatmapSet, $@"tag=""""{tag}""""");

                if (i != tags.Length - 1)
                    loaded.AddText(" ");
            }
        }
    }
}
