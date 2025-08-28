using Grpc.Core;
using System.Text.RegularExpressions;
using VoiceAssistant.Server.RestClients;

namespace VoiceAssistant.Server.Services
{
	public partial class RegistrationService : Registration.RegistrationBase
	{
		private readonly KeycloakClient _keycloak;

		public RegistrationService(KeycloakClient keycloak)
		{
			_keycloak = keycloak;
		}

		public override async Task<RegistrationReply> Register(
			RegistrationRequest request,
			ServerCallContext context)
		{
			if (!CheckEmail(request.Username))
				return Error("Incorrect email.");

			if (!CheckPassword(request.Password))
				return Error("Incorrect password.");

			if (!CheckClient(request.ClientId))
				return ClientError();

			var emailVerified = true;

			var response = await _keycloak.RegisterAsync(request.Username, request.Password, emailVerified);

			response.Match(userId =>
			{
				if (!emailVerified)
					return SuccessVerifyEmail(userId.Id);
				else
					return Success();
			},
			error =>
			{
				return Error(error.Message);
			});

			return Success();
		}

		private static bool CheckEmail(string email)
		{
			var emailRegex = EmailRegex();
			return emailRegex.IsMatch(email);
		}

		private static bool CheckPassword(string password)
		{
			var passwordRegex = PasswordRegex();
			return passwordRegex.IsMatch(password);
		}

		private static bool CheckClient(string clientId)
		{
			return true;
		}

		#region RegistrationReplies

		private static RegistrationReply Success()
		{
			return new()
			{
				Status = 0,
				Message = string.Empty
			};
		}

		private static RegistrationReply SuccessVerifyEmail(string userId)
		{
			return new()
			{
				Status = 1,
				Message = userId
			};
		}

		private static RegistrationReply Error(string message)
		{
			return new()
			{
				Status = 2,
				Message = message
			};
		}

		private static RegistrationReply ClientError()
		{
			return new()
			{
				Status = 3,
				Message = "Client with such Id already registered."
			};
		}

		#endregion

		#region Regexes

		[GeneratedRegex(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$")]
		private static partial Regex EmailRegex();

		[GeneratedRegex(@"^\S{12,}$")]
		private static partial Regex PasswordRegex();

		#endregion
	}
}
