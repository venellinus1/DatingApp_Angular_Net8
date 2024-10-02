using SQLitePCL;

namespace API;

public class PresenceTracker
{
    //just for a demo - use a dictionary to store the connected users - id, list of connections
    //!! this is not thread -safe
    private static readonly Dictionary<string, List<string>> OnlineUsers = [];
    public Task<bool> UserConnected(string username, string connectionId)
    {
        var isOnline = false;
        lock(OnlineUsers)
        {
            if (OnlineUsers.ContainsKey(username))
            {
                OnlineUsers[username].Add(connectionId);
            }
            else 
            {
                OnlineUsers.Add(username, [connectionId]);// [connectionId] = C# 8 feature = new List<string>{connectionId}
                isOnline = true;
            }
        }
        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string username, string connectionId)
    {
        var isOffline = false;
        lock(OnlineUsers)
        {
            if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline); // user not in the dictionary - nothing to do here

            OnlineUsers[username].Remove(connectionId);

            if (OnlineUsers[username].Count == 0)// no more connections in the dict - remove the key for that user
            {
                OnlineUsers.Remove(username); 
                isOffline = true;
            }
        }
        return Task.FromResult(isOffline); 
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] onlineUsers;
        lock (OnlineUsers)
        {
            onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
        }
        return Task.FromResult(onlineUsers); 
    }

    public static Task<List<string>> GetConnectionsForUser(string username)
    {
        List<string> connectionIds;
        if (OnlineUsers.TryGetValue(username, out var connections))
        {
            lock (connections)
            {
                connectionIds = connections.ToList();
            }
        }
        else
        {
            connectionIds = [];
        }

        return Task.FromResult(connectionIds);
    }
}