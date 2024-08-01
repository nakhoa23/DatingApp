using API.Interfaces;
using SQLitePCL;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly DataContext _context;

        public UnitOfWork(DataContext context, IMessageRepository messageRepository, ILikesRepository likesRepository, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _likesRepository = likesRepository; 
            _messageRepository = messageRepository; 
            _context = context;
        }

        public IUserRepository UserRepository => _userRepository;

        public ILikesRepository LikesRepository => _likesRepository;

        public IMessageRepository MessageRepository => _messageRepository;

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }

        
    }
}
