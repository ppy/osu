// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class MetadataSectionTags : MetadataSection
    {
        public MetadataSectionTags(Action<string>? searchAction = null)
            : base(MetadataType.Tags, searchAction)
        {
        }

        protected override void AddMetadata(string metadata, LinkFlowContainer loaded)
        {
            string[] tags = metadata.Split(" ");

            for (int i = 0; i <= tags.Length - 1; i++)
            {
                string tag = tags[i];

                if (SearchAction != null)
                    loaded.AddLink(tag, () => SearchAction(tag));
                else
                    loaded.AddLink(tag, LinkAction.SearchBeatmapSet, tag);

                if (i != tags.Length - 1)
                    loaded.AddText(" ");
            }
        }
    }
}
