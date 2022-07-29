﻿using System;
using System.Web;
using EnterpriseWebLibrary.Configuration;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	public class EwfRequest {
		private static AppRequestBaseUrlProvider baseUrlDefaultProvider;
		private static SystemProviderReference<AppRequestBaseUrlProvider> baseUrlProvider;
		private static Func<HttpRequest> currentRequestGetter;

		internal static void Init( SystemProviderReference<AppRequestBaseUrlProvider> baseUrlProvider, Func<HttpRequest> currentRequestGetter ) {
			baseUrlDefaultProvider = new AppRequestBaseUrlProvider();
			EwfRequest.baseUrlProvider = baseUrlProvider;
			EwfRequest.currentRequestGetter = currentRequestGetter;
		}

		internal static AppRequestBaseUrlProvider AppBaseUrlProvider => baseUrlProvider.GetProvider( returnNullIfNotFound: true ) ?? baseUrlDefaultProvider;

		public static EwfRequest Current => new EwfRequest( currentRequestGetter() );

		internal readonly HttpRequest AspNetRequest;

		private EwfRequest( HttpRequest aspNetRequest ) {
			AspNetRequest = aspNetRequest;
		}

		/// <summary>
		/// Returns true if this request is secure.
		/// </summary>
		public bool IsSecure => AppBaseUrlProvider.RequestIsSecure( AspNetRequest );

		/// <summary>
		/// Gets whether the request is from the local computer.
		/// </summary>
		internal bool IsLocal =>
			// Consider using https://www.strathweb.com/2016/04/request-islocal-in-asp-net-core/ for ASP.NET Core.
			AspNetRequest.IsLocal;
	}
}