using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using Symcol.Core.Graphics.Containers;
using Symcol.osu.Mods.Caster.CasterScreens.Pieces;
using System;
using System.Diagnostics;

namespace Symcol.osu.Mods.Caster.CasterScreens.TeamsPieces.Drawables
{
    public class DrawablePlayer : EditableOsuSpriteText
    {
        public readonly Player Player;

        public DrawablePlayer(Player player, FillFlowContainer<DrawablePlayer> players, Bindable<bool> bindable)
        {
            Player = player;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Editable.BindTo(bindable);

            Text = Player.Name;
            TextSize = 32;

            OsuSpriteText.Position = new Vector2(18, 0);

            SpriteIcon icon = new SpriteIcon
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Icon = FontAwesome.fa_chevron_right,
                Size = new Vector2(TextSize / 2),
                Colour = Color4.White
            };

            OsuTextBox.Width = 0.42f;
            OsuTextBox idBox = new OsuTextBox
            {
                Alpha = 0,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Position = new Vector2(-20, 0),
                RelativeSizeAxes = Axes.X,
                Height = TextSize,
                Width = 0.42f,
                Text = Player.PlayerID.ToString()
            };

            SymcolClickableContainer delete = new SymcolClickableContainer
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,

                Size = new Vector2(TextSize / 2),

                Action = () => players.Remove(this),

                Child = new SpriteIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.fa_osu_cross_o,
                    Colour = Color4.Red
                }
            };

            Add(idBox);
            Add(icon);
            Add(delete);

            OsuTextBox.OnCommit += (commit, ree) => { Player.Name = commit.Text; };

            idBox.OnCommit += (commit, ree) =>
            {
                try
                {
                    int i = Int32.Parse(commit.Text);
                    Player.PlayerID = i;

                    string u = $@"https://osu.ppy.sh/users/{i}";

                    OsuSpriteText.Tooltip = u;
                    OsuSpriteText.Action = () => { Process.Start(u); };
                }
                catch { Logger.Log(commit.Text + " is not a valid user id!", LoggingTarget.Runtime, LogLevel.Error); }
            };

            Editable.ValueChanged += edit =>
            {
                icon.Alpha = edit ? 0 : 1;
                idBox.Alpha = edit ? 1 : 0;
                delete.Alpha = edit ? 1 : 0;

                if (!edit)
                {
                    Player.Name = OsuTextBox.Text;

                    try
                    {
                        int i = Int32.Parse(idBox.Text);
                        Player.PlayerID = i;

                        string u = $@"https://osu.ppy.sh/users/{i}";

                        OsuSpriteText.Tooltip = u;
                        OsuSpriteText.Action = () => { Process.Start(u); };
                    }
                    catch
                    {
                        Logger.Log(OsuTextBox.Text + " is not a valid user id!", LoggingTarget.Runtime, LogLevel.Error);
                        idBox.Text = Player.PlayerID.ToString();
                    }
                }
            };
            Editable.TriggerChange();

            string url = $@"https://osu.ppy.sh/users/{Player.PlayerID}";

            OsuSpriteText.Tooltip = url;
            OsuSpriteText.Action = () => { Process.Start(url); };
        }
    }
}
