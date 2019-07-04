using System;
using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EnterpriseWebLibrary.EnterpriseWebFramework.UserManagement;
using Humanizer;

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
					var registeredComponents = new List<FlowComponent>();
					registeredComponents.Add(
						new Paragraph(
							"You may log in to this system if you have registered your email address with {0}"
								.FormatWith( FormsAuthStatics.SystemProvider.AdministratingCompanyName )
								.ToComponents() ) );

					emailAddress = new DataValue<string>();
					var password = new DataValue<string>();
					registeredComponents.Add(
						FormItemList.CreateStack(
							items: FormState
								.ExecuteWithDataModificationsAndDefaultAction(
									new[] { logInPb, newPasswordPb },
									() => emailAddress.GetEmailAddressFormItem( "Email address".ToComponents() ) )
								.ToCollection()
								.Append(
									password.ToTextControl( true, setup: TextControlSetup.CreateObscured( autoFillTokens: "current-password" ), value: "" )
										.ToFormItem( label: "Password".ToComponents() ) )
								.Materialize() ) );

					if( FormsAuthStatics.PasswordResetEnabled )
						registeredComponents.Add(
							new Paragraph(
								"If you are a first-time user and do not know your password, or if you have forgotten your password, ".ToComponents()
									.Append(
										new EwfButton(
											new StandardButtonStyle( "click here to immediately send yourself a new password.", buttonSize: ButtonSize.ShrinkWrap ),
											behavior: new PostBackBehavior( postBack: newPasswordPb ) ) )
									.Materialize() ) );

					ph.AddControlsReturnThis( new Section( "Registered users", registeredComponents ).ToCollection().GetControls() );

					var specialInstructions = EwfUiStatics.AppProvider.GetSpecialInstructionsForLogInPage();
					if( specialInstructions != null )
						ph.AddControlsReturnThis( specialInstructions );
					else {
						var unregisteredComponents = new List<FlowComponent>();
						unregisteredComponents.Add(
							new Paragraph(
								"If you have difficulty logging in, please {0}".FormatWith( FormsAuthStatics.SystemProvider.LogInHelpInstructions ).ToComponents() ) );
						ph.AddControlsReturnThis( new Section( "Unregistered users", unregisteredComponents ).ToCollection().GetControls() );
					}

					logInHiddenFieldsAndMethod = FormsAuthStatics.GetLogInHiddenFieldsAndMethod(
						emailAddress,
						password,
						getUnregisteredEmailMessage(),
						"Incorrect password. If you do not know your password, enter your email address and send yourself a new password using the link below." );
					logInHiddenFieldsAndMethod.Item1.AddEtherealControls( this );

					EwfUiStatics.SetContentFootActions( new ButtonSetup( "Log In" ).ToCollection() );
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