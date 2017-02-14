﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using EnterpriseWebLibrary.InputValidation;
using Humanizer;

namespace EnterpriseWebLibrary.EnterpriseWebFramework.Controls {
	/// <summary>
	/// A date and time picker.
	/// </summary>
	public class DateTimePicker: WebControl, ControlTreeDataLoader, ControlWithJsInitLogic, ControlWithCustomFocusLogic {
		internal class CssElementCreator: ControlCssElementCreator {
			internal const string CssClass = "ewfDateAndTimePicker";

			IReadOnlyCollection<CssElement> ControlCssElementCreator.CreateCssElements() {
				return new[] { new CssElement( "DateAndTimePicker", "div." + CssClass ) };
			}
		}

		private DateTime? value;
		private bool autoPostBack;
		private DateTime? minDate;
		private DateTime? maxDate;
		private bool constrainToSqlSmallDateTimeRange = true;
		private readonly int minuteInterval;
		private readonly IReadOnlyCollection<DataModification> dataModifications;

		private EwfTextBox textBox;
		private DateTime min;
		private DateTime max;

		/// <summary>
		/// Creates a date and time picker. The minute interval affects the slider but does not prevent other values from passing validation.
		/// </summary>
		public DateTimePicker( DateTime? value, int minuteInterval = 15 ) {
			this.value = value;
			this.minuteInterval = minuteInterval;

			dataModifications = ValidationSetupState.Current.DataModifications;
		}

		/// <summary>
		/// Sets whether this date picker will post back automatically.
		/// </summary>
		public bool AutoPostBack { set { autoPostBack = value; } }

		/// <summary>
		/// The earliest acceptable date.
		/// </summary>
		public DateTime MinDate { set { minDate = value; } }

		/// <summary>
		/// The latest acceptable date.
		/// </summary>
		public DateTime MaxDate { set { maxDate = value; } }

		/// <summary>
		/// Constrains the acceptable dates to those accepted by Sql's small date time type. This defaults to true.
		/// </summary>
		public bool ConstrainToSqlSmallDateTimeRange { set { constrainToSqlSmallDateTimeRange = value; } }

		/// <summary>
		/// EWF ToolTip to display on this control. Setting ToolTipControl will ignore this property.
		/// </summary>
		public override string ToolTip { get; set; }

		/// <summary>
		/// Control to display inside the tool tip. Do not pass null. This will ignore the ToolTip property.
		/// </summary>
		public Control ToolTipControl { get; set; }

		void ControlTreeDataLoader.LoadData() {
			ValidationSetupState.ExecuteWithDataModifications(
				dataModifications,
				() => {
					CssClass = CssClass.ConcatenateWithSpace( CssElementCreator.CssClass );

					textBox = new EwfTextBox(
						value.HasValue ? value.Value.ToMonthDayYearString() + " " + value.Value.ToHourAndMinuteString() : "",
						disableBrowserAutoComplete: true,
						autoPostBack: autoPostBack );
					Controls.Add( new ControlLine( textBox, getIconButton() ) );

					min = DateTime.MinValue;
					max = DateTime.MaxValue;
					if( constrainToSqlSmallDateTimeRange ) {
						min = Validator.SqlSmallDateTimeMinValue;
						max = Validator.SqlSmallDateTimeMaxValue;
					}
					if( minDate.HasValue && minDate.Value > min )
						min = minDate.Value;
					if( maxDate.HasValue && maxDate.Value < max )
						max = maxDate.Value;

					if( ToolTip != null || ToolTipControl != null )
						new ToolTip( ToolTipControl ?? EnterpriseWebFramework.Controls.ToolTip.GetToolTipTextControl( ToolTip ), this );
				} );
		}

		private WebControl getIconButton() {
			return new CustomButton( () => "$( '#{0}' ).datetimepicker( 'show' )".FormatWith( textBox.TextBoxClientId ) )
				{
					ActionControlStyle =
						new CustomActionControlStyle(
							control =>
							control.AddControlsReturnThis(
								new GenericPhrasingContainer(
								new[] { new FontAwesomeIcon( "fa-calendar-o", "fa-stack-2x" ), new FontAwesomeIcon( "fa-clock-o", "fa-stack-1x" ) },
								classes: new ElementClass( "fa-stack" ).Add( new ElementClass( "datetimepickerIcon" ) ) ).ToCollection().GetControls() ) ),
					CssClass = "icon"
				};
		}

		string ControlWithJsInitLogic.GetJsInitStatements() {
			return "$( '#" + textBox.TextBoxClientId + "' ).datetimepicker( { minDate: new Date( " + min.Year + ", " + min.Month + " - 1, " + min.Day +
			       " ), maxDate: new Date( " + max.Year + ", " + max.Month + " - 1, " + max.Day + " ), timeFormat: 'h:mmt', stepMinute: " + minuteInterval + " } );";
		}

		void ControlWithCustomFocusLogic.SetFocus() {
			( textBox as ControlWithCustomFocusLogic ).SetFocus();
		}

		/// <summary>
		/// Validates the date and returns the nullable date.
		/// </summary>
		public DateTime? ValidateAndGetNullablePostBackDate(
			PostBackValueDictionary postBackValues, Validator validator, ValidationErrorHandler errorHandler, bool allowEmpty ) {
			return validator.GetNullableDateTime(
				errorHandler,
				textBox.GetPostBackValue( postBackValues ).ToUpper(),
				DateTimeTools.MonthDayYearFormats.Select( i => i + " " + DateTimeTools.HourAndMinuteFormat ).ToArray(),
				allowEmpty,
				min,
				max );
		}

		/// <summary>
		/// Validates the date and returns the date.
		/// </summary>
		public DateTime ValidateAndGetPostBackDate( PostBackValueDictionary postBackValues, Validator validator, ValidationErrorHandler errorHandler ) {
			return validator.GetDateTime(
				errorHandler,
				textBox.GetPostBackValue( postBackValues ).ToUpper(),
				DateTimeTools.MonthDayYearFormats.Select( i => i + " " + DateTimeTools.HourAndMinuteFormat ).ToArray(),
				min,
				max );
		}

		/// <summary>
		/// Returns true if the value changed on this post back.
		/// </summary>
		public bool ValueChangedOnPostBack( PostBackValueDictionary postBackValues ) {
			return textBox.ValueChangedOnPostBack( postBackValues );
		}

		/// <summary>
		/// Returns the div tag, which represents this control in HTML.
		/// </summary>
		protected override HtmlTextWriterTag TagKey => HtmlTextWriterTag.Div;
	}
}