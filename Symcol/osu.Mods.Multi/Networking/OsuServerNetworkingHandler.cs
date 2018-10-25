using System.Collections.Generic;
using System.Net;
using osu.Framework.Logging;
using osu.Mods.Multi.Networking.Packets.Lobby;
using osu.Mods.Multi.Networking.Packets.Match;
using osu.Mods.Multi.Networking.Packets.Player;
using Symcol.Networking.NetworkingHandlers;
using Symcol.Networking.NetworkingHandlers.Server;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Networking
{
    public class OsuServerNetworkingHandler : ServerNetworkingHandler
    {
        protected override string Gamekey => "osu";

        protected readonly List<OsuMatch> OsuMatches = new List<OsuMatch>();

        protected override Client CreateConnectingClient(ConnectPacket connectPacket)
        {
            OsuConnectPacket osuConnectPacket = (OsuConnectPacket)connectPacket;

            OsuClient c = new OsuClient
            {
                EndPoint = new IPEndPoint(IPAddress.Parse(NetworkingClient.EndPoint.Address.ToString()), NetworkingClient.EndPoint.Port),
                LastConnectionTime = Time.Current,
                Statues = ConnectionStatues.Connecting,
                User = osuConnectPacket.User,
            };
            c.OnDisconnected += () => Clients.Remove(c);

            return c;
        }

        protected override void HandlePackets(PacketInfo info)
        {
            Logger.Log($"Recieved a Packet from {NetworkingClient.EndPoint}", LoggingTarget.Network, LogLevel.Debug);

            if (!HandlePacket(info.Packet))
                return;

            MatchListPacket.MatchInfo match = null;

            switch (info.Packet)
            {
                default:
                    base.HandlePackets(info);
                    break;
                case GetMatchListPacket getMatch:
                    MatchListPacket matchList = new MatchListPacket
                    {
                        MatchInfoList = GetMatches()
                    };
                    ReturnToClient(matchList);
                    break;
                case CreateMatchPacket createMatch:
                    OsuMatches.Add(new OsuMatch
                    {
                        MatchInfo = createMatch.MatchInfo,
                        MatchLastUpdateTime = Time.Current
                    });
                    ReturnToClient(new MatchCreatedPacket{ MatchInfo = createMatch.MatchInfo });
                    break;
                case JoinMatchPacket joinPacket:
                    if (joinPacket.User == null) break;

                    foreach (MatchListPacket.MatchInfo m in GetMatches())
                        if (m.BeatmapTitle == joinPacket.Match.BeatmapTitle &&
                            m.BeatmapArtist == joinPacket.Match.BeatmapArtist &&
                            m.Name == joinPacket.Match.Name &&
                            m.Username == joinPacket.Match.Username)
                            match = m;

                    if (match != null)
                    {
                        //Tell everyone already there someone joined
                        ShareWithMatchClients(match, new PlayerJoinedPacket
                        {
                            User = joinPacket.User
                        });

                        //Add them
                        match.Users.Add(joinPacket.User);

                        foreach (OsuMatch s in OsuMatches)
                            if (s.MatchInfo == match)
                            {
                                OsuClient osu = GetClient(joinPacket.User);
                                osu.User = joinPacket.User;
                                s.Clients.Add(osu);
                            }

                        //Tell them they have joined
                        ReturnToClient(new JoinedMatchPacket { MatchInfo = match });
                    }
                    else
                        Logger.Log("Couldn't find a match matching one in packet!", LoggingTarget.Network, LogLevel.Error);

                    break;
                case GetMapPacket getMap:
                    match = GetMatch(getMap.User);
                    NetworkingClient.SendPacket(SignPacket(new SetMapPacket
                    {
                        OnlineBeatmapSetID = match.OnlineBeatmapSetID,
                        OnlineBeatmapID = match.OnlineBeatmapID,
                        BeatmapTitle = match.BeatmapTitle,
                        BeatmapArtist = match.BeatmapArtist,
                        BeatmapMapper = match.BeatmapMapper,
                        BeatmapDifficulty = match.BeatmapDifficulty,
                        RulesetID = match.RulesetID,
                    }), GetLastClient().EndPoint);
                    break;
                case SetMapPacket map:
                    match = GetMatch(map.User);

                    match.OnlineBeatmapSetID = map.OnlineBeatmapSetID;
                    match.OnlineBeatmapID = map.OnlineBeatmapID;
                    match.BeatmapTitle = map.BeatmapTitle;
                    match.BeatmapArtist = map.BeatmapArtist;
                    match.BeatmapMapper = map.BeatmapMapper;
                    match.BeatmapDifficulty = map.BeatmapDifficulty;
                    match.RulesetID = map.RulesetID;

                    ShareWithMatchClients(match, map);
                    break;
                case ChatPacket chat:
                    ShareWithMatchClients(GetMatch(chat.User), chat);
                    break;
                case LeavePacket leave:
                    match = GetMatch(leave.User);
                    if (match != null)
                        foreach (OsuUserInfo player in match.Users)
                            if (player.ID == leave.User.ID)
                            {
                                match.Users.Remove(player);
                                //Tell everyone already there someone joined
                                ShareWithMatchClients(match, new PlayerDisconnectedPacket
                                {
                                    User = leave.User
                                });

                                foreach (OsuMatch m in OsuMatches)
                                {
                                    foreach (OsuClient p in m.Clients)
                                        if (p.User.ID == leave.User.ID)
                                        {
                                            m.Clients.Remove(p);
                                            break;
                                        }
                                    foreach (OsuClient p in m.LoadedClients)
                                        if (p.User.ID == leave.User.ID)
                                        {
                                            m.LoadedClients.Remove(p);
                                            break;
                                        }
                                }

                                //Update their matchlist next
                                MatchListPacket list = new MatchListPacket();
                                list = (MatchListPacket)SignPacket(list);
                                list.MatchInfoList = GetMatches();
                                NetworkingClient.SendPacket(list, GetLastClient().EndPoint);
                                return;
                            }
                    Logger.Log("Couldn't find a player to remove who told us they were leaving!", LoggingTarget.Network, LogLevel.Error);
                    break;
                case StartMatchPacket start:
                    match = GetMatch(start.User);
                    ShareWithMatchClients(match, new MatchLoadingPacket
                    {
                        Users = match.Users
                    });
                    break;
                case PlayerLoadedPacket loaded:
                    foreach (OsuMatch m in OsuMatches)
                        foreach (OsuClient p in m.Clients)
                            if (p.User.ID == loaded.User.ID)
                            {
                                m.Clients.Remove(p);
                                m.LoadedClients.Add(p);

                                if (m.Clients.Count == 0)
                                    ShareWithMatchClients(m.MatchInfo, new MatchStartingPacket());

                                return;
                            }

                    Logger.Log("A Player we can't find told us they have loaded!", LoggingTarget.Network, LogLevel.Error);
                    break;
                case ScorePacket score:
                    foreach (OsuMatch m in OsuMatches)
                        foreach (OsuClient p in m.LoadedClients)
                            if (p.User.ID == score.UserID)
                                ShareWithMatchClients(m.MatchInfo, score);
                    break;
                case CursorPositionPacket cursor:
                    foreach (OsuMatch m in OsuMatches)
                        foreach (OsuClient p in m.LoadedClients)
                            if (p.User.ID == cursor.ID)
                                ShareWithMatchClients(m.MatchInfo, cursor);
                    break;
                case MatchExitPacket exit:
                    foreach (OsuMatch m in OsuMatches)
                        foreach (OsuClient p in m.LoadedClients)
                            if (p.User.ID == exit.User.ID)
                            {
                                restart:
                                foreach (OsuClient r in m.LoadedClients)
                                {
                                    m.LoadedClients.Remove(r);
                                    m.Clients.Add(r);
                                    goto restart;
                                }
                                ShareWithMatchClients(m.MatchInfo, exit);
                                return;
                            }
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();

            restart:
            foreach (OsuMatch match in OsuMatches)
            {
                if (match.MatchInfo.Users.Count == 0 && match.MatchLastUpdateTime + 60000 <= Time.Current)
                {
                    OsuMatches.Remove(match);
                    Logger.Log("Empty match deleted!");
                    goto restart;
                }

                if (match.MatchInfo.Users.Count > 0)
                {
                    match.MatchLastUpdateTime = Time.Current;
                }
            }
        }

        protected void ShareWithMatchClients(MatchListPacket.MatchInfo match, Packet packet)
        {
            foreach (OsuUserInfo user in match.Users)
                NetworkingClient.SendPacket(packet, GetClient(user).EndPoint);
        }

        protected MatchListPacket.MatchInfo GetMatch(OsuUserInfo player)
        {
            foreach (MatchListPacket.MatchInfo m in GetMatches())
                foreach (OsuUserInfo p in m.Users)
                    if (p.ID == player.ID)
                        return m;
            return null;
        }

        protected OsuClient GetClient(OsuUserInfo user)
        {
            foreach (Client c in Clients)
            {
                OsuClient osu = (OsuClient)c;
                if (osu.User.ID == user.ID)
                    return osu;
            }
            return null;
        }

        protected List<MatchListPacket.MatchInfo> GetMatches()
        {
            List<MatchListPacket.MatchInfo> matches = new List<MatchListPacket.MatchInfo>();

            foreach (OsuMatch match in OsuMatches)
                matches.Add(match.MatchInfo);

            return matches;
        }

        protected List<OsuClient> GetOsuClients(OsuMatch serverMatch)
        {
            List<OsuClient> clients = new List<OsuClient>();

            foreach (OsuClient player in serverMatch.Clients)
                clients.Add(player);

            return clients;
        }

        protected class OsuMatch
        {
            public MatchListPacket.MatchInfo MatchInfo;

            public List<OsuClient> Clients = new List<OsuClient>();

            public List<OsuClient> LoadedClients = new List<OsuClient>();

            public double MatchLastUpdateTime;
        }
    }
}
