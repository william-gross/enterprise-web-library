﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.InputValidation;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// Contains metadata about a control, such as what it is called, ways in which it can be displayed, how it should be validated, etc.
	/// </summary>
	public abstract class FormItem {
		/// <summary>
		/// Creates a form item with the given label and control.
		/// </summary>
		// By taking a FormItemLabel instead of a Control for label, we retain the ability to implement additional behavior for string labels, such as automatically
		// making them bold.
		public static FormItem<ControlType> Create<ControlType>(
			FormItemLabel label, ControlType control, FormItemSetup setup = null, Func<ControlType, EwfValidation> validationGetter = null ) where ControlType: Control {
			return new FormItem<ControlType>( setup, label, control, validationGetter?.Invoke( control ) );
		}

		internal readonly FormItemSetup Setup;
		private readonly FormItemLabel label;
		private readonly Control control;
		public readonly EwfValidation Validation;

		/// <summary>
		/// Creates a form item.
		/// </summary>
		protected FormItem( FormItemSetup setup, FormItemLabel label, Control control, EwfValidation validation ) {
			Setup = setup ?? new FormItemSetup();
			this.label = label ?? throw new ApplicationException( "The label cannot be a null FormItemLabel reference." );
			this.control = control;
			Validation = validation;
		}

		/// <summary>
		/// Gets the label.
		/// </summary>
		public virtual Control Label => label.Text != null
			                                ? label.Text.Any()
				                                  ? new PlaceHolder().AddControlsReturnThis( label.Text.ToComponents().GetControls() )
				                                  : null
			                                : label.Control;

		/// <summary>
		/// Gets the control.
		/// </summary>
		public virtual Control Control => control;

		/// <summary>
		/// Creates a labeled control for this form item.
		/// This can be used to insert controls to a page without a <see cref="FormItemBlock"/> and display inline error messages.
		/// </summary>
		public LabeledControl ToControl( bool omitLabel = false ) {
			return new LabeledControl( omitLabel ? null : Label, control, Validation );
		}
	}

	/// <summary>
	/// Contains metadata about a control, such as what it is called, ways in which it can be displayed, how it should be validated, etc.
	/// </summary>
	public class FormItem<ControlType>: FormItem where ControlType: Control {
		private readonly ControlType control;

		internal FormItem( FormItemSetup setup, FormItemLabel label, ControlType control, EwfValidation validation ): base( setup, label, control, validation ) {
			this.control = control;
		}

		/// <summary>
		/// Gets the control.
		/// </summary>
		public new ControlType Control => control;
	}

	public static class FormItemExtensionCreators {
		public static FormItem ToTextControlFormItem(
			this DataValue<string> dataValue, IEnumerable<PhrasingComponent> label, bool allowEmpty, FormItemSetup formItemSetup = null,
			TextControlSetup controlSetup = null, string value = null, int? maxLength = null, Action<Validator> additionalValidationMethod = null ) {
			return new TextControl(
				value ?? dataValue.Value,
				allowEmpty,
				( postBackValue, validator ) => {
					dataValue.Value = postBackValue;
					additionalValidationMethod?.Invoke( validator );
				},
				setup: controlSetup,
				maxLength: maxLength ).ToFormItem( label, setup: formItemSetup );
		}

		public static FormItem ToHtmlEditorFormItem(
			this DataValue<string> dataValue, IEnumerable<PhrasingComponent> label, bool allowEmpty, FormItemSetup formItemSetup = null,
			WysiwygHtmlEditorSetup editorSetup = null, string value = null, int? maxLength = null, Action<Validator> additionalValidationMethod = null ) {
			return new WysiwygHtmlEditor(
				value ?? dataValue.Value,
				allowEmpty,
				( postBackValue, validator ) => {
					dataValue.Value = postBackValue;
					additionalValidationMethod?.Invoke( validator );
				},
				setup: editorSetup,
				maxLength: maxLength ).ToFormItem( label, setup: formItemSetup );
		}

		public static FormItem ToBlockCheckboxFormItem(
			this DataValue<bool> dataValue, IEnumerable<PhrasingComponent> label, FormItemSetup formItemSetup = null,
			IEnumerable<PhrasingComponent> formItemLabel = null, BlockCheckBoxSetup checkboxSetup = null, bool? value = null,
			Action<Validator> additionalValidationMethod = null ) {
			return new BlockCheckBox(
				value ?? dataValue.Value,
				( postBackValue, validator ) => {
					dataValue.Value = postBackValue.Value;
					additionalValidationMethod?.Invoke( validator );
				},
				setup: checkboxSetup,
				label: label ).ToFormItem( formItemLabel, setup: formItemSetup );
		}

		public static FormItem ToBlockCheckboxFormItem(
			this DataValue<decimal> dataValue, IEnumerable<PhrasingComponent> label, FormItemSetup formItemSetup = null,
			IEnumerable<PhrasingComponent> formItemLabel = null, BlockCheckBoxSetup checkboxSetup = null, decimal? value = null,
			Action<Validator> additionalValidationMethod = null ) {
			var boolValue = new DataValue<bool> { Value = ( value ?? dataValue.Value ).DecimalToBoolean() };
			return boolValue.ToBlockCheckboxFormItem(
				label,
				formItemSetup: formItemSetup,
				formItemLabel: formItemLabel,
				checkboxSetup: checkboxSetup,
				additionalValidationMethod: validator => {
					dataValue.Value = boolValue.Value.BooleanToDecimal();
					additionalValidationMethod?.Invoke( validator );
				} );
		}

		/// <summary>
		/// Creates a form item with this form control and the specified label.
		/// </summary>
		/// <param name="formControl"></param>
		/// <param name="label">Do not pass null.</param>
		/// <param name="setup"></param>
		public static FormItem ToFormItem( this FormControl<FlowComponent> formControl, IEnumerable<PhrasingComponent> label, FormItemSetup setup = null ) {
			// Web Forms compatibility. Remove when EnduraCode goal 790 is complete.
			if( formControl is WebControl webControl )
				return new FormItem<Control>(
					setup,
					label.Any()
						? (FormItemLabel)new PlaceHolder().AddControlsReturnThis(
							( formControl.Labeler != null ? formControl.Labeler.CreateLabel( label ) : label ).GetControls() )
						: "",
					webControl,
					formControl.Validation );

			return formControl.PageComponent.ToCollection()
				.ToFormItem( formControl.Labeler != null ? formControl.Labeler.CreateLabel( label ) : label, setup: setup, validation: formControl.Validation );
		}

		/// <summary>
		/// Creates a form item with these components and the specified label.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="label">Do not pass null.</param>
		/// <param name="setup"></param>
		/// <param name="validation"></param>
		public static FormItem ToFormItem(
			this IEnumerable<FlowComponent> content, IEnumerable<PhrasingComponent> label, FormItemSetup setup = null, EwfValidation validation = null ) {
			return new FormItem<Control>(
				setup,
				label.Any() ? (FormItemLabel)new PlaceHolder().AddControlsReturnThis( label.GetControls() ) : "",
				new PlaceHolder().AddControlsReturnThis( content.GetControls() ),
				validation );
		}
	}
}