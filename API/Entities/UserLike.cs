namespace API.Entities
{
    public class UserLike
    {
        public AppUser SourceUser { get; set; } = null!; // lượt thích user thích ng` khác
        public int SourceUserId { get; set; }
        public AppUser TargetUser { get; set; } = null!; // lượt thích user nhận được
        public int TargetUserId { get; set; }
    }
}
