﻿using System;
using System.Collections.Generic;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	public sealed class BasicPageContent: PageContent {
		internal readonly string TitleOverride;
		internal readonly TrustedHtmlString CustomHeadElements;
		internal readonly ElementClassSet BodyClasses;
		internal readonly List<FlowComponent> BodyContent = new List<FlowComponent>();
		internal readonly List<EtherealComponent> EtherealContent = new List<EtherealComponent>();
		internal readonly Action DataUpdateModificationMethod;
		internal readonly bool IsAutoDataUpdater;

		/// <summary>
		/// Creates a basic page content object.
		/// </summary>
		/// <param name="titleOverride">Do not pass null.</param>
		/// <param name="customHeadElements"></param>
		/// <param name="bodyClasses"></param>
		/// <param name="dataUpdateModificationMethod">The modification method for the page’s data-update modification.</param>
		/// <param name="isAutoDataUpdater">Pass true to force a post-back when a hyperlink is clicked.</param>
		public BasicPageContent(
			string titleOverride = "", TrustedHtmlString customHeadElements = null, ElementClassSet bodyClasses = null, Action dataUpdateModificationMethod = null,
			bool isAutoDataUpdater = false ) {
			TitleOverride = titleOverride;
			CustomHeadElements = customHeadElements ?? new TrustedHtmlString( "" );
			BodyClasses = bodyClasses ?? ElementClassSet.Empty;
			DataUpdateModificationMethod = dataUpdateModificationMethod;
			IsAutoDataUpdater = isAutoDataUpdater;
		}

		public BasicPageContent Add( IReadOnlyCollection<FlowComponent> components ) {
			BodyContent.AddRange( components );
			return this;
		}

		public BasicPageContent Add( FlowComponent component ) {
			BodyContent.Add( component );
			return this;
		}

		public BasicPageContent Add( IReadOnlyCollection<EtherealComponent> components ) {
			EtherealContent.AddRange( components );
			return this;
		}

		public BasicPageContent Add( EtherealComponent component ) {
			EtherealContent.Add( component );
			return this;
		}

		protected internal override PageContent GetContent() {
			throw new NotSupportedException();
		}
	}
}