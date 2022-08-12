﻿using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using EnterpriseWebLibrary.Configuration;
using EnterpriseWebLibrary.DataAccess;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EnterpriseWebLibrary.EnterpriseWebFramework.UserManagement;
using EnterpriseWebLibrary.ExternalFunctionality;
using EnterpriseWebLibrary.UserManagement;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Profiling;
using Tewl.Tools;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	public static class EwfOps {
		/// <summary>
		/// Call this from your Program.cs file. Besides this call, there should be no other code in the file.
		/// </summary>
		/// <param name="globalInitializer">The system's global initializer. Do not pass null.</param>
		/// <param name="appInitializer">The application initializer, which performs web-site specific initialization and cleanup. If you have one of these you
		/// should name the class AppInitializer.</param>
		public static int RunApplication( SystemInitializer globalInitializer, SystemInitializer appInitializer = null ) {
			// If the machine was recently started, delay initialization to give database services time to warm up. This avoids errors during data access.
			if( TimeSpan.FromMilliseconds( GetTickCount64() ) < new TimeSpan( 0, 3, 0 ) )
				Thread.Sleep( new TimeSpan( 0, 1, 0 ) );

			var initTimeDataAccessState = new ThreadLocal<DataAccessState>( () => new DataAccessState() );
			GlobalInitializationOps.InitStatics(
				globalInitializer,
				Path.GetFileName( Path.GetDirectoryName( Directory.GetCurrentDirectory() ) ),
				false,
				telemetryAppErrorContextWriter: writer => {
					// This check ensures that there is an actual request, which is not the case during application initialization.
					if( EwfApp.RequestState != null ) {
						writer.WriteLine();
						writer.WriteLine( "URL: " + AppRequestState.Instance.Url );

						writer.WriteLine();
						foreach( var i in EwfRequest.Current.AspNetRequest.Form )
							writer.WriteLine( "Form field " + i.Key + ": " + i.Value.Single() );

						writer.WriteLine();
						foreach( var cookie in EwfRequest.Current.AspNetRequest.Cookies )
							writer.WriteLine( "Cookie " + cookie.Key + ": " + cookie.Value );

						writer.WriteLine();
						writer.WriteLine( "User agent: " + EwfRequest.Current.AspNetRequest.Headers.UserAgent );
						writer.WriteLine( "Referrer: " + EwfRequest.Current.AspNetRequest.Headers.Referer );

						User user = null;
						User impersonator = null;

						// exception-prone code
						try {
							user = AppTools.User;
							impersonator = AppRequestState.Instance.ImpersonatorExists ? AppRequestState.Instance.ImpersonatorUser : null;
						}
						catch {}

						if( user != null )
							writer.WriteLine(
								"User: {0}{1}".FormatWith( user.Email, impersonator != null ? " (impersonated by {0})".FormatWith( impersonator.Email ) : "" ) );
					}
				},
				mainDataAccessStateGetter: () => EwfApp.RequestState != null ? EwfApp.RequestState.DataAccessState : initTimeDataAccessState.Value );
			try {
				return GlobalInitializationOps.ExecuteAppWithStandardExceptionHandling(
					() => {
						try {
							EwfConfigurationStatics.Init();

							var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(
								new WebApplicationOptions
									{
										EnvironmentName = ConfigurationStatics.IsDevelopmentInstallation ? Environments.Development : Environments.Production,
										ContentRootPath = EwfConfigurationStatics.AppConfiguration.Path
									} );
							builder.Services.AddHttpContextAccessor();
							var app = builder.Build();

							var miniProfilerOptions = new MiniProfilerOptions();
							miniProfilerOptions.IgnoredPaths.Clear();
							MiniProfiler.Configure( miniProfilerOptions );

							var programType = new StackTrace().GetFrames()
								.Select( frame => frame.GetMethod().DeclaringType )
								.First( type => type.Assembly != Assembly.GetExecutingAssembly() );
							var providerGetter = new SystemProviderGetter(
								programType.Assembly,
								programType.Namespace + ".Providers",
								providerName =>
									@"{0} provider not found in application. To implement, create a class named {0} in ""Your Web Site\Providers"" that derives from App{0}Provider."
										.FormatWith( providerName ) );

							if( ExternalFunctionalityStatics.SamlFunctionalityEnabled )
								ExternalFunctionalityStatics.ExternalSamlProvider.InitAppStatics(
									providerGetter,
									() => AuthenticationStatics.SamlIdentityProviders.Select(
											identityProvider => {
												using( var client = new HttpClient() ) {
													client.Timeout = new TimeSpan( 0, 0, 10 );
													var metadata = Task.Run(
															async () => {
																using( var response = await client.GetAsync( identityProvider.MetadataUrl, HttpCompletionOption.ResponseHeadersRead ) ) {
																	response.EnsureSuccessStatusCode();
																	var document = new XmlDocument();
																	using( var stream = await response.Content.ReadAsStreamAsync() )
																		using( var reader = XmlReader.Create( stream ) )
																			document.Load( reader );
																	return document.DocumentElement;
																}
															} )
														.Result;
													return ( metadata, identityProvider.EntityId );
												}
											} )
										.Materialize() );

							var contextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
							EwfRequest.Init( providerGetter.GetProvider<AppRequestBaseUrlProvider>( "RequestBaseUrl" ), () => contextAccessor.HttpContext.Request );
							UrlHandlingStatics.Init(
								() => RequestDispatchingStatics.AppProvider.GetBaseUrlPatterns(),
								( baseUrlString, appRelativeUrl ) =>
									AppRequestState.ExecuteWithUrlHandlerStateDisabled( () => UrlHandlingStatics.ResolveUrl( baseUrlString, appRelativeUrl )?.Last() ) );
							CookieStatics.Init(
								( name, value, options ) => {
									var cookies = contextAccessor.HttpContext.Response.Cookies;
									if( value != null )
										cookies.Append( name, value, options );
									else
										cookies.Delete( name, options );
								} );
							Translation.Init( () => "en-US" );
							CssPreprocessingStatics.Init( globalInitializer.GetType().Assembly, programType.Assembly );
							ResourceBase.Init(
								( requestTransferred, resource ) => {
									if( requestTransferred ) {
										var urlHandlers = new List<BasicUrlHandler>();
										UrlHandler urlHandler = resource;
										do
											urlHandlers.Add( urlHandler );
										while( ( urlHandler = urlHandler.GetParent() ) != null );
										AppRequestState.Instance.SetUrlHandlers( urlHandlers );

										AppRequestState.Instance.SetNewUrlParameterValuesEffective( false );
										AppRequestState.Instance.SetResource( resource );
									}
									else
										AppRequestState.Instance.SetResource( resource );
								},
								() => AppRequestState.Instance.Resource );
							StaticFile.Init( providerGetter.GetProvider<AppStaticFileHandlingProvider>( "StaticFileHandling" ) );
							PageBase.Init(
								( () => BasePageStatics.AppProvider.GetPageViewDataModificationMethod(),
									() => BasePageStatics.AppProvider.JavaScriptDocumentReadyFunctionCall ),
								BasicPageContent.GetContent );
							HyperlinkBehaviorExtensionCreators.Init( ModalBox.GetBrowsingModalBoxOpenStatements );
							FileUpload.Init( () => ( (BasicPageContent)PageBase.Current.BasicContent ).FormUsesMultipartEncoding = true );
							ModalBox.Init( () => ( (BasicPageContent)PageBase.Current.BasicContent ).BrowsingModalBoxId );
							CreditCardCollector.Init( () => ( (BasicPageContent)PageBase.Current.BasicContent ).IncludesStripeCheckout = true );
							BasePageStatics.Init( providerGetter.GetProvider<AppStandardPageLogicProvider>( "StandardPageLogic" ) );
							BasicPageContent.Init(
								contentObjects => {
									var contentUsesUi = contentObjects.Any( i => i is UiPageContent );

									var cssInfos = new List<ResourceInfo>();
									cssInfos.Add(
										new ExternalResource(
											"//fonts.googleapis.com/css2?family=Libre+Franklin:wght@500;600;700&family=Open+Sans:ital,wght@0,400;0,600;0,700;1,400&display=fallback" ) );
									cssInfos.Add( new ExternalResource( "//maxcdn.bootstrapcdn.com/font-awesome/4.5.0/css/font-awesome.min.css" ) );
									cssInfos.Add( new StaticFiles.Versioned.Third_party.Jquery_ui.Jquery_ui_1114custom_v2.Jquery_uiminCss() );
									cssInfos.Add( new StaticFiles.Third_party.Select_cssCss() );
									cssInfos.Add( new StaticFiles.Versioned.Third_party.Chosen.Chosen_v187.ChosenminCss() );
									cssInfos.Add( new StaticFiles.Third_party.Time_picker.StylesCss() );
									cssInfos.Add( new ExternalResource( "//cdn.jsdelivr.net/qtip2/2.2.1/jquery.qtip.min.css" ) );
									cssInfos.Add( new ExternalResource( "//cdnjs.cloudflare.com/ajax/libs/dialog-polyfill/0.4.9/dialog-polyfill.min.css" ) );
									cssInfos.Add( new StaticFiles.Styles.BasicCss() );
									if( contentUsesUi )
										cssInfos.AddRange(
											new ResourceInfo[]
												{
													new StaticFiles.Styles.Ui.ColorsCss(), new StaticFiles.Styles.Ui.FontsCss(), new StaticFiles.Styles.Ui.LayoutCss(),
													new StaticFiles.Styles.Ui.TransitionsCss()
												} );
									foreach( var resource in BasePageStatics.AppProvider.GetStyleSheets() ) {
										assertResourceIsIntermediateInstallationPublicResourceWhenNecessary( resource );
										cssInfos.Add( resource );
									}
									if( contentUsesUi )
										foreach( var resource in EwfUiStatics.AppProvider.GetStyleSheets() ) {
											assertResourceIsIntermediateInstallationPublicResourceWhenNecessary( resource );
											cssInfos.Add( resource );
										}
									else
										foreach( var resource in BasePageStatics.AppProvider.GetCustomUiStyleSheets() ) {
											assertResourceIsIntermediateInstallationPublicResourceWhenNecessary( resource );
											cssInfos.Add( resource );
										}
									return cssInfos;
								},
								( markup, includeStripeCheckout ) => {
									string getElement( ResourceInfo resource ) => "<script src=\"{0}\" defer></script>".FormatWith( resource.GetUrl() );

									var infos = new List<ResourceInfo>();
									infos.Add( new ExternalResource( "//code.jquery.com/jquery-1.12.3.min.js" ) );
									infos.Add( new StaticFiles.Versioned.Third_party.Jquery_ui.Jquery_ui_1114custom_v2.Jquery_uiminJs() );
									infos.Add( new StaticFiles.Versioned.Third_party.Chosen.Chosen_v187.ChosenjqueryminJs() );
									infos.Add( new StaticFiles.Third_party.Time_picker.CodeJs() );
									infos.Add( new ExternalResource( "//cdn.jsdelivr.net/qtip2/2.2.1/jquery.qtip.min.js" ) );
									infos.Add( new ExternalResource( "//cdnjs.cloudflare.com/ajax/libs/dialog-polyfill/0.4.9/dialog-polyfill.min.js" ) );
									infos.Add( new StaticFiles.Third_party.Spin_js.SpinminJs() );
									infos.Add( new ExternalResource( "//cdn.ckeditor.com/4.5.8/full/ckeditor.js" ) );
									infos.Add( new ExternalResource( "https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.9.4/Chart.min.js" ) );
									infos.Add( new ExternalResource( "https://instant.page/5.1.0" ) );
									if( includeStripeCheckout )
										infos.Add( new ExternalResource( "https://checkout.stripe.com/checkout.js" ) );
									infos.Add( new StaticFiles.CodeJs() );
									foreach( var i in infos.Select( getElement ) )
										markup.Append( i );
									markup.Append( MiniProfiler.Current.RenderIncludes( contextAccessor.HttpContext ) );
									foreach( var resource in BasePageStatics.AppProvider.GetJavaScriptFiles() ) {
										assertResourceIsIntermediateInstallationPublicResourceWhenNecessary( resource );
										markup.Append( getElement( resource ) );
									}
								},
								() => {
									var icons = new List<( ResourceInfo, string, string )>();

									var faviconPng48X48 = BasePageStatics.AppProvider.FaviconPng48X48;
									if( faviconPng48X48 != null ) {
										assertResourceIsIntermediateInstallationPublicResourceWhenNecessary( faviconPng48X48 );
										icons.Add( ( faviconPng48X48, "icon", "48x48" ) );
									}

									var favicon = BasePageStatics.AppProvider.Favicon;
									if( favicon != null ) {
										assertResourceIsIntermediateInstallationPublicResourceWhenNecessary( favicon );
										icons.Add( ( favicon, "icon", "" ) );
									}

									return icons;
								},
								hideWarnings => {
									var url = AppRequestState.Instance.Url;
									if( AppRequestState.Instance.UserAccessible && AppRequestState.Instance.ImpersonatorExists )
										url = new UserManagement.Pages.Impersonate(
											url,
											optionalParameterSetter: ( specifier, parameters ) =>
												specifier.User = AppTools.User != null ? AppTools.User.Email : UserManagement.Pages.Impersonate.AnonymousUser ).GetUrl();
									return new NonLiveLogIn(
										url,
										optionalParameterSetter: ( specifier, parameters ) => {
											specifier.Password = ConfigurationStatics.SystemGeneralProvider.IntermediateLogInPassword;
											specifier.HideWarnings = hideWarnings;
										} ).GetUrl();
								},
								() => {
									if( !AppRequestState.Instance.UserAccessible || !AppRequestState.Instance.ImpersonatorExists ||
									    ( ConfigurationStatics.IsIntermediateInstallation && !AppRequestState.Instance.IntermediateUserExists ) )
										return null;
									return ( "User impersonation is in effect.",
										       new HyperlinkSetup( new UserManagement.Pages.Impersonate( AppRequestState.Instance.Url ), "Change user" )
											       .Append<ActionComponentSetup>(
												       new ButtonSetup(
													       "End impersonation",
													       behavior: new PostBackBehavior(
														       postBack: PostBack.CreateFull(
															       id: "ewfEndImpersonation",
															       modificationMethod: UserImpersonationStatics.EndImpersonation,
															       actionGetter: () => new PostBackAction(
																       new ExternalResource(
																	       EwfConfigurationStatics.AppConfiguration.DefaultBaseUrl.GetUrlString(
																		       EwfConfigurationStatics.AppSupportsSecureConnections ) ) ) ) ) ) )
											       .Materialize() );
								} );
							EwfUiStatics.Init( providerGetter.GetProvider<AppEwfUiProvider>( "EwfUi" ) );
							AuthenticationStatics.Init(
								providerGetter.GetProvider<AppAuthenticationProvider>( "Authentication" ),
								app.Services.GetRequiredService<IDataProtectionProvider>(),
								( user, code ) => new UserManagement.Pages.LogIn(
									"",
									optionalParameterSetter: ( specifier, parameters ) => {
										specifier.User = user;
										specifier.Code = code;
									} ).GetUrl(),
								destinationUrl => new UserManagement.Pages.ChangePassword( destinationUrl ).GetUrl( disableAuthorizationCheck: true ) );
							Admin.EntitySetup.Init( () => RequestDispatchingStatics.AppProvider.GetFrameworkUrlParent() );
							RequestDispatchingStatics.Init(
								providerGetter.GetProvider<AppRequestDispatchingProvider>( "RequestDispatching" ),
								() => contextAccessor.HttpContext );

							appInitializer?.InitStatics();

							executeWithAutomaticDatabaseConnections( AuthenticationStatics.InitAppSpecificLogicDependencies );
							if( AuthenticationStatics.SamlIdentityProviders.Any() || ExternalFunctionalityStatics.SamlFunctionalityEnabled )
								executeWithAutomaticDatabaseConnections( ExternalFunctionalityStatics.ExternalSamlProvider.InitAppSpecificLogicDependencies );

							initTimeDataAccessState = null;

							app.Use( RequestDispatchingStatics.ProcessRequest );
							app.UseRouting();
							app.Use( EwfApp.EnsureUrlResolved );
							app.UseMiniProfiler();
							app.Run();
						}
						finally {
							appInitializer?.CleanUpStatics();
						}
					} );
			}
			finally {
				GlobalInitializationOps.CleanUpStatics();
			}
		}

		[ DllImport( "kernel32" ) ]
		private static extern ulong GetTickCount64();

		private static void assertResourceIsIntermediateInstallationPublicResourceWhenNecessary( ResourceInfo resource ) {
			if( !PageBase.Current.IsIntermediateInstallationPublicResource )
				return;
			if( resource is ResourceBase appResource && !appResource.IsIntermediateInstallationPublicResource )
				throw new Exception(
					"You must specify resource {0} as an intermediate-installation public resource because it is used on an intermediate-installation public page."
						.FormatWith( resource.GetUrl( false, false ) ) );
		}

		private static void executeWithAutomaticDatabaseConnections( Action method ) {
			var connectionManager = new AutomaticDatabaseConnectionManager();
			try {
				connectionManager.DataAccessState.ExecuteWithThis( method );
				connectionManager.CommitTransactionsAndExecuteNonTransactionalModificationMethods( false );
			}
			catch {
				connectionManager.RollbackTransactions( false );
				throw;
			}
		}
	}
}