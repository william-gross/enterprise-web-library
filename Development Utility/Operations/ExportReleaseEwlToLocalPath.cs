using System;
using System.IO;
using System.Linq;
using RedStapler.StandardLibrary.InstallationSupportUtility;
using RedStapler.StandardLibrary.InstallationSupportUtility.InstallationModel;

namespace EnterpriseWebLibrary.DevelopmentUtility.Operations {
	internal class ExportReleaseEwlToLocalPath: Operation {
		public static Operation Instance { get; } = new ExportReleaseEwlToLocalPath();
		private string outputPath;

		private ExportReleaseEwlToLocalPath() { }

		bool Operation.IsValid( Installation genericInstallation ) {
			var isEwl = genericInstallation is RecognizedDevelopmentInstallation installation && installation.DevelopmentInstallationLogic.SystemIsEwl;
			if( !isEwl )
				return false;

			var commandLine = Environment.GetCommandLineArgs();

			if( commandLine.Length < 3 )
				throw new UserCorrectableException( "Please pass the output path as the last parameter to this operation." );
			
			outputPath = commandLine.Last();

			return true;
		}

		void Operation.Execute( Installation genericInstallation, OperationResult operationResult ) {
			var installation = genericInstallation as RecognizedDevelopmentInstallation;
			
			// NuGet.exe has problems if the folder doesn't exist.
			Directory.CreateDirectory( outputPath );

			ExportLogic.CreateEwlNuGetPackage( installation, false, outputPath, false );
		}
	}
}