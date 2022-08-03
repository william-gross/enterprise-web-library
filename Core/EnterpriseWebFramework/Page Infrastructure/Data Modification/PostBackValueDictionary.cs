﻿namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// A dictionary of form control values from a post back.
	/// </summary>
	internal class PostBackValueDictionary {
		private readonly Dictionary<string, object> dictionary = new Dictionary<string, object>();
		private HashSet<string> nonRemovedKeys;

		internal PostBackValueDictionary() {}

		/// <summary>
		/// Returns true if extra post-back values exist.
		/// </summary>
		/// <param name="keyValuePairs">Values cannot be null.</param>
		/// <param name="keyPredicate"></param>
		internal bool AddFromRequest( IEnumerable<KeyValuePair<string, object>> keyValuePairs, Func<string, bool> keyPredicate ) {
			var extraPostBackValuesExist = false;
			foreach( var pair in keyValuePairs )
				if( keyPredicate( pair.Key ) && !dictionary.ContainsKey( pair.Key ) )
					dictionary.Add( pair.Key, pair.Value );
				else
					extraPostBackValuesExist = true;
			return extraPostBackValuesExist;
		}

		// This method and the nonRemovedKeys field are ultimately necessary because, for a group of radio buttons, we need to know the difference between
		// "no selection" and "removed from dictionary".
		internal bool KeyRemoved( string key ) {
			return nonRemovedKeys != null && !nonRemovedKeys.Contains( key );
		}

		/// <summary>
		/// Returns null if there is no value for the specified key.
		/// </summary>
		internal object GetValue( string key ) {
			object value;
			return dictionary.TryGetValue( key, out value ) ? value : null;
		}

		internal void RemoveExcept( IEnumerable<string> keys ) {
			var hashSet = new HashSet<string>( keys );
			foreach( var key in dictionary.Keys.Except( hashSet ).ToArray() )
				dictionary.Remove( key );

			nonRemovedKeys = hashSet;
		}
	}
}