using API.Interfaces;

namespace API;

public interface IUnitOfWork
{
    IUserRepository UserRepository {get; }
    IMessageRepository MessageRepository {get;}
    ILikesRepository LikesRepository {get;}
    IPhotoRepository PhotoRepository {get;}

    Task<bool> Complete();//if there are changes in the DB
    bool HasChanges();
 }