using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Tournament.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Screens.Tournament
{
    public class Drawings : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenDefault();

        public Drawings()
        {
            GroupsContainer gc;
            ScrollingTeamContainer stc;
            Container visualiserContainer;

            Children = new Drawable[]
            {
                // Visualiser
                visualiserContainer = new Container()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(1, 10),

                    Colour = new Color4(255, 204, 34, 255)
                },
                // Groups
                gc = new GroupsContainer(8)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,

                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,

                    Padding = new MarginPadding()
                    {
                        Top = 35f,
                        Bottom = 35f
                    }
                },
                stc = new ScrollingTeamContainer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.X,
                    Width = 0.75f
                },
            };

            float offset = 0;
            for (int i = 0; i < 6; i++)
            {
                visualiserContainer.Add(new VisualiserLine(2 * (float)Math.PI, offset, RNG.Next(10000, 12000))
                {
                    RelativeSizeAxes = Axes.Both,
                });

                offset += (float)Math.PI / 6f;
            }


            Team t = new Team()
            {
                FullName = "Australia",
                Acronym = "AUS",
                FlagName = "AU"
            };

            List<Team> teams = new List<Team>();

            for (int i = 0; i < 17; i++)
            {
                gc.AddTeam(t);
                teams.Add(t);
            }

            stc.AvailableTeams = teams;

            stc.StartScrolling();
            Delay(3000).Schedule(() => stc.StopScrolling());
        }
    }
}
