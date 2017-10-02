using System.IO;
using EnterpriseWebLibrary.InstallationSupportUtility;

namespace RedStapler.StandardLibrary.Configuration.Providers {
	internal class Isu: SystemIsuProvider {
		string SystemIsuProvider.RsisHttpBaseUrl { get { return "https://info.nopenope.nope"; } }
		string SystemIsuProvider.RsisTcpBaseUrl { get { return "net.tcp://info.nopenope.nope:7747"; } }
		string SystemIsuProvider.RsisTcpUserName { get { return "definitelyreal"; } }
		string SystemIsuProvider.RsisTcpPassword { get { return "redacted"; } }
		string SystemIsuProvider.NDependFolderPathInUserProfileFolder { get { return ""; } }

		void SystemIsuProvider.WriteGeneralProviderMembers( TextWriter writer ) {
			//writer.WriteLine(
			//	"string SystemGeneralProvider.AsposeLicenseName { get { return StandardLibraryMethods.CombinePaths( AppTools.FilesFolderPath, \"Aspose.Total.lic\" ); } }" );
			writer.WriteLine( "string SystemGeneralProvider.FormsLogInEmail { get { return \"pleasedont@emailme.com\"; } }" );
			writer.WriteLine( "string SystemGeneralProvider.FormsLogInPassword { get { return \"nope\"; } }" );
		}
	}
}