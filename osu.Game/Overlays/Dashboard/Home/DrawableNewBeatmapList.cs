// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Dashboard.Home
{
    public partial class DrawableNewBeatmapList : DrawableBeatmapList
    {
        public DrawableNewBeatmapList(List<APIBeatmapSet> beatmapSets)
            : base(beatmapSets)
        {
        }

        protected override DashboardBeatmapPanel CreateBeatmapPanel(APIBeatmapSet beatmapSet) => new DashboardNewBeatmapPanel(beatmapSet);

        protected override LocalisableString Title => HomeStrings.UserBeatmapsNew;
    }
}
