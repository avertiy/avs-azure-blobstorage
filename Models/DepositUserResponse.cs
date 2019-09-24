namespace BlobStorageWebApp.Models
{
    public class DepositUserResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DOB { get; set; }
        public bool SSNRecorded { get; set; } = true;

    }
}