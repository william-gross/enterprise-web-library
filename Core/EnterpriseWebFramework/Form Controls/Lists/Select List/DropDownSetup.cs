﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// The configuration for a drop-down.
	/// </summary>
	public static class DropDownSetup {
		/// <summary>
		/// Creates a setup object for a standard drop-down.
		/// </summary>
		/// <param name="items">The items in the list. There must be at least one.</param>
		/// <param name="width">The width of the list. This overrides any value that may be specified via CSS. If no width is specified via CSS and you pass null
		/// for this parameter, the list will be just wide enough to show the selected item and will resize whenever the selected item is changed.</param>
		/// <param name="unlistedSelectedItemLabelGetter">A function that will be called if the selected item ID does not match any list item and is not the default
		/// value of the type. The function takes the selected item ID and returns the label of the unlisted selected item, which will appear before all other
		/// items in the list. The string " (invalid)" will be appended to the label.</param>
		/// <param name="placeholderText">The default-value placeholder's text. Do not pass null.</param>
		/// <param name="action">The action that will occur when the user hits Enter on the drop-down list. Pass null to use the current default action.</param>
		/// <param name="selectionChangedAction">The action that will occur when the selection is changed. Pass null for no action.</param>
		/// <param name="itemIdPageModificationValue"></param>
		/// <param name="itemMatchPageModificationSetups"></param>
		/// <param name="validationPredicate"></param>
		/// <param name="validationErrorNotifier"></param>
		public static DropDownSetup<ItemIdType> Create<ItemIdType>(
			IEnumerable<SelectListItem<ItemIdType>> items, ContentBasedLength width = null, Func<ItemIdType, string> unlistedSelectedItemLabelGetter = null,
			string placeholderText = "Please select", FormAction action = null, FormAction selectionChangedAction = null,
			PageModificationValue<ItemIdType> itemIdPageModificationValue = null,
			IReadOnlyCollection<ListItemMatchPageModificationSetup<ItemIdType>> itemMatchPageModificationSetups = null, Func<bool, bool> validationPredicate = null,
			Action validationErrorNotifier = null ) =>
			new DropDownSetup<ItemIdType>(
				width,
				false,
				unlistedSelectedItemLabelGetter,
				placeholderText,
				items,
				action,
				selectionChangedAction,
				itemIdPageModificationValue,
				itemMatchPageModificationSetups,
				validationPredicate,
				validationErrorNotifier );

		/// <summary>
		/// Creates a setup object for a read-only drop-down.
		/// </summary>
		/// <param name="items">The items in the list. There must be at least one.</param>
		/// <param name="width">The width of the list. This overrides any value that may be specified via CSS. If no width is specified via CSS and you pass null
		/// for this parameter, the list will be just wide enough to show the selected item and will resize whenever the selected item is changed.</param>
		/// <param name="unlistedSelectedItemLabelGetter">A function that will be called if the selected item ID does not match any list item and is not the default
		/// value of the type. The function takes the selected item ID and returns the label of the unlisted selected item, which will appear before all other
		/// items in the list. The string " (invalid)" will be appended to the label.</param>
		/// <param name="placeholderText">The default-value placeholder's text. Do not pass null.</param>
		/// <param name="validationPredicate"></param>
		/// <param name="validationErrorNotifier"></param>
		public static DropDownSetup<ItemIdType> CreateReadOnly<ItemIdType>(
			IEnumerable<SelectListItem<ItemIdType>> items, ContentBasedLength width = null, Func<ItemIdType, string> unlistedSelectedItemLabelGetter = null,
			string placeholderText = "Please select", Func<bool, bool> validationPredicate = null, Action validationErrorNotifier = null ) =>
			new DropDownSetup<ItemIdType>(
				width,
				true,
				unlistedSelectedItemLabelGetter,
				placeholderText,
				items,
				null,
				null,
				null,
				null,
				validationPredicate,
				validationErrorNotifier );
	}

	/// <summary>
	/// The configuration for a drop-down.
	/// </summary>
	public class DropDownSetup<ItemIdType> {
		internal readonly ContentBasedLength Width;
		internal readonly bool IsReadOnly;
		internal readonly Func<ItemIdType, string> UnlistedSelectedItemLabelGetter;
		internal readonly string PlaceholderText;
		internal readonly IEnumerable<SelectListItem<ItemIdType>> Items;
		internal readonly FormAction Action;
		internal readonly FormAction SelectionChangedAction;
		internal readonly PageModificationValue<ItemIdType> ItemIdPageModificationValue;
		internal readonly IReadOnlyCollection<ListItemMatchPageModificationSetup<ItemIdType>> ItemMatchPageModificationSetups;
		internal readonly Func<bool, bool> ValidationPredicate;
		internal readonly Action ValidationErrorNotifier;

		internal DropDownSetup(
			ContentBasedLength width, bool isReadOnly, Func<ItemIdType, string> unlistedSelectedItemLabelGetter, string placeholderText,
			IEnumerable<SelectListItem<ItemIdType>> items, FormAction action, FormAction selectionChangedAction,
			PageModificationValue<ItemIdType> itemIdPageModificationValue,
			IReadOnlyCollection<ListItemMatchPageModificationSetup<ItemIdType>> itemMatchPageModificationSetups, Func<bool, bool> validationPredicate,
			Action validationErrorNotifier ) {
			Width = width;
			IsReadOnly = isReadOnly;
			UnlistedSelectedItemLabelGetter = unlistedSelectedItemLabelGetter;
			PlaceholderText = placeholderText;
			Items = items;
			Action = action ?? FormState.Current.DefaultAction;
			SelectionChangedAction = selectionChangedAction;
			ItemIdPageModificationValue = itemIdPageModificationValue;
			ItemMatchPageModificationSetups = itemMatchPageModificationSetups ?? Enumerable.Empty<ListItemMatchPageModificationSetup<ItemIdType>>().Materialize();
			ValidationPredicate = validationPredicate;
			ValidationErrorNotifier = validationErrorNotifier;
		}
	}
}