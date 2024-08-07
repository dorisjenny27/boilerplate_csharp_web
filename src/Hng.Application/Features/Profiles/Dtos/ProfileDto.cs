namespace Hng.Application.Features.Profiles.Dtos
{
    public class ProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string AvatarUrl { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Pronoun { get; set; }
        public string JobTitle { get; set; }
        public string Bio { get; set; }
        public string FacebookLink { get; set; }
        public string TwitterLink { get; set; }
        public string LinkedinLink { get; set; }
    }
}