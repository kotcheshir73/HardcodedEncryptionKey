namespace WebApplicationHardcodedEncrypt.Models
{
    public class DataModel
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public byte[] EncodingText { get; set; }
    }
}