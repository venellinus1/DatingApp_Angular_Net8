using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.SignalR;
using API.DTOs;
using AutoMapper;
using API.Entities;
using System.Security.Cryptography.X509Certificates;
using System.Data.SqlTypes;

namespace API.SignalR;

public class MessageHub(IUnitOfWork unitOfWork, 
    IMapper mapper, IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();// its WebSocket connection but its started with http request
        var otherUser = httpContext?.Request.Query["user"];

        if (Context.User == null && string.IsNullOrEmpty(otherUser)) throw new Exception("Cannot join group");        
        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroup(groupName);// allow clients to check who is in the group at any time, so that when we mark a message as read we can check to make sure that the other user is in that group before doing so
        
        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);
        
        var messages = await unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser!);
        
        if (unitOfWork.HasChanges()) await unitOfWork.Complete();//complete transation here instead of in the Repository - so far this was in MessageRepository GetMessageThread
        
        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }
 
    public async Task SendMessage(CreateMessageDto createMessageDto) // careful - this method name will be used on the Client side 
    {
        var username = Context.User?.GetUsername() ?? throw new Exception("Could not get user");
        if (username == createMessageDto.RecipientUsername.ToLower()) 
            throw new HubException("You cannot send message to yourself");

        var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (sender == null || recipient == null || sender.UserName == null || recipient.UserName==null) 
            throw new HubException("Cannot send message at this time");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content,
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);

        //check if the recipient is in the group chat
        if (group != null && group.Connections.Any(x => x.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else//notify user that they have a message if they are connected to the app
        {
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            if (connections != null && connections?.Count != null)
            {
                await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", 
                    new { username = sender.UserName,  knownAs = sender.KnownAs});
            }
        }

        unitOfWork.MessageRepository.AddMessage(message);

        if (await unitOfWork.Complete())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var group = await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
        await base.OnDisconnectedAsync(exception);
    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var username = Context.User?.GetUsername() ?? throw new Exception("Cannot get username");

        var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
        var connection = new Connection{ ConnectionId = Context.ConnectionId, Username = username};
        if (group == null)
        {
            group = new Group{Name = groupName};
            unitOfWork.MessageRepository.AddGroup(group);            
        }
        group.Connections.Add(connection);
        if (await unitOfWork.Complete()) return group;
        
        throw new HubException("Failed to join group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
        var group = await unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

        if (connection != null && group != null)
        {
            unitOfWork.MessageRepository.RemoveConnection(connection);
            if (await unitOfWork.Complete()) return group;
        }
        throw new Exception("Failed to remove from group");
    }
    private string GetGroupName(string caller, string? other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}