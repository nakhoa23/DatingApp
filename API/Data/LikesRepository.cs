using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public LikesRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddLike(UserLike like)
        {
            _context.Likes.Add(like);
        }

        public void DeleteLike(UserLike like)
        {
            _context.Likes.Remove(like);
        }

        // get userId những user mình(currentUser) đang thích
        public async Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId)
        {
            return await _context.Likes
                .Where(x => x.SourceUserId == currentUserId)
                .Select(x => x.TargetUserId)
                .ToListAsync();
        }

        // get by id (2 fk)
        public async Task<UserLike?> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        public async Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams)
        {
            var likes = _context.Likes.AsQueryable();
            IQueryable<MemberDto> query;
            switch (likesParams.Predicate)
            {
                // get những user mà mình đã thích
                case "liked":
                    query = likes
                        .Where(x => x.SourceUserId == likesParams.UserId)
                        .Select(x => x.TargetUser)
                        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider);
                    break;
                
                // get những user mà đã thích mình
                case "likedBy":
                    query = likes
                        .Where(x => x.TargetUserId == likesParams.UserId)
                        .Select(x => x.SourceUser)
                        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider);
                    break;
                // get user mà cả 2 cùng thích nhau
                default: 
                    var likeIds = await GetCurrentUserLikeIds(likesParams.UserId); // id những user mình đang thích

                    query = likes
                         .Where(x => x.TargetUserId == likesParams.UserId && likeIds.Contains(x.SourceUserId))
                         .Select(x => x.SourceUser)
                         .ProjectTo<MemberDto>(_mapper.ConfigurationProvider);
                    break;
            }
            return await PagedList<MemberDto>.CreateAsync(query, likesParams.PageNumber, likesParams.PageSize);
        }

    }
}
