using System.ComponentModel.DataAnnotations;

namespace PawsPort.Dtos
{
    public class CategorySaveDTO
    {

        [Required]
        public string CategoryName { get; set; }
        
        public string CategoryDescription { get; set; }
        
        public int? ParentId { get; set; }

        
        public int? Level { get; set; }
     
        public int? SortOrder { get; set; }
       
       
      
    }
}
