using Cyc.Standard;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Download;
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
	public class GoogleApiManager {
		public class Timeouts {
			public static readonly TimeSpan Silent = TimeSpan.FromSeconds(10);
			public static readonly TimeSpan Interactive = TimeSpan.FromMinutes(1);
		}
		private readonly ILogger logger;
		private readonly Dictionary<string, (DriveService driveService, UserCredential userCredential)> userRegistry = new Dictionary<string, (DriveService, UserCredential)>();

		public event EventHandler BeforeTaskExecute;
		public event EventHandler TaskExecuted;

		public string[] Scopes { get; set; } = new[] { DriveService.Scope.Drive };
		public string ClientSecretsPath { get; set; } = @"GoogleApi\client_secret.json";
		public string DataStorePath { get; private set; } = GoogleWebAuthorizationBroker.Folder;
		public bool HasUser(string userId)
		{
			return userRegistry.ContainsKey(userId);
		}
		public GoogleApiManager(ILogger logger, string clientSecretsPath = null, string[] scopes = null, string dataStorePath = null)
		{
			ClientSecretsPath = clientSecretsPath ?? ClientSecretsPath;
			Scopes = scopes ?? Scopes;
			DataStorePath = dataStorePath ?? DataStorePath;
			this.logger = logger;
		}
		public async Task<About> GetAboutAsync(string userId)
		{
			if (!userRegistry.ContainsKey(userId)) {
				logger?.Log("User has not been registered.");
				return null;
			}
			BeforeTaskExecute?.Invoke(this, null);
			var request = userRegistry[userId].driveService.About.Get();
			request.Fields = "user";
			try {
				var about = await request.ExecuteAsync().ConfigureAwait(false);
				return about;

			} catch (TokenResponseException ex) {
				logger?.Log(ex);
				return null;
			} finally {
				TaskExecuted?.Invoke(this, null);
			}
		}
		public async Task<File> GetDriveRootAsync(string userId)
		{
			if (!userRegistry.ContainsKey(userId)) {
				logger?.Log("User has not been registered.");
				return null;
			}
			BeforeTaskExecute?.Invoke(this, null);
			var request = userRegistry[userId].driveService.Files.Get("root");
			request.Fields = "*";
			try {
				var root = await request.ExecuteAsync().ConfigureAwait(false);
				return root;

			} catch (TokenResponseException ex) {
				logger?.Log(ex);
				return null;
			} finally {
				TaskExecuted?.Invoke(this, null);

			}
		}
		public async IAsyncEnumerable<File> GetChildrenAsync(string userId, string id)
		{
			if (!userRegistry.ContainsKey(userId)) {
				logger?.Log("User has not been registered.");
				yield break;
			}
			BeforeTaskExecute?.Invoke(this, null);
			var request = userRegistry[userId].driveService.Files.List();
			request.Q = $"parents in '{id}' and trashed = false";
			request.Fields = "*";
			FileList fileList;
			do {

				try {
					fileList = await request.ExecuteAsync().ConfigureAwait(false);

				} catch (TokenResponseException ex) {
					logger?.Log(ex);
					yield break;
				} finally {
					TaskExecuted?.Invoke(this, null);

				}
				foreach (var file in fileList.Files) {
					yield return file;
				}
				request.PageToken = fileList.NextPageToken;
			} while (!string.IsNullOrEmpty(request.PageToken));
			TaskExecuted?.Invoke(this, null);
		}
		public async Task DownloadAsync(string userId, string fileId, string localPath, Action<IDownloadProgress> progressChanged = null)
		{
			if (!userRegistry.ContainsKey(userId)) {
				logger?.Log("User has not been registered.");
				return;
			}
			BeforeTaskExecute?.Invoke(this, null);
			var request = userRegistry[userId].driveService.Files.Get(fileId);
			request.MediaDownloader.ProgressChanged += progressChanged;
			using var stream = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.Write);
			try {
				await request.DownloadAsync(stream).ConfigureAwait(false);
			} catch (TokenResponseException ex) {
				logger?.Log(ex);
			} finally {
				TaskExecuted?.Invoke(this, null);
			}
		}
		public async Task<string> UserLoginAsync()
		{
			BeforeTaskExecute?.Invoke(this, null);
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
				return userId;
			} catch (TokenResponseException ex) {
				logger?.Log(ex);
				return null;
			} catch (OperationCanceledException) {
				//logger?.Log(ex); // do not show message, let the task expires automatically
				return null;
			} finally {
				TaskExecuted?.Invoke(this, null);
			}
		}
		public async Task<string> UserLoginAsync(CancellationToken token)
		{
			BeforeTaskExecute?.Invoke(this, null);
			var userId = Guid.NewGuid().ToString();
			using var stream = new FileStream(ClientSecretsPath, FileMode.Open, FileAccess.Read);
			try {
				var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
					GoogleClientSecrets.Load(stream).Secrets,
					Scopes,
					userId,
					token,
					new FileDataStore(DataStorePath)).ConfigureAwait(false);
				RegisterUser(userId, credential);
				return userId;
			} catch (TokenResponseException ex) {
				logger?.Log(ex);
				return null;
			} catch (OperationCanceledException) {
				//logger?.Log(ex); // do not show message, let the task expires automatically
				return null;
			} finally {
				TaskExecuted?.Invoke(this, null);
			}
		}
		public async Task<string> UserLoginAsync(string userId)
		{
			BeforeTaskExecute?.Invoke(this, null);
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
				return userId;
			} catch (TokenResponseException ex) {
				logger?.Log(ex);
				return null;
			} catch (OperationCanceledException ex) {
				logger?.Log(ex);
				return null;
			} finally {
				TaskExecuted?.Invoke(this, null);
			}
		}
		public IEnumerable<string> LoadAllUserId()
		{
			var datastore = new FileDataStore(DataStorePath);
			var filepaths = Directory.GetFiles(datastore.FolderPath);
			return filepaths.Select(path => string.Concat(path.SkipWhile(c => c != '-')).Remove(0, 1));
		}
		public async Task<bool> UserLogoutAsync(string userId)
		{
			if (!userRegistry.ContainsKey(userId)) {
				logger?.Log("User has not been registered.");
				return false;
			}
			BeforeTaskExecute?.Invoke(this, null);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var result = await userRegistry[userId].userCredential.RevokeTokenAsync(cts.Token).ConfigureAwait(false);
			if (result == true) {
				userRegistry.Remove(userId);
			}
			TaskExecuted?.Invoke(this, null);
			return result;
		}
		private void RegisterUser(string userId, UserCredential credential)
		{
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
		private async Task<UserCredential> GetUserCredentialInteractivelyAsync(string path, IEnumerable<string> scopes)
		{
			var app = GetAuthorizationCodeInstalledApp(path, scopes);
			var redirectUri = app.CodeReceiver.RedirectUri;
			var codeRequest = app.Flow.CreateAuthorizationCodeRequest(redirectUri);
			var response = await app.CodeReceiver.ReceiveCodeAsync(codeRequest, CancellationToken.None)
				.ConfigureAwait(false);
			if (string.IsNullOrEmpty(response.Code)) {
				var errorResponse = new TokenErrorResponse(response);
				logger?.Log($"Received an error. The response is: {errorResponse}");
				throw new TokenResponseException(errorResponse);
			}
			var token = await app.Flow.ExchangeCodeForTokenAsync("user", response.Code, redirectUri,
				CancellationToken.None).ConfigureAwait(false);
			return new UserCredential(app.Flow, "user", token);
		}
		[Obsolete]
		private async Task<UserCredential> UserLoginSilently(string userId)
		{
			var app = GetAuthorizationCodeInstalledApp(ClientSecretsPath, Scopes);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var token = await app.Flow.LoadTokenAsync(userId, cts.Token).ConfigureAwait(false);
			if (app.ShouldRequestAuthorizationCode(token)) {
				return null;
			}
			return new UserCredential(app.Flow, userId, token);
		}
		[Obsolete]
		private static AuthorizationCodeInstalledApp GetAuthorizationCodeInstalledApp(string path, IEnumerable<string> scopes)
		{
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