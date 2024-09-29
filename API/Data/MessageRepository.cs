using API.Data;
using API.DTOs;
using API.Helpers;
using API.Interfaces;

namespace API;

public class MessageRepository(DataContext context) : IMessageRepository
{
    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await context.Messages.FindAsync(id);
    }

    public Task<PagedList<MessageDto>> GetMessageForUser()
    {
        throw new NotImplementedException();
    }

    public Task<PagedList<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}