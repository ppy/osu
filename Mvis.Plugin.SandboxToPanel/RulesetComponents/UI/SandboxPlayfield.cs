using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.UI;
using osuTK;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.UI
{
    public partial class SandboxPlayfield : Playfield
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            TextFlowContainer flow;

            InternalChildren = new Drawable[]
            {
                HitObjectContainer,
                flow = new TextFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 10)
                }
            };

            flow.AddText(new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "How to use:",
                Colour = colours.Yellow,
                Font = OsuFont.GetFont(size: 30, weight: FontWeight.Bold)
            });

            flow.AddText(new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "* go to Main Menu",
                Font = OsuFont.GetFont(size: 20)
            });

            flow.AddText(new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "* open settings",
                Font = OsuFont.GetFont(size: 20)
            });

            flow.AddText(new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "* type \"Sandbox\"",
                Font = OsuFont.GetFont(size: 20)
            });

            flow.AddText(new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "* click \"Open Main Screen\"",
                Font = OsuFont.GetFont(size: 20)
            });
        }
    }
}
