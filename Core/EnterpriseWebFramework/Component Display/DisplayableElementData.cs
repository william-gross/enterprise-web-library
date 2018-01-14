﻿using System;
using System.Collections.Generic;
using Humanizer;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// Data for a displayable element.
	/// </summary>
	public class DisplayableElementData {
		internal readonly Func<ElementContext, ElementData> BaseDataGetter;

		/// <summary>
		/// Creates a displayable-element-data object.
		/// </summary>
		public DisplayableElementData(
			DisplaySetup displaySetup, Func<DisplayableElementLocalData> localDataGetter, ElementClassSet classes = null,
			IEnumerable<FlowComponentOrNode> children = null, IEnumerable<EtherealComponent> etherealChildren = null ) {
			displaySetup = displaySetup ?? new DisplaySetup( true );
			BaseDataGetter = context => {
				displaySetup.AddJsShowStatements( "$( '#{0}' ).show( 200 );".FormatWith( context.Id ) );
				displaySetup.AddJsHideStatements( "$( '#{0}' ).hide( 200 );".FormatWith( context.Id ) );
				return new ElementData( () => localDataGetter().BaseDataGetter( displaySetup ), classes: classes, children: children, etherealChildren: etherealChildren );
			};
		}
	}
}