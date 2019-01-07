using System.IO;
using RedStapler.StandardLibrary.InstallationSupportUtility;

namespace RedStapler.StandardLibrary.Configuration.Providers {
	internal class Isu : SystemIsuProvider {
		string SystemIsuProvider.RsisHttpBaseUrl { get { return "https://localhost/Rsis"; } }
		string SystemIsuProvider.RsisTcpBaseUrl { get { return "net.tcp://localhost:7747/Rsis"; } }
		string SystemIsuProvider.RsisTcpUserName { get { return ""; } }
		string SystemIsuProvider.RsisTcpPassword { get { return ""; } }
		string SystemIsuProvider.NDependFolderPathInUserProfileFolder { get { return ""; } }

		void SystemIsuProvider.WriteGeneralProviderMembers( TextWriter writer ) {
			
		}
	}
}