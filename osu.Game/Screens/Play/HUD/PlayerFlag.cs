// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Skinning;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class PlayerFlag : CompositeDrawable, ISerialisableDrawable
    {
        protected override bool ReceivePositionalInputAtSubTree(Vector2 screenSpacePos) => false;

        private readonly UpdateableFlag flag;

        private const float default_size = 40f;

        [Resolved]
        private GameplayState? gameplayState { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private IBindable<APIUser>? apiUser;

        public PlayerFlag()
        {
            Size = new Vector2(default_size, default_size / 1.4f);

            InternalChild = flag = new UpdateableFlag
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (gameplayState != null)
                flag.CountryCode = gameplayState.Score.ScoreInfo.User.CountryCode;
            else
            {
                apiUser = api.LocalUser.GetBoundCopy();
                apiUser.BindValueChanged(u => flag.CountryCode = u.NewValue.CountryCode, true);
            }
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
