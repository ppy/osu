// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Select.Details;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Mods
{
    public partial class ModMapInfoContainer : Container
    {
        private ModMapInfoDisplay starRatingDisplay = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private Bindable<BeatmapInfo> adjustedInfo { get; set; } = null!;
        private Bindable<double> starRatingValue = new Bindable<double>();

        //public ModMapInfoContainer()
        //{
        //    
        //}

        protected override void LoadComplete()
        {
            starRatingDisplay = new ModMapInfoDisplay("Star Rating", colours.ForStarDifficulty);
            starRatingDisplay.Current.BindTo(starRatingValue);

            Content.Add(starRatingDisplay);

            adjustedInfo.BindValueChanged(e => { updateValues(); }, true);
        }

        private void updateValues()
        {
            starRatingValue.Value = adjustedInfo.Value.StarRating;
        }
    }
}
