
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using File = Google.Apis.Drive.v3.Data.File;

namespace Cyc.GoogleApi {
	public class GoogleManager {
		private class Timeouts {
			public static readonly TimeSpan Silent = TimeSpan.FromSeconds(10);
		}

		private readonly string[] scopes = new[] { DriveService.Scope.Drive };
		private readonly UserCredential credential;
		private readonly DriveService service;

		public GoogleManager(string clientSecretsPath = null) {
			if (clientSecretsPath == null) {
				clientSecretsPath = @"GoogleApi\client_secret.json";
			}
			credential = FromFileAsync(
				clientSecretsPath,
				scopes).Result;
			service = new DriveService(
				new BaseClientService.Initializer
				{
					HttpClientInitializer = credential,
					ApplicationName = Assembly.GetExecutingAssembly().GetName().Name,
				});
		}

		public async Task<User> GetUserAsync() {
			var request = service.About.Get();
			request.Fields = "user";
			var about = await request.ExecuteAsync().ConfigureAwait(false);
			return about.User;
		}

		public async Task<File> GetDriveRootAsync() {
			var root = await service.Files.Get("root").ExecuteAsync().ConfigureAwait(false);
			return root;
		}

		public async IAsyncEnumerable<File> GetChildrenAsync(string id) {
			var request = service.Files.List();
			request.Q = $"parents in '{id}' and trashed = false";
			do {
				var fileList = await request.ExecuteAsync().ConfigureAwait(false);
				foreach (var file in fileList.Files) {
					yield return file;
				}
				request.PageToken = fileList.NextPageToken;
			} while (!string.IsNullOrEmpty(request.PageToken));
		}

		/// <summary>
		/// Get UserCredential from file.
		/// </summary>
		/// <param name="path">The path of user credential file.</param>
		/// <param name="scopes">The scopes needed for the client application.</param>
		/// <returns>UserCredential class</returns>
		private static async Task<UserCredential> FromFileAsync(string path, IEnumerable<string> scopes) {
			using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
			using var cts = new CancellationTokenSource(Timeouts.Silent);
			return await GoogleWebAuthorizationBroker.AuthorizeAsync(
				GoogleClientSecrets.Load(stream).Secrets,
				new[] { DriveService.Scope.Drive },
				"user",
				cts.Token).ConfigureAwait(false);
		}
	}
}