﻿using System;
using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary.Configuration;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EnterpriseWebLibrary.EnterpriseWebFramework.UserManagement;
using EnterpriseWebLibrary.WebSessionState;
using Humanizer;
using Tewl.Tools;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	public class UiPageContent: PageContent {
		internal class CssElementCreator: ControlCssElementCreator {
			internal const string GlobalContainerId = "ewfUiGlobal";
			internal static readonly ElementClass AppLogoAndUserInfoClass = new ElementClass( "ewfUiAppLogoAndUserInfo" );
			internal static readonly ElementClass AppLogoClass = new ElementClass( "ewfUiAppLogo" );
			internal static readonly ElementClass UserInfoClass = new ElementClass( "ewfUiUserInfo" );
			internal static readonly ElementClass TopErrorMessageListContainerClass = new ElementClass( "ewfUiStatus" );
			internal static readonly ElementClass GlobalNavListContainerClass = new ElementClass( "ewfUiGlobalNav" );

			internal const string EntityAndTabAndContentBlockId = "ewfUiEntityAndTabsAndContent";

			internal static readonly ElementClass EntityAndTopTabContainerClass = new ElementClass( "ewfUiEntityAndTopTabs" );

			internal static readonly ElementClass EntityContainerClass = new ElementClass( "ewfUiEntity" );
			internal static readonly ElementClass EntityNavAndActionContainerClass = new ElementClass( "ewfUiEntityNavAndActions" );
			internal static readonly ElementClass EntityNavListContainerClass = new ElementClass( "ewfUiEntityNav" );
			internal static readonly ElementClass EntityActionListContainerClass = new ElementClass( "ewfUiEntityActions" );
			internal static readonly ElementClass EntitySummaryContainerClass = new ElementClass( "ewfUiEntitySummary" );

			internal static readonly ElementClass TopTabListContainerClass = new ElementClass( "ewfUiTopTab" );

			internal static readonly ElementClass SideTabAndContentBlockClass = new ElementClass( "ewfUiTabsAndContent" );

			internal static readonly ElementClass SideTabContainerClass = new ElementClass( "ewfUiSideTab" );
			internal static readonly ElementClass SideTabGroupHeadClass = new ElementClass( "ewfEditorTabSeparator" );

			internal static readonly ElementClass CurrentTabClass = new ElementClass( "ewfEditorSelectedTab" );
			internal static readonly ElementClass DisabledTabClass = new ElementClass( "ewfUiDisabledTab" );

			internal static readonly ElementClass ContentClass = new ElementClass( "ewfUiContent" );
			internal static readonly ElementClass PageActionListContainerClass = new ElementClass( "ewfUiPageAction" );
			internal static readonly ElementClass ContentFootBlockClass = new ElementClass( "ewfButtons" );
			internal static readonly ElementClass ContentFootActionListContainerClass = new ElementClass( "ewfUiCfActions" );

			internal const string GlobalFootContainerId = "ewfUiGlobalFoot";
			internal static readonly ElementClass PoweredByEwlFooterClass = new ElementClass( "ewfUiPoweredBy" );


			// Some of the elements below cover a subset of other CSS elements in a more specific way. For example, UiGlobalNavControlList selects the control list
			// used for global navigation. This control list is also selected, with lower specificity, by the CSS element that selects all control lists. In general
			// this is a bad situation, but in this case we think it's ok because web apps are not permitted to add their own CSS classes to the controls selected
			// here and therefore it will be difficult for a web app to accidentally trump a CSS element here by adding classes to a lower-specificity element. It
			// would still be possible to accidentally trump some of the rules in the EWF UI style sheets by chaining together several lower-specificity elements, but
			// we protect against this by incorporating an ID into the selectors here. A selector with an ID should always trump a selector without any IDs.

			IReadOnlyCollection<CssElement> ControlCssElementCreator.CreateCssElements() {
				return getGlobalElements().Concat( getEntityAndTabAndContentElements() ).Concat( getGlobalFootElements() ).ToArray();
			}

			private IEnumerable<CssElement> getGlobalElements() {
				const string globalContainerSelector = "div#" + GlobalContainerId;
				return new[]
					{
						new CssElement( "UiGlobalContainer", globalContainerSelector ),
						new CssElement( "UiAppLogoAndUserInfoContainer", globalContainerSelector + " " + "div." + AppLogoAndUserInfoClass.ClassName ),
						new CssElement( "UiAppLogoContainer", globalContainerSelector + " " + "div." + AppLogoClass.ClassName ),
						new CssElement( "UiUserInfoContainer", globalContainerSelector + " " + "div." + UserInfoClass.ClassName ),
						new CssElement(
							"UiTopErrorMessageListContainer",
							ListErrorDisplayStyle.CssSelectors.Select( i => globalContainerSelector + " " + i + "." + TopErrorMessageListContainerClass.ClassName )
								.ToArray() ),
						new CssElement( "UiGlobalNavListContainer", globalContainerSelector + " " + "div." + GlobalNavListContainerClass.ClassName )
					};
			}

			private IEnumerable<CssElement> getEntityAndTabAndContentElements() {
				var elements = new List<CssElement>();

				const string entityAndTabAndContentBlockSelector = "div#" + EntityAndTabAndContentBlockId;
				elements.Add( new CssElement( "UiEntityAndTabAndContentBlock", entityAndTabAndContentBlockSelector ) );

				elements.Add(
					new CssElement( "UiEntityAndTopTabContainer", entityAndTabAndContentBlockSelector + " > " + "div." + EntityAndTopTabContainerClass.ClassName ) );
				elements.AddRange( getEntityElements( entityAndTabAndContentBlockSelector ) );
				elements.Add( new CssElement( "UiTopTabListContainer", entityAndTabAndContentBlockSelector + " " + "div." + TopTabListContainerClass.ClassName ) );
				elements.AddRange( getSideTabAndContentElements( entityAndTabAndContentBlockSelector ) );
				elements.AddRange( getTabElements() );
				return elements;
			}

			private IEnumerable<CssElement> getEntityElements( string entityAndTabAndContentBlockSelector ) {
				return new[]
					{
						new CssElement( "UiEntityContainer", entityAndTabAndContentBlockSelector + " " + "div." + EntityContainerClass.ClassName ),
						new CssElement(
							"UiEntityNavAndActionContainer",
							entityAndTabAndContentBlockSelector + " " + "div." + EntityNavAndActionContainerClass.ClassName ),
						new CssElement( "UiEntityNavListContainer", entityAndTabAndContentBlockSelector + " " + "div." + EntityNavListContainerClass.ClassName ),
						new CssElement( "UiEntityActionListContainer", entityAndTabAndContentBlockSelector + " " + "div." + EntityActionListContainerClass.ClassName ),
						new CssElement( "UiEntitySummaryContainer", entityAndTabAndContentBlockSelector + " " + "div." + EntitySummaryContainerClass.ClassName )
					};
			}

			private IEnumerable<CssElement> getSideTabAndContentElements( string entityAndTabAndContentBlockSelector ) {
				return new[]
					{
						new CssElement(
							"UiSideTabAndContentBlock",
							TableCssElementCreator.Selectors.Select( i => entityAndTabAndContentBlockSelector + " > " + i + "." + SideTabAndContentBlockClass.ClassName )
								.ToArray() ),
						new CssElement( "UiSideTabContainerCell", entityAndTabAndContentBlockSelector + " td." + SideTabContainerClass.ClassName ),
						new CssElement( "UiSideTabContainer", entityAndTabAndContentBlockSelector + " div." + SideTabContainerClass.ClassName ),
						new CssElement( "UiSideTabGroupHead", entityAndTabAndContentBlockSelector + " div." + SideTabGroupHeadClass.ClassName ),
						new CssElement( "UiPageActionAndContentAndContentFootCell", entityAndTabAndContentBlockSelector + " td." + ContentClass.ClassName ),
						new CssElement( "UiPageActionListContainer", entityAndTabAndContentBlockSelector + " " + "div." + PageActionListContainerClass.ClassName ),
						new CssElement( "UiContentBox", entityAndTabAndContentBlockSelector + " " + "div." + ContentClass.ClassName ),
						new CssElement(
							"UiContentFootBlock",
							TableCssElementCreator.Selectors.Select( i => entityAndTabAndContentBlockSelector + " " + i + "." + ContentFootBlockClass.ClassName )
								.ToArray() ),
						new CssElement(
							"UiContentFootActionListContainer",
							entityAndTabAndContentBlockSelector + " " + "div." + ContentFootActionListContainerClass.ClassName )
					};
			}

			private IEnumerable<CssElement> getTabElements() {
				return new[]
					{
						new CssElement(
							"UiCurrentTabActionControl",
							ActionComponentCssElementCreator.Selectors.Select( i => i + "." + CurrentTabClass.ClassName ).ToArray() ),
						new CssElement(
							"UiDisabledTabActionControl",
							ActionComponentCssElementCreator.Selectors.Select( i => i + "." + DisabledTabClass.ClassName ).ToArray() )
					};
			}

			private IEnumerable<CssElement> getGlobalFootElements() {
				const string globalFootContainerSelector = "div#" + GlobalFootContainerId;
				return new[]
					{
						new CssElement( "UiGlobalFootContainer", globalFootContainerSelector ),
						new CssElement( "UiPoweredByEwlFooterContainer", globalFootContainerSelector + " ." + PoweredByEwlFooterClass.ClassName )
					};
			}
		}

		private readonly BasicPageContent basicContent;
		private readonly EntityUiSetup entityUiSetup;
		private readonly List<FlowComponent> content = new List<FlowComponent>();

		/// <summary>
		/// Creates a page content object that uses the EWF user interface.
		/// </summary>
		/// <param name="omitContentBox">Pass true to omit the box-style effect around the page content. Useful when all content is contained within multiple
		/// box-style sections.</param>
		/// <param name="pageActions">The page actions.</param>
		/// <param name="contentFootActions">The content-foot actions. The first action, if it is a post-back, will produce a submit button.</param>
		/// <param name="contentFootComponents">The content-foot components.</param>
		public UiPageContent(
			bool omitContentBox = false, IReadOnlyCollection<ActionComponentSetup> pageActions = null, IReadOnlyCollection<ButtonSetup> contentFootActions = null,
			IReadOnlyCollection<FlowComponent> contentFootComponents = null ) {
			pageActions = pageActions ?? Enumerable.Empty<ActionComponentSetup>().Materialize();
			if( contentFootActions != null && contentFootComponents != null )
				throw new ApplicationException( "Either contentFootActions or contentFootComponents may be specified, but not both." );
			if( contentFootActions == null && contentFootComponents == null )
				contentFootActions = Enumerable.Empty<ButtonSetup>().Materialize();

			entityUiSetup = ( EwfPage.Instance.EsAsBaseType as UiEntitySetup )?.GetUiSetup();
			basicContent = new BasicPageContent().Add(
				getGlobalContainer()
					.Append(
						new GenericFlowContainer(
							getEntityAndTopTabContainer()
								.Append(
									EwfTable.Create( style: EwfTableStyle.Raw, classes: CssElementCreator.SideTabAndContentBlockClass )
										.AddItem(
											EwfTableItem.Create(
												( entityUsesTabMode( TabMode.Vertical )
													  ? getSideTabContainer().ToCell( setup: new TableCellSetup( classes: CssElementCreator.SideTabContainerClass ) ).ToCollection()
													  : Enumerable.Empty<EwfTableCell>() ).Append(
													getPageActionListContainer( pageActions )
														.Append(
															new DisplayableElement(
																context => new DisplayableElementData(
																	null,
																	() => new DisplayableElementLocalData( "div" ),
																	classes: omitContentBox ? null : CssElementCreator.ContentClass,
																	children: content ) ) )
														.Concat( getContentFootBlock( contentFootActions, contentFootComponents ) )
														.Materialize()
														.ToCell( setup: new TableCellSetup( classes: CssElementCreator.ContentClass ) ) )
												.Materialize() ) ) )
								.Materialize(),
							clientSideIdOverride: CssElementCreator.EntityAndTabAndContentBlockId ) )
					.Concat( getGlobalFootContainer() )
					.Materialize() );

			if( !EwfUiStatics.AppProvider.BrowserWarningDisabled() ) {
				if( AppRequestState.Instance.Browser.IsOldVersionOfMajorBrowser() && !StandardLibrarySessionState.Instance.HideBrowserWarningForRemainderOfSession )
					EwfPage.AddStatusMessage(
						StatusMessageType.Warning,
						StringTools.ConcatenateWithDelimiter(
							" ",
							"We've detected that you are not using the latest version of your browser.",
							"While most features of this site will work, and you will be safe browsing here, we strongly recommend using the newest version of your browser in order to provide a better experience on this site and a safer experience throughout the Internet." ) +
						"<br/>" +
						Tewl.Tools.NetTools.BuildBasicLink( "Click here to get Firefox (it's free)", new ExternalResource( "http://www.getfirefox.com" ).GetUrl(), true ) +
						"<br />" +
						Tewl.Tools.NetTools.BuildBasicLink(
							"Click here to get Chrome (it's free)",
							new ExternalResource( "https://www.google.com/intl/en/chrome/browser/" ).GetUrl(),
							true ) + "<br />" + Tewl.Tools.NetTools.BuildBasicLink(
							"Click here to get the latest Internet Explorer (it's free)",
							new ExternalResource( "http://www.beautyoftheweb.com/" ).GetUrl(),
							true ) );
				StandardLibrarySessionState.Instance.HideBrowserWarningForRemainderOfSession = true;
			}
		}

		private FlowComponent getGlobalContainer() =>
			new GenericFlowContainer(
				new[]
						{
							getAppLogoAndUserInfoContainer(),
							new FlowErrorContainer(
								new ErrorSourceSet( includeGeneralErrors: true ),
								new ListErrorDisplayStyle( classes: CssElementCreator.TopErrorMessageListContainerClass ) ),
							getGlobalNavListContainer()
						}.Where( i => i != null )
					.Materialize(),
				clientSideIdOverride: CssElementCreator.GlobalContainerId );

		private FlowComponent getAppLogoAndUserInfoContainer() {
			var appLogo = new GenericFlowContainer(
				( !ConfigurationStatics.IsIntermediateInstallation || AppRequestState.Instance.IntermediateUserExists
					  ? EwfUiStatics.AppProvider.GetLogoComponent()
					  : null ) ?? ( EwfApp.Instance.AppDisplayName.Length > 0 ? EwfApp.Instance.AppDisplayName : ConfigurationStatics.SystemName ).ToComponents(),
				classes: CssElementCreator.AppLogoClass );

			var userInfo = new List<FlowComponent>();
			if( AppRequestState.Instance.UserAccessible ) {
				var changePasswordPage = EwfApp.MetaLogicFactory.CreateChangePasswordPageInfo( EwfPage.Instance.InfoAsBaseType.GetUrl() );
				if( changePasswordPage.UserCanAccessResource && AppTools.User != null )
					userInfo.Add( new GenericFlowContainer( getUserInfo( changePasswordPage ), classes: CssElementCreator.UserInfoClass ) );
			}

			return new GenericFlowContainer( appLogo.Concat( userInfo ).Materialize(), classes: CssElementCreator.AppLogoAndUserInfoClass );
		}

		private IReadOnlyCollection<FlowComponent> getUserInfo( PageInfo changePasswordPage ) {
			var components = new List<FlowComponent>();

			components.AddRange( "Logged in as {0}".FormatWith( AppTools.User.Email ).ToComponents() );
			if( !FormsAuthStatics.FormsAuthEnabled )
				return components;

			components.Add(
				new InlineList(
					new EwfHyperlink( changePasswordPage, new ButtonHyperlinkStyle( "Change password", buttonSize: ButtonSize.ShrinkWrap ) ).ToCollection()
						.ToComponentListItem()
						.ToCollection()
						.Append(
							new EwfButton(
									new StandardButtonStyle( "Log out", buttonSize: ButtonSize.ShrinkWrap ),
									behavior: new PostBackBehavior(
										postBack: PostBack.CreateFull(
											id: "ewfLogOut",
											firstModificationMethod: FormsAuthStatics.LogOutUser,
											actionGetter: () => {
												// NOTE: Is this the correct behavior if we are already on a public page?
												return new PostBackAction( new ExternalResource( NetTools.HomeUrl ) );
											} ) ) ).ToCollection()
								.ToComponentListItem() ) ) );

			return components;
		}

		private FlowComponent getGlobalNavListContainer() {
			// This check exists to prevent the display of lookup boxes or other post back controls. With these controls we sometimes don't have a specific
			// destination page to use for an authorization check, meaning that the system code has no way to prevent their display when there is no intermediate
			// user.
			if( ConfigurationStatics.IsIntermediateInstallation && !AppRequestState.Instance.IntermediateUserExists )
				return null;

			var formItems = EwfUiStatics.AppProvider.GetGlobalNavFormControls()
				.Select( ( control, index ) => control.GetFormItem( PostBack.GetCompositeId( "global", "nav", index.ToString() ) ) )
				.Materialize();
			var listItems = getActionListItems( EwfUiStatics.AppProvider.GetGlobalNavActions() ).Concat( formItems.Select( i => i.ToListItem() ) ).Materialize();
			if( !listItems.Any() )
				return null;

			return new GenericFlowContainer(
				new LineList( listItems.Select( i => (LineListItem)i ) ).ToCollection(),
				classes: CssElementCreator.GlobalNavListContainerClass );
		}

		private FlowComponent getEntityAndTopTabContainer() {
			var components = new List<FlowComponent> { getEntityContainer() };
			if( entityUsesTabMode( TabMode.Horizontal ) ) {
				var resourceGroups = EwfPage.Instance.EsAsBaseType.Resources;
				if( resourceGroups.Count > 1 )
					throw new ApplicationException( "Top tabs are not supported with multiple resource groups." );
				components.Add( getTopTabListContainer( resourceGroups.Single() ) );
			}
			return new GenericFlowContainer( components, classes: CssElementCreator.EntityAndTopTabContainerClass );
		}

		private FlowComponent getEntityContainer() =>
			new GenericFlowContainer(
				getPagePath().Concat( getEntityNavAndActionContainer() ).Concat( getEntitySummaryContainer() ).Materialize(),
				classes: CssElementCreator.EntityContainerClass );

		private IReadOnlyCollection<FlowComponent> getPagePath() {
			var entitySetup = EwfPage.Instance.EsAsBaseType;
			var pagePath = new PagePath(
				currentPageBehavior: entitySetup != null && EwfPage.Instance.InfoAsBaseType.ParentResource == null && entitySetup.Resources.Any()
					                     ? PagePathCurrentPageBehavior.IncludeCurrentPageAndExcludePageNameIfEntitySetupExists
					                     : PagePathCurrentPageBehavior.IncludeCurrentPage );
			return pagePath.IsEmpty ? Enumerable.Empty<FlowComponent>().Materialize() : pagePath.ToCollection();
		}

		private IReadOnlyCollection<FlowComponent> getEntityNavAndActionContainer() {
			var items = new[] { getEntityNavListContainer(), getEntityActionListContainer() }.Where( i => i != null ).Materialize();
			return items.Any()
				       ? new GenericFlowContainer( items, classes: CssElementCreator.EntityNavAndActionContainerClass ).ToCollection()
				       : Enumerable.Empty<FlowComponent>().Materialize();
		}

		private FlowComponent getEntityNavListContainer() {
			if( entityUiSetup == null )
				return null;

			var formItems = entityUiSetup.NavFormControls
				.Select( ( control, index ) => control.GetFormItem( PostBack.GetCompositeId( "entity", "nav", index.ToString() ) ) )
				.Materialize();
			var listItems = getActionListItems( entityUiSetup.NavActions ).Concat( formItems.Select( i => i.ToListItem() ) ).Materialize();
			if( !listItems.Any() )
				return null;

			return new GenericFlowContainer(
				new LineList( listItems.Select( i => (LineListItem)i ) ).ToCollection(),
				classes: CssElementCreator.EntityNavListContainerClass );
		}

		private FlowComponent getEntityActionListContainer() {
			if( entityUiSetup == null || EwfPage.Instance.InfoAsBaseType.ParentResource != null )
				return null;
			var listItems = getActionListItems( entityUiSetup.Actions ).Materialize();
			if( !listItems.Any() )
				return null;
			return new GenericFlowContainer(
				new LineList( listItems.Select( i => (LineListItem)i ) ).ToCollection(),
				classes: CssElementCreator.EntityActionListContainerClass );
		}

		private IReadOnlyCollection<FlowComponent> getEntitySummaryContainer() {
			if( entityUiSetup?.EntitySummaryContent != null )
				return new GenericFlowContainer( entityUiSetup.EntitySummaryContent, classes: CssElementCreator.EntitySummaryContainerClass ).ToCollection();
			return Enumerable.Empty<FlowComponent>().Materialize();
		}

		private FlowComponent getTopTabListContainer( ResourceGroup resourceGroup ) =>
			new GenericFlowContainer(
				new LineList(
					getTabHyperlinksForResources( resourceGroup, false ).Select( i => (LineListItem)i.ToComponentListItem() ),
					verticalAlignment: FlexboxVerticalAlignment.Bottom ).ToCollection(),
				classes: CssElementCreator.TopTabListContainerClass );

		private bool entityUsesTabMode( TabMode tabMode ) =>
			entityUiSetup != null && EwfPage.Instance.InfoAsBaseType.ParentResource == null && entityUiSetup.GetTabMode( EwfPage.Instance.EsAsBaseType ) == tabMode;

		private FlowComponent getSideTabContainer() {
			var components = new List<FlowComponent>();
			foreach( var resourceGroup in EwfPage.Instance.EsAsBaseType.Resources ) {
				var tabs = getTabHyperlinksForResources( resourceGroup, true );
				if( tabs.Any() && resourceGroup.Name.Any() )
					components.Add( new GenericFlowContainer( resourceGroup.Name.ToComponents(), classes: CssElementCreator.SideTabGroupHeadClass ) );
				components.AddRange( tabs );
			}
			return new GenericFlowContainer( components, classes: CssElementCreator.SideTabContainerClass );
		}

		private IReadOnlyCollection<PhrasingComponent> getTabHyperlinksForResources( ResourceGroup resourceGroup, bool includeIcons ) {
			var hyperlinks = new List<PhrasingComponent>();
			foreach( var resource in resourceGroup.Resources.Where( p => p.UserCanAccessResource ) )
				hyperlinks.Add(
					new EwfHyperlink(
						resource.IsIdenticalToCurrent() ? null : resource,
						new StandardHyperlinkStyle(
							resource.ResourceName,
							icon: includeIcons ? new ActionComponentIcon( new FontAwesomeIcon( resource.IsIdenticalToCurrent() ? "fa-circle" : "fa-circle-thin" ) ) : null ),
						classes: resource.IsIdenticalToCurrent() ? CssElementCreator.CurrentTabClass :
						         resource.AlternativeMode is DisabledResourceMode ? CssElementCreator.DisabledTabClass : ElementClassSet.Empty ) );
			return hyperlinks;
		}

		private IReadOnlyCollection<FlowComponent> getPageActionListContainer( IReadOnlyCollection<ActionComponentSetup> pageActions ) {
			var listItems = getActionListItems( pageActions ).Materialize();
			if( !listItems.Any() )
				return Enumerable.Empty<FlowComponent>().Materialize();
			return new GenericFlowContainer(
				( EwfUiStatics.AppProvider.PageActionItemsSeparatedWithPipe()
					  ? (FlowComponent)new InlineList( listItems )
					  : new LineList( listItems.Select( i => (LineListItem)i ) ) ).ToCollection(),
				classes: CssElementCreator.PageActionListContainerClass ).ToCollection();
		}

		private IEnumerable<ComponentListItem> getActionListItems( IReadOnlyCollection<ActionComponentSetup> actions ) =>
			from action in actions
			let actionComponent = action.GetActionComponent(
				( text, icon ) => new ButtonHyperlinkStyle( text, buttonSize: ButtonSize.ShrinkWrap, icon: icon ),
				( text, icon ) => new StandardButtonStyle( text, buttonSize: ButtonSize.ShrinkWrap, icon: icon ) )
			where actionComponent != null
			select actionComponent.ToComponentListItem( displaySetup: action.DisplaySetup );

		private IReadOnlyCollection<FlowComponent> getContentFootBlock(
			IReadOnlyCollection<ButtonSetup> contentFootActions, IReadOnlyCollection<FlowComponent> contentFootComponents ) {
			var components = new List<FlowComponent>();
			if( contentFootActions != null ) {
				if( contentFootActions.Any() )
					components.Add(
						new GenericFlowContainer(
							new LineList(
								contentFootActions.Select(
									( action, index ) => (LineListItem)action.GetActionComponent(
											null,
											( text, icon ) => new StandardButtonStyle( text, buttonSize: ButtonSize.Large, icon: icon ),
											enableSubmitButton: index == 0 )
										.ToComponentListItem( displaySetup: action.DisplaySetup ) ) ).ToCollection(),
							classes: CssElementCreator.ContentFootActionListContainerClass ) );
				else if( EwfPage.Instance.IsAutoDataUpdater )
					components.Add( new SubmitButton( new StandardButtonStyle( "Update Now" ), postBack: EwfPage.Instance.DataUpdatePostBack ) );
			}
			else {
				if( EwfPage.Instance.IsAutoDataUpdater )
					throw new ApplicationException( "AutoDataUpdater is not currently compatible with custom content foot controls." );
				components.AddRange( contentFootComponents );
			}

			if( !components.Any() )
				return Enumerable.Empty<FlowComponent>().Materialize();

			var table = EwfTable.Create( style: EwfTableStyle.StandardLayoutOnly, classes: CssElementCreator.ContentFootBlockClass );
			table.AddItem(
				EwfTableItem.Create(
					components.ToCell(
						new TableCellSetup(
							textAlignment: contentFootActions == null || !contentFootActions.Any() ? TextAlignment.Center : TextAlignment.NotSpecified ) ) ) );
			return table.ToCollection();
		}

		private IReadOnlyCollection<FlowComponent> getGlobalFootContainer() {
			var components = new List<FlowComponent>();

			// This check exists to prevent the display of post back controls. With these controls we sometimes don't have a specific destination page to use for an
			// authorization check, meaning that the system code has no way to prevent their display when there is no intermediate user.
			if( !ConfigurationStatics.IsIntermediateInstallation || AppRequestState.Instance.IntermediateUserExists )
				components.AddRange( EwfUiStatics.AppProvider.GetGlobalFootComponents() );

			var ewlWebSite = new ExternalResource( "http://enterpriseweblibrary.org/" );
			if( ewlWebSite.UserCanAccessResource && !EwfUiStatics.AppProvider.PoweredByEwlFooterDisabled() )
				components.Add(
					new Paragraph(
						"Powered by the ".ToComponents()
							.Append( new EwfHyperlink( ewlWebSite.ToHyperlinkNewTabBehavior(), new StandardHyperlinkStyle( EwlStatics.EwlName ) ) )
							.Concat(
								" ({0} version)".FormatWith( TimeZoneInfo.ConvertTime( EwlStatics.EwlBuildDateTime, TimeZoneInfo.Local ).ToMonthYearString() ).ToComponents() )
							.Materialize(),
						classes: CssElementCreator.PoweredByEwlFooterClass ) );

			return components.Any()
				       ? new GenericFlowContainer( components, clientSideIdOverride: CssElementCreator.GlobalFootContainerId ).ToCollection()
				       : Enumerable.Empty<FlowComponent>().Materialize();
		}

		public UiPageContent Add( IReadOnlyCollection<FlowComponent> components ) {
			content.AddRange( components );
			return this;
		}

		public UiPageContent Add( FlowComponent component ) {
			content.Add( component );
			return this;
		}

		protected internal override PageContent GetContent() => basicContent;
	}
}