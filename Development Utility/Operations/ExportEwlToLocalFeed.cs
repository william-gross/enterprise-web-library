using System;
using System.IO;
using System.Linq;
using RedStapler.StandardLibrary;
using RedStapler.StandardLibrary.Configuration;
using RedStapler.StandardLibrary.InstallationSupportUtility;
using RedStapler.StandardLibrary.InstallationSupportUtility.InstallationModel;

namespace EnterpriseWebLibrary.DevelopmentUtility.Operations {
	internal class ExportEwlToLocalFeed: Operation {
		public static Operation Instance { get; } = new ExportEwlToLocalFeed();
		private string outputPath;

		private ExportEwlToLocalFeed() { }

		bool Operation.IsValid( Installation genericInstallation ) {
			var isEwl = genericInstallation is RecognizedDevelopmentInstallation installation && installation.DevelopmentInstallationLogic.SystemIsEwl;
			if( !isEwl )
				return false;

			var commandLine = Environment.GetCommandLineArgs();

			if( commandLine.Length < 3 )
				outputPath = StandardLibraryMethods.CombinePaths( ConfigurationStatics.RedStaplerFolderPath, "Local NuGet Feed" );
			else
				outputPath = commandLine.Last();

			return true;
		}

		void Operation.Execute( Installation genericInstallation, OperationResult operationResult ) {
			var installation = genericInstallation as RecognizedDevelopmentInstallation;

			// NuGet.exe has problems if the folder doesn't exist.
			Directory.CreateDirectory( outputPath );

			ExportLogic.CreateEwlNuGetPackage( installation, true, outputPath, true );
		}
	}
}