﻿using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary.Configuration;
using Tewl.Tools;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	public static class RequestDispatchingStatics {
		private static SystemProviderReference<AppRequestDispatchingProvider> provider;

		internal static void Init( SystemProviderReference<AppRequestDispatchingProvider> provider ) {
			RequestDispatchingStatics.provider = provider;
		}

		/// <summary>
		/// Framework use only.
		/// </summary>
		public static AppRequestDispatchingProvider AppProvider => provider.GetProvider();

		/// <summary>
		/// Returns a list of URL patterns for the framework.
		/// </summary>
		/// <param name="frameworkUrlSegment">The URL segment that will be a base for the framework’s own pages and resources. Pass the empty string to use the
		/// default of “ewl”. Do not pass null.</param>
		/// <param name="appStaticFileUrlSegment">The URL segment that will be a base for the application’s static files. Pass the empty string to use the default
		/// of “static”. Do not pass null.</param>
		public static IReadOnlyCollection<UrlPattern> GetFrameworkUrlPatterns( string frameworkUrlSegment = "", string appStaticFileUrlSegment = "" ) {
			var patterns = new List<UrlPattern>();

			if( !frameworkUrlSegment.Any() )
				frameworkUrlSegment = EwlStatics.EwlInitialism.ToUrlSlug();
			patterns.Add( Admin.EntitySetup.UrlPatterns.Literal( frameworkUrlSegment ) );

			if( !appStaticFileUrlSegment.Any() )
				appStaticFileUrlSegment = "static";
			patterns.Add( AppProvider.GetStaticFilesFolderUrlPattern( appStaticFileUrlSegment ) );

			return patterns;
		}
	}
}