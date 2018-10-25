using osu.Framework.Graphics;
using OpenTK;
using Symcol.osu.Mods.Caster.Pieces;
using Symcol.osu.Mods.Caster.CasterScreens.TeamsPieces;

namespace Symcol.osu.Mods.Caster.CasterScreens
{
    public class Teams : CasterSubScreen
    {
        private readonly TeamBox leftTeam;
        private readonly TeamBox rightTeam;

        public Teams(CasterControlPanel controlPanel)
            : base(controlPanel)
        {
            Children = new Drawable[]
            {
                leftTeam = new TeamBox(controlPanel)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Position = new Vector2(4, 0)
                },
                rightTeam = new TeamBox(controlPanel)
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Position = new Vector2(-4, 0)
                },
            };
        }
    }
}
