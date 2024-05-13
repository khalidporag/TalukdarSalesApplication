namespace TalukdarSalesAPI.Models
{
    public interface IEntityBase
    {
        int Id { get; set; }
        DateTime CreatedOn { get; set; }
        DateTime? ModifiedOn { get; set; }
        bool IsDeleted { get; set; }
        DateTime? DeletedOn { get; set; }
    }
}
