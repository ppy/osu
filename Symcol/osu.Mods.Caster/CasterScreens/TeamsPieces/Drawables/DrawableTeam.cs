using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;
using Symcol.Core.Graphics.Containers;
using Symcol.osu.Mods.Caster.CasterScreens.Pieces;

namespace Symcol.osu.Mods.Caster.CasterScreens.TeamsPieces.Drawables
{
    public class DrawableTeam : EditableOsuSpriteText
    {
        public readonly Team Team;

        public DrawableTeam(Team team, FillFlowContainer<DrawableTeam> teams, Bindable<bool> bindable)
        {
            Team = team;

            if (bindable != null)
                Editable.BindTo(bindable);

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Text = Team.Name;
            TextSize = 32;

            Add(new SymcolClickableContainer
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,

                Size = new Vector2(TextSize / 2),

                Action = () => teams.Remove(this),

                Child = new SpriteIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.fa_osu_cross_o,
                    Colour = Color4.Red
                }
            });

            OsuTextBox.Current.ValueChanged += t => { Team.Name = t; };
        }
    }
}
