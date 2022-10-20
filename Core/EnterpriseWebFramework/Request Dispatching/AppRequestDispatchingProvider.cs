﻿using System.Collections.Immutable;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// Application-specific request-dispatching logic.
	/// </summary>
	public abstract class AppRequestDispatchingProvider {
		/// <summary>
		/// Returns the IP addresses that are allowed to access the application, or null when the application is not down for maintenance.
		/// </summary>
		protected internal virtual ImmutableHashSet<string> GetWhitelistedIpAddressesForMaintenance() => null;

		/// <summary>
		/// Returns the base URL patterns for the application.
		/// </summary>
		protected internal abstract IEnumerable<BaseUrlPattern> GetBaseUrlPatterns();

		/// <summary>
		/// Returns the URL handler that will determine the canonical URL patterns for the framework.
		/// </summary>
		public abstract UrlHandler GetFrameworkUrlParent();

		/// <summary>
		/// Framework use only.
		/// </summary>
		protected internal abstract UrlPattern GetStaticFilesFolderUrlPattern( string urlSegment );

		/// <summary>
		/// Gets the page that users will be transferred to when errors occur in the application.
		/// </summary>
		protected internal virtual PageBase GetErrorPage() => null;
	}
}