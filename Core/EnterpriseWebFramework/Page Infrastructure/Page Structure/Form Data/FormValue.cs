﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EnterpriseWebLibrary.InputValidation;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	public interface FormValue {
		/// <summary>
		/// Returns the empty string if this form value is inactive.
		/// </summary>
		string GetPostBackValueKey();

		string GetDurableValueAsString();
		bool PostBackValueIsInvalid( PostBackValueDictionary postBackValues );
		bool ValueChangedOnPostBack( PostBackValueDictionary postBackValues );
		void SetPageModificationValues( PostBackValueDictionary postBackValues );
	}

	internal class FormValue<T>: FormValue {
		private readonly Func<T> durableValueGetter;

		// This should not be called until after control tree validation, to enable scenarios such as RadioButtonGroup using its first on-page button's UniqueID as
		// the key.
		private readonly Func<string> postBackValueKeyGetter;

		private readonly Func<T, string> stringValueSelector;
		private readonly Func<string, PostBackValueValidationResult<T>> stringPostBackValueValidator;
		private readonly Func<HttpPostedFile, PostBackValueValidationResult<T>> filePostBackValueValidator;
		private readonly List<Action<T>> pageModificationValueAdders = new List<Action<T>>();

		/// <summary>
		/// Creates a form value.
		/// </summary>
		/// <param name="durableValueGetter"></param>
		/// <param name="postBackValueKeyGetter"></param>
		/// <param name="stringValueSelector"></param>
		/// <param name="stringPostBackValueValidator">Avoid using exceptions in this method if possible since it is sometimes called many times during a request,
		/// and we've seen exceptions take as long as 50 ms each when debugging.</param>
		public FormValue(
			Func<T> durableValueGetter, Func<string> postBackValueKeyGetter, Func<T, string> stringValueSelector,
			Func<string, PostBackValueValidationResult<T>> stringPostBackValueValidator ) {
			this.durableValueGetter = durableValueGetter;
			this.postBackValueKeyGetter = postBackValueKeyGetter;
			this.stringValueSelector = stringValueSelector;
			this.stringPostBackValueValidator = stringPostBackValueValidator;

			// This dependency on EwfPage should not exist.
			EwfPage.Instance.AddFormValue( this );
		}

		/// <summary>
		/// Creates a form value.
		/// </summary>
		/// <param name="durableValueGetter"></param>
		/// <param name="postBackValueKeyGetter"></param>
		/// <param name="stringValueSelector"></param>
		/// <param name="filePostBackValueValidator">Avoid using exceptions in this method if possible since it is sometimes called many times during a request, and
		/// we've seen exceptions take as long as 50 ms each when debugging.</param>
		public FormValue(
			Func<T> durableValueGetter, Func<string> postBackValueKeyGetter, Func<T, string> stringValueSelector,
			Func<HttpPostedFile, PostBackValueValidationResult<T>> filePostBackValueValidator ) {
			this.durableValueGetter = durableValueGetter;
			this.postBackValueKeyGetter = postBackValueKeyGetter;
			this.stringValueSelector = stringValueSelector;
			this.filePostBackValueValidator = filePostBackValueValidator;

			// This dependency on EwfPage should not exist.
			EwfPage.Instance.AddFormValue( this );
		}

		/// <summary>
		/// Adds the specified page-modification value.
		/// </summary>
		public void AddPageModificationValue<ModificationValueType>(
			PageModificationValue<ModificationValueType> pageModificationValue, Func<T, ModificationValueType> modificationValueSelector ) {
			pageModificationValueAdders.Add( value => pageModificationValue.AddValue( modificationValueSelector( value ) ) );
		}

		/// <summary>
		/// Creates a validation with the specified method and adds it to the current data modifications.
		/// </summary>
		/// <param name="validationMethod">The method that will be called by the data modification(s) to which this validation is added.</param>
		public EwfValidation CreateValidation( Action<PostBackValue<T>, Validator> validationMethod ) {
			return new EwfValidation(
				validator => validationMethod(
					new PostBackValue<T>(
						getValue( AppRequestState.Instance.EwfPageRequestState.PostBackValues ),
						valueChangedOnPostBack( AppRequestState.Instance.EwfPageRequestState.PostBackValues ) ),
					validator ) );
		}

		public T GetDurableValue() {
			return durableValueGetter();
		}

		string FormValue.GetPostBackValueKey() {
			return postBackValueKeyGetter();
		}

		string FormValue.GetDurableValueAsString() {
			return stringValueSelector( durableValueGetter() );
		}

		bool FormValue.PostBackValueIsInvalid( PostBackValueDictionary postBackValues ) {
			var key = postBackValueKeyGetter();
			return !postBackValues.KeyRemoved( key ) && !validatePostBackValue( postBackValues.GetValue( key ) ).IsValid;
		}

		bool FormValue.ValueChangedOnPostBack( PostBackValueDictionary postBackValues ) {
			return valueChangedOnPostBack( postBackValues );
		}

		private bool valueChangedOnPostBack( PostBackValueDictionary postBackValues ) => !EwlStatics.AreEqual( getValue( postBackValues ), durableValueGetter() );

		void FormValue.SetPageModificationValues( PostBackValueDictionary postBackValues ) {
			foreach( var i in pageModificationValueAdders )
				i( getValue( postBackValues ) );
		}

		private T getValue( PostBackValueDictionary postBackValues ) {
			var key = postBackValueKeyGetter();
			if( !key.Any() || postBackValues == null || postBackValues.KeyRemoved( key ) )
				return durableValueGetter();
			var result = validatePostBackValue( postBackValues.GetValue( key ) );
			return result.IsValid ? result.Value : durableValueGetter();
		}

		private PostBackValueValidationResult<T> validatePostBackValue( object value ) {
			if( filePostBackValueValidator != null ) {
				var fileValue = value as HttpPostedFile;
				return value == null || fileValue != null ? filePostBackValueValidator( fileValue ) : PostBackValueValidationResult<T>.CreateInvalid();
			}

			var stringValue = value as string;
			return value == null || stringValue != null ? stringPostBackValueValidator( stringValue ) : PostBackValueValidationResult<T>.CreateInvalid();
		}
	}
}