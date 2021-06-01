﻿using System;
using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary.Configuration;
using EnterpriseWebLibrary.WebSessionState;
using Humanizer;
using JetBrains.Annotations;
using Tewl.Tools;

// EwlPage
// Parameter: string returnUrl
// OptionalParameter: string user

namespace EnterpriseWebLibrary.EnterpriseWebFramework.UserManagement.Pages {
	// This page does not use the EWF UI because displaying authenticated user information would be misleading.
	partial class Impersonate {
		private static readonly ElementClass elementClass = new ElementClass( "ewfSelectUser" );

		[ UsedImplicitly ]
		private class CssElementCreator: ControlCssElementCreator {
			IReadOnlyCollection<CssElement> ControlCssElementCreator.CreateCssElements() =>
				new CssElement( "SelectUserPageBody", "body.{0}".FormatWith( elementClass.ClassName ) ).ToCollection();
		}

		private bool pageViewDataModificationsExecuted;
		internal User UserObject { get; private set; }

		protected override void init() {
			if( !UserManagementStatics.UserManagementEnabled )
				throw new ApplicationException( "User management not enabled" );

			if( User.Any() && User != "anonymous" && ( UserObject = UserManagementStatics.GetUser( User ) ) == null )
				throw new ApplicationException( "user" );
		}

		protected override bool userCanAccessResource {
			get {
				var user = AppRequestState.Instance.ImpersonatorExists ? AppRequestState.Instance.ImpersonatorUser : AppTools.User;
				return ( user != null && user.Role.CanManageUsers ) || !ConfigurationStatics.IsLiveInstallation;
			}
		}

		protected override UrlHandler getUrlParent() => new Admin.EntitySetup();

		protected override Action getPageViewDataModificationMethod() {
			pageViewDataModificationsExecuted = true;

			if( !User.Any() )
				return null;
			return () => UserImpersonationStatics.BeginImpersonation( UserObject );
		}

		protected override PageContent getContent() {
			var content = new BasicPageContent( bodyClasses: elementClass );

			if( User.Any() ) {
				if( !pageViewDataModificationsExecuted )
					throw new ApplicationException( "Page-view data modifications did not execute." );

				content.Add( new Paragraph( "Please wait.".ToComponents() ) );
				StandardLibrarySessionState.Instance.SetInstantClientSideNavigation( new ExternalResource( ReturnUrl ).GetUrl() );
				return content;
			}

			content.Add( new PageName() );

			if( ConfigurationStatics.IsLiveInstallation )
				content.Add(
					new Paragraph(
						new ImportantContent( "Warning:".ToComponents() ).ToCollection()
							.Concat(
								" Do not impersonate a user without permission. Your actions will be attributed to the user you are impersonating, not to you.".ToComponents() )
							.Materialize() ) );

			var user = new DataValue<User>();
			var pb = PostBack.CreateFull(
				modificationMethod: () => UserImpersonationStatics.BeginImpersonation( user.Value ),
				actionGetter: () => new PostBackAction(
					new ExternalResource(
						ReturnUrl.Any()
							? ReturnUrl
							: EwfConfigurationStatics.AppConfiguration.DefaultBaseUrl.GetUrlString( EwfConfigurationStatics.AppSupportsSecureConnections ) ) ) );
			FormState.ExecuteWithDataModificationsAndDefaultAction(
				pb.ToCollection(),
				() => {
					content.Add(
						new EmailAddressControl(
								"",
								true,
								validationMethod: ( postBackValue, validator ) => {
									if( !postBackValue.Any() ) {
										user.Value = null;
										return;
									}
									user.Value = UserManagementStatics.GetUser( postBackValue );
									if( user.Value == null )
										validator.NoteErrorAndAddMessage( "The email address you entered does not match a user." );
								} ).ToFormItem( label: "User's email address (leave blank for anonymous)".ToComponents() )
							.ToComponentCollection()
							.Append(
								new Paragraph(
									new EwfButton(
											new StandardButtonStyle(
												AppRequestState.Instance.ImpersonatorExists ? "Change User" : "Begin Impersonation",
												buttonSize: ButtonSize.Large ) )
										.ToCollection() ) )
							.Materialize() );
				} );

			return content;
		}
	}
}