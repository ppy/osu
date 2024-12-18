// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Changelog
{
    public partial class ChangelogUpdateStreamControl : OverlayStreamControl<APIUpdateStream>
    {
        public ChangelogUpdateStreamControl()
        {
            SelectFirstTabByDefault = false;
        }

        protected override OverlayStreamItem<APIUpdateStream> CreateStreamItem(APIUpdateStream value) => new ChangelogUpdateStreamItem(value);
    }
}
