#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using UnityEngine;

namespace CrazySummerLab
{
    /// <summary>
    /// Use this class to show the iOS 14 App Tracking Transparency native
    /// popup requesting user's authorization to obtain Identifier for Advertising  (IDFA)
    /// </summary>
    public class AppTrackingTransparency
    {
        private static TaskScheduler currentSynchronizationContext;
        static AppTrackingTransparency()
        {
            currentSynchronizationContext = TaskScheduler.FromCurrentSynchronizationContext();
        }

        /// <summary>
        /// Callback invoked once user made a decision through iOS App Tracking Transparency native popup
        /// </summary>
        public static Action<AuthorizationStatus> OnAuthorizationRequestDone;

        #region request app tracking, callback authorization status
        private delegate void AppTrackingTransparencyCallback(int result);
        [MonoPInvokeCallback(typeof(AppTrackingTransparencyCallback))]
        private static void appTrackingTransparencyCallbackReceived(int result)
        {
            Debug.Log(string.Format("UnityAppTrackingTransparencyCallback received: {0}", result));
            // Force to use Default Synchronization context to run callback on Main Thread
            Task.Delay(1).ContinueWith((unused) =>
            {
                if (OnAuthorizationRequestDone != null)
                {
                    switch (result)
                    {
                        case 0:
                            OnAuthorizationRequestDone(AuthorizationStatus.NOT_DETERMINED);
                            break;
                        case 1:
                            OnAuthorizationRequestDone(AuthorizationStatus.RESTRICTED);
                            break;
                        case 2:
                            OnAuthorizationRequestDone(AuthorizationStatus.DENIED);
                            break;
                        case 3:
                            OnAuthorizationRequestDone(AuthorizationStatus.AUTHORIZED);
                            break;
                        default:
                            OnAuthorizationRequestDone(AuthorizationStatus.NOT_DETERMINED);
                            break;
                    }
                }
            }, currentSynchronizationContext);
        }
        [DllImport("__Internal")]
        private static extern void requestTrackingAuthorization(AppTrackingTransparencyCallback callback);
        /// <summary>
        /// Requests iOS Tracking Authorization.
        /// </summary>
        public static void RequestTrackingAuthorization()
        {
#if UNITY_EDITOR
            Debug.Log("Running on Editor platform. Callback invoked with debug result");
            OnAuthorizationRequestDone?.Invoke(AuthorizationStatus.AUTHORIZED);
#else
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                requestTrackingAuthorization(appTrackingTransparencyCallbackReceived);
            }
            else
            {
                Debug.Log(string.Format("Platform '{0}' not supported", Application.platform));
            }
#endif
        }

        [DllImport("__Internal")]
        private static extern string identifierForAdvertising();
        /// <summary>
        /// Obtains iOS Identifier for Advertising (IDFA) if authorized.
        /// </summary>
        /// <returns>The IDFA value if authorized, null otherwise</returns>
        public static string IdentifierForAdvertising()
        {
#if UNITY_EDITOR
            Debug.Log("Running on Editor platform. Callback invoked with debug result");
            return "unity-editor-test-idfa";
#else
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                string idfa = identifierForAdvertising();
                return string.IsNullOrEmpty(idfa) ? null : idfa;
            }else
            {
                Debug.Log(string.Format("Platform '{0}' not supported", Application.platform));
                return null;
            }
#endif
        }

        [DllImport("__Internal")]
        private static extern int trackingAuthorizationStatus();
        /// <summary>
        /// Obtain current Tracking Authorization Status
        /// </summary>
        public static AuthorizationStatus TrackingAuthorizationStatus
        {
            get
            {
#if UNITY_EDITOR
                Debug.Log("Running on Editor platform. Callback invoked with debug result");
                return AuthorizationStatus.AUTHORIZED;
#else
                return (AuthorizationStatus)trackingAuthorizationStatus();
#endif
            }
        }

        [DllImport("__Internal")]
        private static extern void registerAppForAdNetworkAttribution();
        /// <summary>
        ///  Verifies the first launch of an app installed as a result of an ad.
        ///  See https://developer.apple.com/documentation/storekit/skadnetwork/2943654-registerappforadnetworkattributi
        /// </summary>
        public static void RegisterAppForAdNetworkAttribution()
        {
#if UNITY_EDITOR
            Debug.Log("Running on Editor platform. Callback invoked with debug result");
#else
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                registerAppForAdNetworkAttribution();
            }
            else
            {
                Debug.Log(string.Format("Platform '{0}' not supported", Application.platform));
            }
#endif
        }

        [DllImport("__Internal")]
        private static extern void updateConversionValue(int value);
        /// <summary>
        /// Updates the conversion value and verifies the first launch of an app installed as a result of an ad.
        /// See https://developer.apple.com/documentation/storekit/skadnetwork/3566697-updateconversionvalue
        /// </summary>
        /// <param name="value"></param>
        public static void UpdateConversionValue(int value)
        {
#if !UNITY_EDITOR
            Debug.Log("Running on Editor platform. Callback invoked with debug result");
#else
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Debug.Log(string.Format("Updating conversion value to {0}", value));
                updateConversionValue(value);
            }
            else
            {
                Debug.Log(string.Format("Platform '{0}' not supported", Application.platform));
            }
#endif
        }
#endregion
    }

    /// <summary>
    /// Possible App Tracking Transparency authorization status 
    /// </summary>
    public enum AuthorizationStatus
    {
        NOT_DETERMINED,
        /// <summary>
        /// User restrited app tracking. IDFA not available
        /// </summary>
        RESTRICTED,
        /// <summary>
        /// User did not grant access to IDFA
        /// </summary>
        DENIED,
        /// <summary>
        /// You can safely request IDFA
        /// </summary>
        AUTHORIZED
    }
}
#endif