// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Users.Drawables
{
    /// <summary>
    /// A team logo which can update to a new team when needed.
    /// </summary>
    public partial class UpdateableTeamFlag : ModelBackedDrawable<APITeam?>
    {
        public APITeam? Team
        {
            get => Model;
            set
            {
                Model = value;
                Invalidate(Invalidation.Presence);
            }
        }

        protected override double LoadDelay => 200;

        public UpdateableTeamFlag(APITeam? team = null)
        {
            Team = team;

            Masking = true;
        }

        protected override Drawable? CreateDrawable(APITeam? team)
        {
            if (team == null)
                return Empty();

            return new TeamFlag(team) { RelativeSizeAxes = Axes.Both };
        }

        // Generally we just want team flags to disappear if the user doesn't have one.
        // This also handles fill flow cases and avoids spacing being added for non-displaying flags.
        public override bool IsPresent => base.IsPresent && Team != null;

        protected override void Update()
        {
            base.Update();

            CornerRadius = DrawHeight / 8;
        }

        [LongRunningLoad]
        public partial class TeamFlag : CompositeDrawable, IHasTooltip
        {
            private readonly APITeam team;

            public LocalisableString TooltipText { get; }

            public TeamFlag(APITeam team)
            {
                this.team = team;
                TooltipText = team.Name;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textures)
            {
                InternalChildren = new Drawable[]
                {
                    new HoverClickSounds(),
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.FromHex("333"),
                    },
                    new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Texture = textures.Get(team.FlagUrl),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        FillMode = FillMode.Fit,
                    }
                };
            }
        }
    }
}
