using OneOf;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using VoiceAssistant.Server.Options;

namespace VoiceAssistant.Server.RestClients
{
	public record UserId(string Id);
	public record Error(HttpStatusCode Status, string Message = "");

	public class KeycloakClient
	{
		private readonly KeycloakOptions _options;
		private readonly HttpClient _httpClient;
		private string? _adminToken;
		private DateTime? _expiresAt;

		public KeycloakClient(HttpClient client, KeycloakOptions options)
		{
			_httpClient = client;
			_options = options;
		}

		public async Task<OneOf<UserId, Error>> RegisterAsync(string email, string password, bool emailVerified = false)
		{
			var token = await GetAdminToken();

			var json = new JsonObject();
			json["username"] = email;
			json["email"] = email;
			json["enabled"] = true;
			json["emailVerified"] = emailVerified;
			json["credentials"] = new JsonObject()
			{
				["type"] = "password",
				["value"] = password,
				["temporary"] = false
			};

			var request = new HttpRequestMessage(HttpMethod.Post, "/admin/realms/Avrora-realm/users")
			{
				Content = JsonContent.Create(json.ToJsonString())
			};
			request.Headers.Authorization = new("Bearer", token);

			var response = await _httpClient.SendAsync(request);

			if (response.IsSuccessStatusCode)
				return GetUserIdFromResponse(response);
			else
				return await GetErrorFromResponse(response);
		}

		public async Task SendEmailVerification(string userId)
		{
			await _httpClient.PutAsync($"/admin/realms/Avrora-realm/users/{userId}/send-verify-email", null);
		}

		private async Task<string> GetAdminToken()
		{
			if(_adminToken is null || DateTime.UtcNow >= _expiresAt)
			{
				var content = new FormUrlEncodedContent(
				[
					new KeyValuePair<string, string>("client_id", _options.AdminId),
					new("client_secret", _options.AdminSecret),
					new("grant_type", "client_credentials"),
				]);

				var response = await _httpClient.PostAsync("/realms/Avrora-realm/protocol/openid-connect/token", content);
				var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
				_adminToken = json.RootElement.GetProperty("access_token").GetString()!;
				var expiresIn = json.RootElement.GetProperty("expires_in").GetInt32();
				_expiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 30);
			}

			return _adminToken;
		}

		private static UserId GetUserIdFromResponse(HttpResponseMessage response)
		{
			var location = response.Headers.GetValues("Location").ElementAt(0);
			var userid = location[(location.LastIndexOf('/') + 1)..];

			return new(userid);
		}

		private static async Task<Error> GetErrorFromResponse(HttpResponseMessage response)
		{
			var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync())!;
			var message = json.RootElement.GetProperty("errorMessage")!.ToString();

			return new(response.StatusCode, message);
		}
	}
}
