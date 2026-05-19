namespace PawsPort.Dtos
{
    public class LocationInfoDTO
    {
        public string GeonameId { get; set; }
        public string Capital { get; set; }
        public List<LanguageDTO> Languages { get; set; }
        public string CountryFlag { get; set; }
        public string CountryFlagEmoji { get; set; }
        public string CountryFlagEmojiUnicode { get; set; }
        public string CallingCode { get; set; }
        public bool IsEu { get; set; }
    }
}