using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DetleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message?> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            // orderbyDescending: return các phần tử được sắp xếp giảm dần
            var query = _context.Messages
                .OrderByDescending(x => x.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(x => x.Recipient.UserName == messageParams.Username && x.RecipientDeleted == false),
                "Outbox" => query.Where(x => x.Sender.UserName == messageParams.Username && x.SenderDeleted == false),
                _ => query.Where(x => x.Recipient.UserName == messageParams.Username && x.DateRead == null && x.RecipientDeleted == false)
            };
            var message = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
            return await PagedList<MessageDto>.CreateAsync(message, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await _context.Messages
                .Include(x=> x.Sender).ThenInclude(x => x.Photos)
                .Include(x=>x.Recipient).ThenInclude(x=>x.Photos)
                .Where(x=> 
                x.RecipientUsername == currentUsername && x.RecipientDeleted == false && x.SenderUsername == recipientUsername || // người nhận là mình && người gửi là người nhận
                x.SenderUsername == currentUsername && x.SenderDeleted == false && x.RecipientUsername == recipientUsername) // người gửi là mình && người nhận là người nhận
                .OrderBy(x=>x.MessageSent).ToListAsync();

            var unreadMessages = messages.Where(x=> x.DateRead == null && x.RecipientUsername == currentUsername).ToList();

            if(unreadMessages.Count != 0)
            {
                unreadMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
                await _context.SaveChangesAsync(); 
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
           return await _context.SaveChangesAsync() > 0;
        }
    }
}
