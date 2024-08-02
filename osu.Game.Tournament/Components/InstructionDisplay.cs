// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class InstructionDisplay : CompositeDrawable
    {
        private readonly InstructionInfo thisStep = null!;

        public InstructionDisplay(TeamColour team = TeamColour.Neutral, Steps step = Steps.Default)
        {
            thisStep = new InstructionInfo
            (
                team: team,
                step: step
            );
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Height = 100;
            Width = 500;
            AlwaysPresent = true;

            InternalChild = new FillFlowContainer
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Direction = FillDirection.Horizontal,
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(20, 0),
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = thisStep.Icon,
                        Size = new Vector2(56),
                        Colour = thisStep.IconColor,
                        Alpha = 1,
                        Shadow = true,
                        ShadowColour = Color4.Black.Opacity(0.5f),
                        ShadowOffset = new Vector2(2, 3),
                    },
                    new Box
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Colour = Color4.White,
                        RelativeSizeAxes = Axes.Y,
                        Height = 0.85f,
                        Width = 3,
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Direction = FillDirection.Vertical,

                        // Add Margin to the bottom inorder to let Trap messages fit in the box.
                        Margin = new MarginPadding { Bottom = 12 },
                        Children = new Drawable[]
                        {
                            new TournamentSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = thisStep.Name,
                                Font = OsuFont.GetFont(typeface: Typeface.HarmonyOSSans, size: 49, weight: FontWeight.Bold),
                                //OsuFont.Default.With(size: 49, weight: FontWeight.Bold),
                            },
                            new TournamentSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = thisStep.Description,
                                Font = OsuFont.GetFont(typeface: Typeface.HarmonyOSSans, size: 30, weight: FontWeight.Regular),
                                // OsuFont.Default.With(size: 30, weight: FontWeight.Regular),
                            },
                        }
                    }
                }
            };
        }
    }
}
