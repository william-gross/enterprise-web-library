using System;
using EnterpriseWebLibrary.EnterpriseWebFramework.UserManagement;
using EnterpriseWebLibrary.WebSessionState;
using Tewl.Tools;

// Parameter: string emailAddress
// Parameter: string returnUrl

namespace EnterpriseWebLibrary.EnterpriseWebFramework.EnterpriseWebLibrary.WebSite.UserManagement {
	partial class ConfirmPasswordReset: EwfPage {
		partial class Info {
			internal User User { get; private set; }

			protected override void init() {
				User = UserManagementStatics.GetUser( EmailAddress );
				if( User == null )
					throw new ApplicationException( "emailAddress" );
			}

			protected override AlternativeResourceMode createAlternativeMode() =>
				FormsAuthStatics.PasswordResetEnabled ? null : new DisabledResourceMode( "Password reset is disabled." );
		}

		protected override PageContent getContent() =>
			new UiPageContent(
				contentFootActions: new ButtonSetup(
					"Reset Password",
					behavior: new PostBackBehavior(
						postBack: PostBack.CreateFull(
							modificationMethod: () => {
								FormsAuthStatics.ResetAndSendPassword( info.User.UserId );
								AddStatusMessage( StatusMessageType.Info, "Your new password has been sent to your email address." );
							},
							actionGetter: () => new PostBackAction( new ExternalResource( info.ReturnUrl ) ) ) ) ).ToCollection() ).Add(
				new Paragraph(
					StringTools.ConcatenateWithDelimiter(
							" ",
							"Click \"Reset Password\" to email yourself a new password.",
							"Upon receiving your new password, you may immediately use it to log in.",
							"You will then be prompted to change your password to something you will remember, which you may use to log in from that point forward." )
						.ToComponents() ) );
	}
}