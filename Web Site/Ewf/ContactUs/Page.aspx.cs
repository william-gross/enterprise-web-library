using System;
using EnterpriseWebLibrary.Configuration;
using EnterpriseWebLibrary.Email;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EnterpriseWebLibrary.InputValidation;
using EnterpriseWebLibrary.WebSessionState;

// Parameter: string returnUrl

namespace EnterpriseWebLibrary.EnterpriseWebFramework.EnterpriseWebLibrary.WebSite.ContactUs {
	partial class Page: EwfPage {
		partial class Info {
			public override string ResourceName { get { return ""; } }
		}

		private string emailText;

		protected override void loadData() {
			var pb = PostBack.CreateFull( actionGetter: () => new PostBackAction( new ExternalResourceInfo( info.ReturnUrl ) ) );

			ph.AddControlsReturnThis(
				FormItem.Create(
					"You may report any problems, make suggestions, or ask for help here.",
					new EwfTextBox( "", rows: 20 ),
					validationGetter:
						control =>
						new EwfValidation(
							( pbv, validator ) => emailText = validator.GetString( new ValidationErrorHandler( "text" ), control.GetPostBackValue( pbv ), false ),
							pb ) ).ToControl() );

			EwfUiStatics.SetContentFootActions( new ActionButtonSetup( "Send", new PostBackButton( pb ) ) );

			pb.AddModificationMethod( modifyData );
		}

		private void modifyData() {
			var message = new EmailMessage
				{
					Subject = "Contact from " + ConfigurationStatics.SystemName,
					BodyHtml = ( "Contact from " + AppTools.User.Email + Environment.NewLine + Environment.NewLine + emailText ).GetTextAsEncodedHtml()
				};
			message.ToAddresses.AddRange( EmailStatics.GetAdministratorEmailAddresses() );
			message.ReplyToAddresses.Add( new EmailAddress( AppTools.User.Email ) );
			EmailStatics.SendEmailWithDefaultFromAddress( message );
			AddStatusMessage( StatusMessageType.Info, "Your feedback has been sent." );
		}
	}
}