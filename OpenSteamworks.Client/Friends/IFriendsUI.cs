using OpenSteamworks.Data.Enums;
using OpenSteamworks.Data.Structs;

namespace OpenSteamworks.Client.Friends;

public interface IFriendsUI {
    public void ShowFriendsList();
    public void ShowChatUI(CSteamID steamid);
}