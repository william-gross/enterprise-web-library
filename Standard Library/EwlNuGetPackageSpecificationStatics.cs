using System;

namespace RedStapler.StandardLibrary {
	/// <summary>
	/// EWL and System Manager use only.
	/// </summary>
	public class EwlNuGetPackageSpecificationStatics {
		/// <summary>
		/// Specify the build number only for prereleases.
		/// </summary>
		public static string GetNuGetPackageFileName( string systemShortName, bool prerelease, DateTime localExportDateAndTime ) =>
			$"{GetNuGetPackageId( systemShortName )}.{GetNuGetPackageVersionString( prerelease, localExportDateAndTime: localExportDateAndTime )}.nupkg";

		public static string GetNuGetPackageId( string systemShortName ) => systemShortName;

		/// <summary>
		/// Specify the build number only for prereleases.
		/// </summary>
		public static string GetNuGetPackageVersionString( bool prerelease, DateTime localExportDateAndTime ) =>
			$"{localExportDateAndTime.Year}.{localExportDateAndTime.Month}.{localExportDateAndTime.Day}.{localExportDateAndTime.Hour * 10000 + localExportDateAndTime.Minute * 100 + localExportDateAndTime.Second}{( prerelease ? "-pr" : "" )}";
	}
}