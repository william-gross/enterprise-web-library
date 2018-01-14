using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EnterpriseWebLibrary.EnterpriseWebFramework.UserManagement;
using MoreLinq;

// Parameter: string returnUrl

namespace EnterpriseWebLibrary.EnterpriseWebFramework.EnterpriseWebLibrary.WebSite.UserManagement {
	public partial class LogIn: EwfPage {
		private DataValue<string> emailAddress;
		private FormsAuthCapableUser user;

		protected override void loadData() {
			Tuple<IReadOnlyCollection<EtherealComponent>, Func<FormsAuthCapableUser>> logInHiddenFieldsAndMethod = null;
			var logInPb = PostBack.CreateFull(
				firstModificationMethod: () => user = logInHiddenFieldsAndMethod.Item2(),
				actionGetter: () => new PostBackAction(
					user.MustChangePassword ? ChangePassword.Page.GetInfo( info.ReturnUrl ) as ResourceInfo : new ExternalResourceInfo( info.ReturnUrl ) ) );
			var newPasswordPb = PostBack.CreateFull( id: "newPw", actionGetter: getSendNewPasswordAction );

			FormState.ExecuteWithDataModificationsAndDefaultAction(
				logInPb.ToCollection(),
				() => {
					var registeredTable = EwfTable.Create( caption: "Registered users" );
					registeredTable.AddItem(
						new EwfTableItem(
							( "You may log in to this system if you have registered your email address with " + FormsAuthStatics.SystemProvider.AdministratingCompanyName ).ToCell(
								new TableCellSetup( fieldSpan: 2 ) ) ) );

					emailAddress = new DataValue<string>();
					FormState.ExecuteWithDataModificationsAndDefaultAction(
						new[] { logInPb, newPasswordPb },
						() => registeredTable.AddItem(
							new EwfTableItem( "Email address", emailAddress.GetEmailAddressFormItem( "", "Please enter a valid email address." ).ToControl() ) ) );

					var password = new DataValue<string>();
					registeredTable.AddItem(
						new EwfTableItem(
							"Password",
							FormItem.Create(
									"",
									new EwfTextBox( "", masksCharacters: true ),
									validationGetter: control => new EwfValidation( ( pbv, v ) => password.Value = control.GetPostBackValue( pbv ) ) )
								.ToControl() ) );

					if( FormsAuthStatics.PasswordResetEnabled )
						registeredTable.AddItem(
							new EwfTableItem(
								new PlaceHolder().AddControlsReturnThis(
										"If you are a first-time user and do not know your password, or if you have forgotten your password, ".ToComponents()
											.GetControls()
											.Concat(
												new PostBackButton(
													new TextActionControlStyle( "click here to immediately send yourself a new password." ),
													usesSubmitBehavior: false,
													postBack: newPasswordPb ) ) )
									.ToCell( new TableCellSetup( fieldSpan: 2 ) ) ) );

					ph.AddControlsReturnThis( registeredTable );

					var specialInstructions = EwfUiStatics.AppProvider.GetSpecialInstructionsForLogInPage();
					if( specialInstructions != null )
						ph.AddControlsReturnThis( specialInstructions );
					else {
						var unregisteredTable = EwfTable.Create( caption: "Unregistered users" );
						unregisteredTable.AddItem( new EwfTableItem( "If you have difficulty logging in, please " + FormsAuthStatics.SystemProvider.LogInHelpInstructions ) );
						ph.AddControlsReturnThis( unregisteredTable );
					}

					logInHiddenFieldsAndMethod = FormsAuthStatics.GetLogInHiddenFieldsAndMethod(
						emailAddress,
						password,
						getUnregisteredEmailMessage(),
						"Incorrect password. If you do not know your password, enter your email address and send yourself a new password using the link below." );
					logInHiddenFieldsAndMethod.Item1.AddEtherealControls( this );

					EwfUiStatics.SetContentFootActions( new ActionButtonSetup( "Log In", new PostBackButton() ) );
				} );
		}

		private PostBackAction getSendNewPasswordAction() {
			var userLocal = UserManagementStatics.GetUser( emailAddress.Value );
			if( userLocal == null )
				throw new DataModificationException( getUnregisteredEmailMessage() );
			return new PostBackAction( ConfirmPasswordReset.GetInfo( userLocal.Email, info.ReturnUrl ) );
		}

		private string getUnregisteredEmailMessage() {
			return "The email address you entered is not registered.  You must register the email address with " +
			       FormsAuthStatics.SystemProvider.AdministratingCompanyName + " before using it to log in.  To do this, " +
			       FormsAuthStatics.SystemProvider.LogInHelpInstructions;
		}
	}
}