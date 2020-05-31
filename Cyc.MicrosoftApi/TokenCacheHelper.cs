using Microsoft.Identity.Client;

using System.IO;
using System.Security.Cryptography;

namespace Cyc.MicrosoftApi {

	internal static class TokenCacheHelper {

		#region Public Fields

		/// <summary>
		/// Path to the token cache
		/// </summary>
		public static readonly string CacheFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.bin3";

		#endregion Public Fields

		#region Private Fields

		private static readonly object FileLock = new object();

		#endregion Private Fields

		#region Public Methods

		public static void AfterAccessNotification(TokenCacheNotificationArgs args)
		{
			// if the access operation resulted in a cache update
			if (args.HasStateChanged) {
				lock (FileLock) {
					// reflect changesgs in the persistent store
					File.WriteAllBytes(CacheFilePath,
									   ProtectedData.Protect(args.TokenCache.SerializeMsalV3(),
															 null,
															 DataProtectionScope.CurrentUser)
									  );
				}
			}
		}

		public static void BeforeAccessNotification(TokenCacheNotificationArgs args)
		{
			lock (FileLock) {
				args.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath)
						? ProtectedData.Unprotect(File.ReadAllBytes(CacheFilePath),
												 null,
												 DataProtectionScope.CurrentUser)
						: null);
			}
		}

		#endregion Public Methods

		#region Internal Methods

		internal static void EnableSerialization(ITokenCache tokenCache)
		{
			tokenCache.SetBeforeAccess(BeforeAccessNotification);
			tokenCache.SetAfterAccess(AfterAccessNotification);
		}

		#endregion Internal Methods
	}
}