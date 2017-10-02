namespace EnterpriseWebLibrary.Configuration.Providers {
	internal class General: SystemGeneralProvider {
		string SystemGeneralProvider.AsposePdfLicenseName => "";
		string SystemGeneralProvider.AsposeWordsLicenseName => "";
		string SystemGeneralProvider.IntermediateLogInPassword { get { return "password"; } }
		string SystemGeneralProvider.FormsLogInEmail { get { return ""; } }
		string SystemGeneralProvider.FormsLogInPassword { get { return ""; } }
		string SystemGeneralProvider.EmailDefaultFromName { get { return "EWL Team"; } }
		string SystemGeneralProvider.EmailDefaultFromAddress { get { return "contact@enterpriseweblibrary.org"; } }
	}
}