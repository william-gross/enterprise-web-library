﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// An item group in an EWF table.
	/// </summary>
	public class EwfTableItemGroup {
		internal readonly Lazy<EwfTableItemGroupRemainingData> RemainingData;
		internal readonly List<Func<EwfTableItem>> Items;

		/// <summary>
		/// Creates an item group.
		/// </summary>
		public EwfTableItemGroup( Func<EwfTableItemGroupRemainingData> remainingDataGetter, IEnumerable<Func<EwfTableItem>> items ) {
			RemainingData = new Lazy<EwfTableItemGroupRemainingData>( remainingDataGetter );
			Items = items.ToList();
		}

		internal IReadOnlyCollection<EwfTableItem> GetHeadItems( int fieldCount ) {
			return getHeadItem( fieldCount ).Concat( getItemActionsItem( fieldCount ) ).Materialize();
		}

		private IReadOnlyCollection<EwfTableItem> getHeadItem( int fieldCount ) {
			// NOTE: If group is collapsible, set up item-count display and "click to expand" button.
			// NOTE: The item-count display should be wrapped in a NamingPlaceholder that is part of all tail update regions for this group.
			if( !RemainingData.Value.GroupName.Any() )
				return Enumerable.Empty<EwfTableItem>().Materialize();
			return new EwfTableItem(
				new EwfTableItemSetup( activationBehavior: RemainingData.Value.GroupHeadActivationBehavior ),
				new GenericFlowContainer(
					new GenericFlowContainer( RemainingData.Value.GroupName ).Concat( EwfTable.GetGeneralActionList( null, RemainingData.Value.GroupActions ) )
						.Materialize(),
					classes: EwfTable.ItemGroupNameAndGeneralActionContainerClass ).ToCell( new TableCellSetup( fieldSpan: fieldCount ) ) ).ToCollection();
		}

		private IReadOnlyCollection<EwfTableItem> getItemActionsItem( int fieldCount ) {
			// NOTE: Set up group-level check box selection (if enabled) and group-level check box actions (if they exist). Make sure all items in the group have identical lists.
			// NOTE: Check box actions should show an error if clicked and no items are selected; this caused confusion in M+Vision.
			return Enumerable.Empty<EwfTableItem>().Materialize();
		}
	}
}