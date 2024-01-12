namespace socialmediaAPI.RequestsResponses.Requests
{
    public class UpdatePostRequest
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public List<string?> deleteUrls { get; set; }
        public Dictionary<string, string?> keepUrls { get; set; }
        public List<IFormFile>? Files { get; set; }

        public UpdatePostRequest()
        {
            Id = string.Empty; Content = string.Empty;
            deleteUrls = new List<string?>();
            keepUrls = new Dictionary<string, string?>();
        }

    }
}
