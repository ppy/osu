// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Multi.Components
{
    public class ParticipantCountDisplay : MultiplayerComposite
    {
        private const float text_size = 30;
        private const float transition_duration = 100;

        private OsuSpriteText slash, maxText;

        public ParticipantCountDisplay()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            OsuSpriteText count;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                LayoutDuration = transition_duration,
                Children = new[]
                {
                    count = new OsuSpriteText
                    {
                        TextSize = text_size,
                        Font = @"Exo2.0-Bold"
                    },
                    slash = new OsuSpriteText
                    {
                        Text = @"/",
                        TextSize = text_size,
                        Font = @"Exo2.0-Light"
                    },
                    maxText = new OsuSpriteText
                    {
                        TextSize = text_size,
                        Font = @"Exo2.0-Light"
                    },
                }
            };

            MaxParticipants.BindValueChanged(_ => updateMax(), true);
            ParticipantCount.BindValueChanged(v => count.Text = v.ToString("#,0"), true);
        }

        private void updateMax()
        {
            if (MaxParticipants.Value == null)
            {
                slash.FadeOut(transition_duration);
                maxText.FadeOut(transition_duration);
            }
            else
            {
                slash.FadeIn(transition_duration);
                maxText.Text = MaxParticipants.Value.ToString();
                maxText.FadeIn(transition_duration);
            }
        }
    }
}
