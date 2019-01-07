using RedStapler.StandardLibrary.Encryption;

namespace RedStapler.StandardLibrary.Configuration.Providers {
	internal class Encryption: SystemEncryptionProvider {
		byte[] SystemEncryptionProvider.Key {
			get {
				return new byte[]
					{ 100, 115, 120, 123, 130, 155, 161, 17, 177, 182, 183, 200, 210, 219, 220, 222, 224, 237, 246, 247, 30, 40, 41, 54, 71, 76, 77, 77, 82, 85, 86, 92 };
			}
		}
	}
}