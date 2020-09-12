﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tewl.Tools;

namespace EnterpriseWebLibrary {
	/// <summary>
	/// Helpful System.Exception methods.
	/// </summary>
	public static class ExceptionTools {
		/// <summary>
		/// Returns a list containing this exception and all inner exceptions.
		/// </summary>
		public static IEnumerable<Exception> GetChain( this Exception exception ) {
			IEnumerable<Exception> chain = exception.ToCollection();
			if( exception.InnerException != null )
				chain = chain.Concat( exception.InnerException.GetChain() );
			return chain;
		}
	}
}