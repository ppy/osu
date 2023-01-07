// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class MetadataSectionNominators : MetadataSection<(BeatmapSetOnlineNomination[] CurrentNominations, APIUser[] RelatedUsers)>
    {
        public override (BeatmapSetOnlineNomination[] CurrentNominations, APIUser[] RelatedUsers) Metadata
        {
            set
            {
                if (value.CurrentNominations.Length == 0)
                {
                    this.FadeOut(TRANSITION_DURATION);
                    return;
                }

                base.Metadata = value;
            }
        }

        public MetadataSectionNominators(Action<(BeatmapSetOnlineNomination[] CurrentNominations, APIUser[] RelatedUsers)>? searchAction = null)
            : base(MetadataType.Nominators, searchAction)
        {
        }

        protected override void AddMetadata((BeatmapSetOnlineNomination[] CurrentNominations, APIUser[] RelatedUsers) metadata, LinkFlowContainer loaded)
        {
            int[] nominatorIds = metadata.CurrentNominations.Select(n => n.UserId).ToArray();

            int nominatorsFound = 0;

            foreach (int nominatorId in nominatorIds)
            {
                foreach (var user in metadata.RelatedUsers)
                {
                    if (nominatorId != user.OnlineID) continue;

                    nominatorsFound++;

                    loaded.AddUserLink(new APIUser
                    {
                        Username = user.Username,
                        Id = nominatorId,
                    });

                    if (nominatorsFound < nominatorIds.Length)
                        loaded.AddText(CommonStrings.ArrayAndWordsConnector);

                    break;
                }
            }
        }
    }
}
