namespace PawsPort.Dtos
{
    public class ArticleListDTO
    {
        //===隱藏===
        public int ArticleId { get; set; }
        public int CategoryId { get; set; }

        //===條件顯示===
        public DateTime? EventStartDate { get; set; }

        public DateTime? EventEndDate { get; set; }

        public string EventLocation { get; set; }

        //===顯示===
        public string UserName  { get; set; }

        public string CategoryName { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime? LastEditTime { get; set; }

        public int Status { get; set; }

        public int ViewCount { get; set; }

        public string MainImageUrl { get; set; }

        public List<string> TagNames { get; set; }
    }
}
