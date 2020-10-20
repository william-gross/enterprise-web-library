﻿namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// A base set of functionality that can be used to discover information about a resource before actually requesting it.
	/// </summary>
	public abstract class ResourceInfo {
		/// <summary>
		/// Returns true if the authenticated user is authorized to access the resource.
		/// </summary>
		public abstract bool UserCanAccessResource { get; }

		/// <summary>
		/// Gets the alternative mode for this resource or null if it is in normal mode. Do not call this from the createAlternativeMode method of an ancestor;
		/// doing so will result in a stack overflow.
		/// </summary>
		public abstract AlternativeResourceMode AlternativeMode { get; }

		/// <summary>
		/// Returns an absolute URL that can be used to request the resource.
		/// </summary>
		/// <param name="disableAuthorizationCheck">Pass true to allow a URL to be returned that the authenticated user cannot access. Use with caution. Might be
		/// useful if you are adding the URL to an email message or otherwise displaying it outside the application.</param>
		/// <returns></returns>
		public abstract string GetUrl( bool disableAuthorizationCheck = false );
	}
}