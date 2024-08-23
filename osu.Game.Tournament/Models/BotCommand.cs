using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Handle special commands from IRC chat.
    /// </summary>
    [Serializable]
    public class BotCommand
    {
        private Regex finalRegex = new Regex("\\[\\*\\] (.+)获胜");
        private Regex winRegex = new Regex("\\[\\*\\] 执行将(.*)设为(.*)获胜");
        private Regex stateRegex = new Regex("\\[\\*\\] 检查棋盘: 进入(.*)");
        private Regex pickEXRegex = new Regex("\\[\\*\\] 执行强制选取EX(.*) - 已完成");

        public Commands Command;
        public TeamColour Team;
        public string MapMod;

        public BotCommand(Commands command = Commands.Unknown, TeamColour team = TeamColour.Neutral,
            string map = "")
        {
            Command = command;
            Team = team;
            MapMod = map;
        }

        public BotCommand ParseFromText(string line)
        {
            GroupCollection obj;

            if (finalRegex.Match(line).Success)
            {
                obj = finalRegex.Match(line).Groups;
                return new BotCommand(Commands.SetWin, TranslateFromTeamName(obj[0].Value));
            }
            if (pickEXRegex.Match(line).Success)
            {
                obj = pickEXRegex.Match(line).Groups;
                return new BotCommand(Commands.PickEX, map: "EX" + obj[0].Value);
            }
            if (stateRegex.Match(line).Success)
            {
                obj = stateRegex.Match(line).Groups;
                if (obj[0].Value == "EX图池")
                {
                    return new BotCommand(Commands.EnterEX);
                }
            }
            if (winRegex.Match(line).Success)
            {
                obj = winRegex.Match(line).Groups;
                return new BotCommand(Commands.MarkWin, team: TranslateFromTeamName(obj[1].Value), map: obj[0].Value);
            }
            return new BotCommand(Commands.Unknown);
        }

        public TeamColour TranslateFromTeamName(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "红队":
                case "红":
                case "red":
                    return TeamColour.Red;

                case "蓝队":
                case "蓝":
                case "blue":
                    return TeamColour.Blue;

                default:
                    return TeamColour.Neutral;
            }
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Commands
    {
        SetWin,
        EnterEX,
        PickEX,
        MarkEXWin,
        MarkWin,
        Unknown
    }
}
