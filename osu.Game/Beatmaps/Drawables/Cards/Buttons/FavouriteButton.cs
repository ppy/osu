// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public class FavouriteButton : BeatmapCardIconButton
    {
        private readonly APIBeatmapSet beatmapSet;

        public FavouriteButton(APIBeatmapSet beatmapSet)
        {
            this.beatmapSet = beatmapSet;

            Icon.Icon = FontAwesome.Regular.Heart;
        }

        // TODO: implement behaviour
    }
}
