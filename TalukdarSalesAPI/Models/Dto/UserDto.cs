namespace TalukdarSalesAPI.Models.Dto
{
    public class UserDto
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string SequencialUserId { get; set; }
        public int UserTypeId { get; set; }
        public string UserTypeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImageName { get; set; }
        public string PhoneNumber { get; set; }
        public decimal DueAmount { get; set; }
        public decimal MaxCreditLimit { get; set; }
        public int MaxCreditDays { get; set; }
        public string Address { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonPhone { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsPayRollUser { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
