﻿#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class GameRichPresenceJoinRequestedEvent : UnityEvent<UserData, string> { }
}
#endif