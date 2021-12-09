﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EnterpriseWebLibrary.DataAccess {
	/// <summary>
	/// Manages transactions and cleanup for automatically-opened database connections in a data-access state object.
	/// </summary>
	internal class AutomaticDatabaseConnectionManager {
		private readonly DataAccessState dataAccessState;
		private bool primaryDatabaseConnectionInitialized;
		private readonly List<string> secondaryDatabasesWithInitializedConnections = new List<string>();
		private readonly List<Action> nonTransactionalModificationMethods = new List<Action>();
		private bool transactionsMarkedForRollback;

		public AutomaticDatabaseConnectionManager() {
			dataAccessState = new DataAccessState(
				databaseConnectionInitializer: connection => {
					if( connection.DatabaseInfo.SecondaryDatabaseName.Any()
						    ? secondaryDatabasesWithInitializedConnections.Contains( connection.DatabaseInfo.SecondaryDatabaseName )
						    : primaryDatabaseConnectionInitialized )
						return;

					connection.Open();
					if( DataAccessStatics.DatabaseShouldHaveAutomaticTransactions( connection.DatabaseInfo ) )
						connection.BeginTransaction();

					if( connection.DatabaseInfo.SecondaryDatabaseName.Any() )
						secondaryDatabasesWithInitializedConnections.Add( connection.DatabaseInfo.SecondaryDatabaseName );
					else
						primaryDatabaseConnectionInitialized = true;
				} );
		}

		public DataAccessState DataAccessState => dataAccessState;

		/// <summary>
		/// Queues the specified non-transactional modification method to be executed after database transactions are committed.
		/// </summary>
		public void AddNonTransactionalModificationMethod( Action modificationMethod ) {
			nonTransactionalModificationMethods.Add( modificationMethod );
		}

		public void PreExecuteCommitTimeValidationMethods() {
			if( primaryDatabaseConnectionInitialized ) {
				var connection = dataAccessState.PrimaryDatabaseConnection;
				if( DataAccessStatics.DatabaseShouldHaveAutomaticTransactions( connection.DatabaseInfo ) )
					connection.PreExecuteCommitTimeValidationMethods();
			}
			foreach( var databaseName in secondaryDatabasesWithInitializedConnections ) {
				var connection = dataAccessState.GetSecondaryDatabaseConnection( databaseName );
				if( DataAccessStatics.DatabaseShouldHaveAutomaticTransactions( connection.DatabaseInfo ) )
					connection.PreExecuteCommitTimeValidationMethods();
			}
		}

		public void CommitTransactionsAndExecuteNonTransactionalModificationMethods( bool cacheEnabled ) {
			CleanUpConnectionsAndExecuteNonTransactionalModificationMethods( cacheEnabled );
		}

		public void RollbackTransactions( bool cacheEnabled ) {
			transactionsMarkedForRollback = true;
			CleanUpConnectionsAndExecuteNonTransactionalModificationMethods( cacheEnabled );
		}

		internal void CleanUpConnectionsAndExecuteNonTransactionalModificationMethods( bool cacheEnabled, bool skipNonTransactionalModificationMethods = false ) {
			var methods = new List<Action>();
			if( primaryDatabaseConnectionInitialized )
				methods.Add( () => cleanUpConnection( dataAccessState.PrimaryDatabaseConnection ) );
			foreach( var databaseName in secondaryDatabasesWithInitializedConnections ) {
				var databaseNameCopy = databaseName;
				methods.Add( () => cleanUpConnection( dataAccessState.GetSecondaryDatabaseConnection( databaseNameCopy ) ) );
			}
			methods.Add(
				() => {
					try {
						if( !skipNonTransactionalModificationMethods && !transactionsMarkedForRollback ) {
							if( cacheEnabled ) {
								dataAccessState.DisableCache();
								try {
									foreach( var i in nonTransactionalModificationMethods )
										i();
								}
								finally {
									dataAccessState.ResetCache();
								}
							}
							else
								foreach( var i in nonTransactionalModificationMethods )
									i();
						}
					}
					finally {
						nonTransactionalModificationMethods.Clear();
					}
				} );
			EwlStatics.CallEveryMethod( methods.ToArray() );
			transactionsMarkedForRollback = false;
		}

		private void cleanUpConnection( DBConnection connection ) {
			// Keep the connection initialized during cleanup to accommodate commit-time validation methods.
			try {
				try {
					if( !DataAccessStatics.DatabaseShouldHaveAutomaticTransactions( connection.DatabaseInfo ) )
						return;

					try {
						if( !transactionsMarkedForRollback )
							connection.CommitTransaction();
					}
					catch {
						// Modifying this boolean here means that the order in which connections are cleaned up matters. Not modifying it here means
						// possibly committing things to secondary databases that shouldn't be committed. We've decided that the primary connection
						// is the most likely to have these errors, and is cleaned up first, so modifying this boolean here will yield the best results
						// until we implement a true distributed transaction model with two-phase commit.
						transactionsMarkedForRollback = true;

						throw;
					}
					finally {
						if( transactionsMarkedForRollback )
							connection.RollbackTransaction();
					}
				}
				finally {
					connection.Close();
				}
			}
			finally {
				if( connection.DatabaseInfo.SecondaryDatabaseName.Any() )
					secondaryDatabasesWithInitializedConnections.Remove( connection.DatabaseInfo.SecondaryDatabaseName );
				else
					primaryDatabaseConnectionInitialized = false;
			}
		}
	}
}