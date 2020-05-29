using Cyc.Standard;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using File = Google.Apis.Drive.v3.Data.File;
namespace Cyc.GoogleApi {
	public class GoogleManager {
		private class Timeouts {
			public static readonly TimeSpan Silent = TimeSpan.FromSeconds(10);
			public static readonly TimeSpan Interactive = TimeSpan.FromMinutes(1);
		}
		private readonly ILogger logger;
		private readonly Dictionary<string, (DriveService driveService, UserCredential userCredential)> userRegistry = new Dictionary<string, (DriveService, UserCredential)>();
		public string[] Scopes { get; set; } = new[] { DriveService.Scope.Drive };
		public string ClientSecretsPath { get; set; } = @"GoogleApi\client_secret.json";
		public string DataStorePath { get; private set; } = GoogleWebAuthorizationBroker.Folder;
		public bool HasUser(string userId) {
			return userRegistry.ContainsKey(userId);
		}
		public GoogleManager(ILogger logger, string clientSecretsPath = null, string[] scopes = null, string dataStorePath = null) {
			ClientSecretsPath = clientSecretsPath ?? ClientSecretsPath;
			Scopes = scopes ?? Scopes;
			DataStorePath = dataStorePath ?? dataStorePath;
			this.logger = logger;
		}
		public async Task<About> GetAboutAsync(string userId) {
			if (!userRegistry.ContainsKey(userId)) {
				logger.Log("User has not been registered.");
				return null;
			}
			var request = userRegistry[userId].driveService.About.Get();
			request.Fields = "user";
			return await request.ExecuteAsync().ConfigureAwait(false);
		}
		public async Task<File> GetDriveRootAsync(string userId) {
			if (!userRegistry.ContainsKey(userId)) {
				logger.Log("User has not been registered.");
				return null;
			}
			var root = await userRegistry[userId].driveService.Files.Get("root").ExecuteAsync().ConfigureAwait(false);
			return root;
		}
		public async IAsyncEnumerable<File> GetChildrenAsync(string userId, string id) {
			if (!userRegistry.ContainsKey(userId)) {
				logger.Log("User has not been registered.");
				yield break;
			}
			var request = userRegistry[userId].driveService.Files.List();
			request.Q = $"parents in '{id}' and trashed = false";
			do {
				var fileList = await request.ExecuteAsync().ConfigureAwait(false);
				foreach (var file in fileList.Files) {
					yield return file;
				}
				request.PageToken = fileList.NextPageToken;
			} while (!string.IsNullOrEmpty(request.PageToken));
		}
		public async Task<string> UserLoginAsync() {
			var userId = Guid.NewGuid().ToString();
			using var stream = new FileStream(ClientSecretsPath, FileMode.Open, FileAccess.Read);
			try {
				using var cts = new CancellationTokenSource(Timeouts.Interactive);
				var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
					GoogleClientSecrets.Load(stream).Secrets,
					Scopes,
					userId,
					cts.Token,
					new FileDataStore(DataStorePath)).ConfigureAwait(false);
				RegisterUser(userId, credential);
			} catch (OperationCanceledException) {
				//logger.Log(ex); // do not show message, let the task expires automatically
				return null;
			}
			return userId;
		}
		public async Task<string> UserLoginAsync(string userId) {
			using var stream = new FileStream(ClientSecretsPath, FileMode.Open, FileAccess.Read);
			try {
				using var cts = new CancellationTokenSource(Timeouts.Interactive);
				var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
					GoogleClientSecrets.Load(stream).Secrets,
					Scopes,
					userId,
					cts.Token,
					new FileDataStore(DataStorePath)).ConfigureAwait(false);
				RegisterUser(userId, credential);
			} catch (OperationCanceledException ex) {
				logger.Log(ex);
				return null;
			}
			return userId;
		}
		public IEnumerable<string> LoadAllUserId() {
			var datastore = new FileDataStore(DataStorePath);
			var filepaths = Directory.GetFiles(datastore.FolderPath);
			return filepaths.Select(path => string.Concat(path.SkipWhile(c => c != '-')).Remove(0, 1));
		}
		public async Task<bool> UserLogoutAsync(string userId) {
			if (!userRegistry.ContainsKey(userId)) {
				logger.Log("User has not been registered.");
				return false;
			}
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var result = await userRegistry[userId].userCredential.RevokeTokenAsync(cts.Token).ConfigureAwait(false);
			if (result == true) {
				userRegistry.Remove(userId);
			}
			return result;
		}
		private void RegisterUser(string userId, UserCredential credential) {
			var service = new DriveService(new BaseClientService.Initializer
			{
				HttpClientInitializer = credential,
				ApplicationName = Assembly.GetExecutingAssembly().GetName().Name,
			});
			if (!userRegistry.ContainsKey(userId)) {
				userRegistry.Add(userId, (service, credential));
			} else {
				userRegistry[userId].driveService.Dispose(); // dispose old service
				userRegistry[userId] = (service, credential);
			}
		}
		[Obsolete]
		private async Task<UserCredential> GetUserCredentialInteractivelyAsync(string path, IEnumerable<string> scopes) {
			var app = GetAuthorizationCodeInstalledApp(path, scopes);
			var redirectUri = app.CodeReceiver.RedirectUri;
			var codeRequest = app.Flow.CreateAuthorizationCodeRequest(redirectUri);
			var response = await app.CodeReceiver.ReceiveCodeAsync(codeRequest, CancellationToken.None)
				.ConfigureAwait(false);
			if (string.IsNullOrEmpty(response.Code)) {
				var errorResponse = new TokenErrorResponse(response);
				logger.Log($"Received an error. The response is: {errorResponse}");
				throw new TokenResponseException(errorResponse);
			}
			var token = await app.Flow.ExchangeCodeForTokenAsync("user", response.Code, redirectUri,
				CancellationToken.None).ConfigureAwait(false);
			return new UserCredential(app.Flow, "user", token);
		}
		[Obsolete]
		private async Task<UserCredential> UserLoginSilently(string userId) {
			var app = GetAuthorizationCodeInstalledApp(ClientSecretsPath, Scopes);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var token = await app.Flow.LoadTokenAsync(userId, cts.Token).ConfigureAwait(false);
			if (app.ShouldRequestAuthorizationCode(token)) {
				return null;
			}
			return new UserCredential(app.Flow, userId, token);
		}
		[Obsolete]
		private static AuthorizationCodeInstalledApp GetAuthorizationCodeInstalledApp(string path, IEnumerable<string> scopes) {
			var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
			var initializer = new GoogleAuthorizationCodeFlow.Initializer
			{
				ClientSecrets = GoogleClientSecrets.Load(stream).Secrets,
				Scopes = scopes,
				DataStore = new FileDataStore(GoogleWebAuthorizationBroker.Folder),
			};
			var flow = new GoogleAuthorizationCodeFlow(initializer);
			var codeReceiver = new LocalServerCodeReceiver();
			var app = new AuthorizationCodeInstalledApp(flow, codeReceiver);
			return app;
		}
	}
}