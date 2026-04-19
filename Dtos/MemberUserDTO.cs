namespace PawsPort.Dtos
{
    public class MemberUserDTO
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Photo { get; set; } 

        public string Job { get; set; }

        public string Phone { get; set; }

        public DateOnly? Birthday { get; set; }

        public string City { get; set; }

        public int? Point { get; set; }

        public string Note { get; set; }

        public bool? HasPriorExp { get; set; }

        public bool Status { get; set; }

        public bool? IsSubscribe { get; set; }

        public bool? IsVerify { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

    }
}
