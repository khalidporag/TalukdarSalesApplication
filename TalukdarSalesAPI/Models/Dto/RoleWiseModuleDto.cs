namespace TalukdarSalesAPI.Models.Dto
{
    public class RoleWiseModuleDto
    {
        public int RoleId { get; set; }
        public List<int> ModuleIds { get; set; }
        public string Modules { get; set; }
    }
}
