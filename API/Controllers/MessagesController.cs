using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private IMapper _mapper;
        public MessagesController(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper)
        {
            _mapper = mapper;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("can not sent message yourself");

            var sender = await _userRepository.GetUserByUserNameAsync(username);
            var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);

            if (sender == null || recipient == null
               || sender.UserName == null || recipient.UserName == null) return BadRequest("Can not send message at this time");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to saved message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages);

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();
            return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();

            var message = await _messageRepository.GetMessage(id);

            if (message == null) return BadRequest("Can not delete this message");

            if (message.SenderUsername != username && message.RecipientUsername != username) return Forbid();

            if (message.SenderUsername == username) message.SenderDeleted = true;
            if(message.RecipientUsername ==  username) message.RecipientDeleted = true;


            //if(message.SenderDeleted == true && message.RecipientDeleted == true)

            if (message is { SenderDeleted: true, RecipientDeleted: true })
            {
                _messageRepository.DetleteMessage(message);
            }

            if (await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}
