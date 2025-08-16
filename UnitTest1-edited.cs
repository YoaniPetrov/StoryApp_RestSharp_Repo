using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using StoryApp.Models;


//new GitHubActions workflow test


namespace StoryApp
{
    [TestFixture]
    public class StoryTests
    {
        private RestClient client;
        private static string createdStoryId;
        private static string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("yoanipetrov", "123456abv");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }

        [Test, Order(1)]
        public void CreateNewStory_ShouldReturnSuccessfullyCreated()
        {
            var storyBody = new StoryDTO
            {
                Title = "My First Story Spoiler",
                Description = "This is my first story spoiler so far.",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storyBody);
            var response = client.Execute(request);

            var createdResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(createdResponse.Msg, Is.EqualTo("Successfully created!"));

            createdStoryId = createdResponse.StoryId;

        }

        [Test, Order(2)]
        public void EditStory_ShouldReturnOK()
        {
            var updatedStory = new StoryDTO
            {
                Title = "My first edited story",
                Description = "This is the updated version of my first story.",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            request.AddJsonBody(updatedStory);

            var response = client.Execute(request);

            var createdResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
           
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(createdResponse.Msg, Is.EqualTo("Successfully edited"));

        }

        [Test, Order(3)]
        public void GetAllStories_ShouldReturnOK()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);

            var response = client.Execute(request);

            var allStories = JsonSerializer.Deserialize<List<StoryDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(allStories, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStory_ShouldReturnDeletedSuccessfully()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute(request);

            var createdResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(createdResponse.Msg, Is.EqualTo("Deleted successfully!"));

        }

        [Test, Order(5)]
        public void CreateStoryWithoutRequiredFields_ShouldReturnBadRequest()
        {
            var storyBody = new StoryDTO
            {
                Title = "",
                Description = "",
                
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storyBody);
            var response = client.Execute(request);

            var createdResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }

        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            var nonExistingID = "358";

            var updatedStory = new StoryDTO
            {
                Title = "My first edited story",
                Description = "This is the updated version of my first story.",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{nonExistingID}", Method.Put);
            request.AddJsonBody(updatedStory);

            var response = client.Execute(request);

            var createdResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(createdResponse.Msg, Is.EqualTo("No spoilers..."));

        }

        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            var nonExistingID = "358";

            var request = new RestRequest($"/api/Story/Delete/{nonExistingID}", Method.Delete);
            var response = client.Execute(request);

            var createdResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(createdResponse.Msg, Is.EqualTo("Unable to delete this story spoiler!"));

        }


        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }

}
