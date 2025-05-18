// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class MultiScoreDiff : RollingCounter<long>, ISerialisableDrawable
    {
        [Resolved]
        private Player player { get; set; } = null!;

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; } = null!;

        [Resolved]
        private IGameplayLeaderboardProvider leaderboardProvider { get; set; } = null!;

        [Resolved]
        private SkinEditorOverlay skinEditor { get; set; } = null!;

        protected override double RollingDuration => 0;

        private bool isHidden;

        private List<GameplayLeaderboardScore>? scoresButMe;

        [BackgroundDependencyLoader]
        private void load()
        {
        }

        protected override void Update()
        {
            if (isHidden)
            {
                return;
            }

            if (multiplayerClient.LocalUser == null)
            {
                if (skinEditor.State.Value == Visibility.Hidden)
                {
                    Hide();
                    isHidden = true;
                }

                return;
            }

            base.Update();

            updateCounterValue();
            updateTextColor();
        }

        public bool UsesFixedAnchor { get; set; }

        private void updateCounterValue()
        {
            // If the leaderborad is yet fully loaded
            if (leaderboardProvider.Scores.Count == 0)
            {
                return;
            }

            if (scoresButMe == null)
            {
                scoresButMe = leaderboardProvider.Scores.ToList().Where(s => s.User.Username != multiplayerClient.LocalUser!.User!.Username).ToList();

                // If the user is the only one in the room
                if (scoresButMe.Count == 0)
                {
                    Hide();
                    isHidden = true;

                    return;
                }
            }

            var curTopScore = scoresButMe.ToList().MaxBy(s => s.TotalScore.Value);

            Current.Value = player.Score.ScoreInfo.TotalScore - curTopScore!.TotalScore.Value;
        }

        private void updateTextColor()
        {
            SRGBColour colour = new SRGBColour();

            if (Current.Value < 0)
            {
                colour.SRGB = Color4.Red;
            }
            else
            {
                colour.SRGB = Color4.White;
            }

            Colour = ColourInfo.SingleColour(colour);
        }
    }
}
