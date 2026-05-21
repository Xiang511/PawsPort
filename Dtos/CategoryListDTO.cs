namespace PawsPort.Dtos
{
    public class CategoryListDTO
    {
        public int CategoryId { get; set; }
        public int? ParentId { get; set; }
        public int? Level { get; set; }
        public string ParentCategoryName { get; set; }
        public string CategoryName { get; set; }
    }
}
