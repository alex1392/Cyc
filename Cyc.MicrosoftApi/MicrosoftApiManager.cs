
using MLog = Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Cyc.MicrosoftApi {

	/// <summary>
	/// Handles all graph api calling, includes error handling. If there's any error occurs in the api call, returns null
	/// </summary>
	public class MicrosoftApiManager : IAuthenticationProvider {

		#region Public Classes

		public static class Authority {

			#region Public Fields

			public const string Common = "https://login.microsoftonline.com/common";
			public const string Comsumers = "https://login.microsoftonline.com/comsumers";
			public const string Organizations = "https://login.microsoftonline.com/organizations";

			#endregion Public Fields
		}

		public static class Permissions {

			#region Public Classes

			public static class Files {

				#region Public Fields

				public const string Read = "Files.Read";
				public const string ReadAll = "Files.Read.All";
				public const string ReadWrite = "Files.ReadWrite";
				public const string ReadWriteAll = "Files.ReadWrite.All";

				#endregion Public Fields
			}

			public static class User {

				#region Public Fields

				public const string Read = "User.Read";
				public const string ReadAll = "User.Read.All";
				public const string ReadWrite = "User.ReadWrite";
				public const string ReadWriteAll = "User.ReadWrite.All";

				#endregion Public Fields
			}

			#endregion Public Classes
		}

		public static class RedirectUrl {

			#region Public Fields

			public const string LocalHost = "http://localhost";
			public const string NativeClient = "https://login.microsoftonline.com/common/oauth2/nativeclient";

			#endregion Public Fields
		}

		public static class Selects {

			#region Public Fields

			/// <summary>
			/// Not working
			/// </summary>
			public const string content = "content";

			public const string createdDateTime = "createdDateTime";

			/// <summary>
			/// Not working
			/// </summary>
			public const string downloadUrl = "@microsoft.graph.downloadUrl";

			public const string id = "id";
			public const string name = "name";
			public const string size = "size";
			public const string webUrl = "webUrl";

			#endregion Public Fields
		}

		#endregion Public Classes

		#region Private Classes

		private static class Timeouts {

			#region Public Fields

			public static readonly TimeSpan Silent = TimeSpan.FromSeconds(10);

			#endregion Public Fields
		}

		#endregion Private Classes

		#region Public Fields

		public const string ApiEndpoint = "https://graph.microsoft.com/v1.0/";

		#endregion Public Fields

		#region Private Fields

		private readonly List<IAccount> accountList = new List<IAccount>();
		private readonly GraphServiceClient graphClient;
		private readonly MLog::ILogger logger;
		private readonly IPublicClientApplication msalClient;
		private string appId;
		private string password;
		private string[] scopes;
		private string username;

		#endregion Private Fields

		#region Public Events

		public event EventHandler BeforeTaskExecuted;

		public event EventHandler TaskExecuted;

		#endregion Public Events

		#region Public Constructors

		public MicrosoftApiManager(MLog::ILogger logger = null, string authority = Authority.Common)
		{
			graphClient = new GraphServiceClient(this);
			this.logger = logger;

			AppConfig();

			msalClient = PublicClientApplicationBuilder
				.Create(appId)
				.WithBroker(true)
				.WithAuthority(authority)
				.WithDefaultRedirectUri()
				.Build();
			TokenCacheHelper.EnableSerialization(msalClient.UserTokenCache);
		}

		#endregion Public Constructors

		#region Public Methods

		/// <summary>
		/// Implementation of <see cref="IAuthenticationProvider"/>. This method is called everytime when <see cref="MicrosoftApiManager"/> make a request.
		/// </summary>
		public async Task AuthenticateRequestAsync(HttpRequestMessage request)
		{
			var url = request.RequestUri.ToString().ToLower();
			if (!url.Contains("users")) {
				throw new InvalidOperationException("The request doesn't specify a user");
			}
			var userId = GetUserId(url);
			var userAccount = GetAccount(userId);
			var token = await GetAccessTokenSilently(userAccount).ConfigureAwait(false);
			if (token == null) {
				throw new Exception("Cannot get access token");
			}
			// attach authentication to the header of http request
			request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
		}

		public async IAsyncEnumerable<DriveItem> GetChildrenAsync(IAccount account, string parentId)
		{
			BeforeTaskExecuted?.Invoke(this, null);
			var userId = GetUserId(account);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var request = graphClient.Users[userId].Drive.Items[parentId].Children.Request();
			do {
				IDriveItemChildrenCollectionPage page;
				try {
					page = await request.GetAsync(cts.Token).ConfigureAwait(false);
				} catch (TaskCanceledException ex) {
					logger?.LogError(ex, "Use canncel the task.");
					TaskExecuted?.Invoke(this, null);
					yield break;
				} catch (ServiceException ex) {
					logger?.LogError(ex, "onedrive server error.");
					TaskExecuted?.Invoke(this, null);
					yield break;
				}
				foreach (var file in page) {
					yield return file;
				}
				request = page?.NextPageRequest;
			} while (request != null);
			TaskExecuted?.Invoke(this, null);
		}

		public async Task<DriveItem> GetDriveRootAsync(IAccount account)
		{
			BeforeTaskExecuted?.Invoke(this, null);
			var userId = GetUserId(account);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				return await graphClient.Users[userId].Drive.Root.Request().GetAsync(cts.Token).ConfigureAwait(false);
			} catch (ServiceException ex) {
				logger?.LogError(ex, "onedrive server error");
				return null;
			} finally {
				TaskExecuted?.Invoke(this, null);
			}
		}

		public async Task<Stream> GetFileContentAsync(IAccount account, string fileId)
		{
			BeforeTaskExecuted?.Invoke(this, null);
			var userId = GetUserId(account);
			var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				return await graphClient.Users[userId].Drive.Items[fileId].Content.Request().GetAsync(cts.Token).ConfigureAwait(false);
			} catch (ServiceException ex) {
				logger?.LogError(ex, "onedrive server error");
				return null;
			} finally {
				TaskExecuted?.Invoke(this, null);
			}
		}

		public async Task<User> GetUserAsync(IAccount account)
		{
			BeforeTaskExecuted?.Invoke(this, null);
			var userId = GetUserId(account);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			try {
				var user = await graphClient.Users[userId].Request().GetAsync(cts.Token).ConfigureAwait(false);
				return user;
			} catch (ServiceException ex) {
				logger?.LogError(ex, "onedrive server error");
				return null;
			} finally {
				TaskExecuted?.Invoke(this, null);
			}
		}

		public bool HasAccount(IAccount account)
		{
			return accountList.Any(a => a.HomeAccountId.Identifier == account.HomeAccountId.Identifier);
		}

		public async IAsyncEnumerable<AuthenticationResult> LoginAllUserSilently()
		{
			BeforeTaskExecuted?.Invoke(this, null);
			var accounts = await msalClient.GetAccountsAsync().ConfigureAwait(false);
			foreach (var account in accounts) {
				AuthenticationResult result;
				try {
					using var cts = new CancellationTokenSource(Timeouts.Silent);
					result = await msalClient.AcquireTokenSilent(scopes, account)
												 .ExecuteAsync(cts.Token)
												 .ConfigureAwait(false);

					RegisterUser(result?.Account);
				} catch (Exception ex) {
					logger?.LogError(ex, "onedrive server error");
					continue;
				}
				yield return result;
			}
			TaskExecuted?.Invoke(this, null);
		}

		public async Task<AuthenticationResult> LoginInteractively(CancellationToken token)
		{
			BeforeTaskExecuted?.Invoke(this, null);
			var requestBuilder = msalClient.AcquireTokenInteractive(scopes);
			try {
				var result = await requestBuilder
					.ExecuteAsync(token)
					.ConfigureAwait(false);
				RegisterUser(result?.Account);
				return result;
			} catch (MsalClientException) {
				logger?.LogInformation("User cancelled.");
			} catch (MsalException ex) {
				logger?.LogWarning(ex, "Masl exception.");
			} catch (InvalidOperationException ex) {
				logger?.LogError(ex, "");
			}
			TaskExecuted?.Invoke(this, null);
			return null;
		}

		public async Task<AuthenticationResult> LoginInteractively(IAccount account = null, string claims = null)
		{
			BeforeTaskExecuted?.Invoke(this, null);
			var requestBuilder = msalClient.AcquireTokenInteractive(scopes);
			if (claims != null) {
				requestBuilder = requestBuilder.WithClaims(claims);
			}
			if (account != null) {
				requestBuilder = requestBuilder.WithAccount(account);
			}
			try {
				var result = await requestBuilder
					.ExecuteAsync()
					.ConfigureAwait(false);
				RegisterUser(result?.Account);
				return result;
			}
			catch (MsalClientException)
			{
				logger?.LogInformation("User cancelled.");
			}
			catch (MsalException ex)
			{
				logger?.LogWarning(ex, "Masl exception.");
			}
			catch (InvalidOperationException ex)
			{
				logger?.LogError(ex, "");
			}
			TaskExecuted?.Invoke(this, null);
			return null;
		}

		/// <summary>
		/// This method can only be used with <see cref="Authority.Organizations"/>
		/// Only used in test.
		/// </summary>
		public async Task<AuthenticationResult> LoginWithUsernamePassword(string username = null, string password = null)
		{
			this.username = username ?? this.username;
			this.password = password ?? this.password;
			if (this.username == null || this.password == null) {
				return null;
			}
			BeforeTaskExecuted?.Invoke(this, null);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			var secureString = new SecureString();
			foreach (var c in this.password ?? "") {
				secureString.AppendChar(c);
			}
			var result = await msalClient
				.AcquireTokenByUsernamePassword(scopes, this.username, secureString)
				.ExecuteAsync(cts.Token).ConfigureAwait(false);
			var account = result?.Account;
			RegisterUser(account);

			TaskExecuted?.Invoke(this, null);
			return result;
		}

		public async Task<bool> LogoutAsync(IAccount account)
		{
			BeforeTaskExecuted?.Invoke(this, null);
			try {
				// TODO: this method just clears the cache without truely logout the user!!
				await msalClient.RemoveAsync(account).ConfigureAwait(false);
				accountList.Remove(account);
				return true;
			} catch (Exception ex) {
				logger?.LogError(ex, "");
				return false;
			} finally {
				TaskExecuted?.Invoke(this, null);
			}
		}

		#endregion Public Methods

		#region Private Methods

		private static string GetUserId(string url)
		{
			var paths = url.Split('/').ToList();
			var i = paths.IndexOf("users");
			var userId = paths[i + 1];
			return userId;
		}

		private static string GetUserId(IAccount account)
		{
			var id = account.HomeAccountId.ObjectId;
			while (id.StartsWith("0")) {
				id = string.Concat(id.SkipWhile(c => c == '0')).Remove(0, 1);
			}
			if (string.IsNullOrEmpty(id)) {
				throw new InvalidOperationException("Cannot get user id");
			}
			return id;
		}

		private void AppConfig()
		{
			var appConfig = System.Configuration.ConfigurationManager.AppSettings;
			if (!ContainsKey(appConfig, nameof(appId))) {
				throw new ArgumentException($"Given {nameof(appConfig)} has no  configuration key named {nameof(appId)}");
			}
			if (!ContainsKey(appConfig, nameof(scopes))) {
				throw new ArgumentException($"Given {nameof(appConfig)} has no  configuration key named {nameof(scopes)}");
			}
			appId = appConfig[nameof(appId)];
			scopes = appConfig[nameof(scopes)].Split(';');
			username = appConfig[nameof(username)]; // optional
			password = appConfig[nameof(password)]; // optional

			bool ContainsKey(NameValueCollection appConfig, string key)
			{
				return appConfig.AllKeys.Any(item => item == key);
			}
		}

		private async Task<string> GetAccessTokenSilently(IAccount account)
		{
			try {
				using var cts = new CancellationTokenSource(Timeouts.Silent);
				// If there is an account, call AcquireTokenSilent
				// By doing this, MSAL will refresh the token automatically if
				// it is expired. Otherwise it returns the cached token.
				var result = await msalClient.AcquireTokenSilent(scopes, account)
											 .ExecuteAsync(cts.Token)
											 .ConfigureAwait(false);

				return result?.AccessToken;
			} catch (MsalUiRequiredException ex) {
				var result = await LoginInteractively(account, ex.Claims).ConfigureAwait(false);
				return result?.AccessToken;
			} catch (ServiceException ex) {
				logger?.LogError(ex, "onedrive server error");
				return null;
			}
		}

		private IAccount GetAccount(string userId)
		{
			var account = accountList.FirstOrDefault(account => GetUserId(account) == userId);
			if (account == null) {
				throw new InvalidOperationException("Cannot get account");
			}
			return account;
		}

		private void RegisterUser(IAccount account)
		{
			if (HasAccount(account)) {
				throw new InvalidOperationException("The user has already signed in.");
			}
			accountList.Add(account);
		}

		#endregion Private Methods
	}
}