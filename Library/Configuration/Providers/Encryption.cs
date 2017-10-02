using EnterpriseWebLibrary.Encryption;

namespace EnterpriseWebLibrary.Configuration.Providers {
	internal class Encryption: SystemEncryptionProvider {
		byte[] SystemEncryptionProvider.Key => new byte[ 32 ];
	}
}