#if UNITY_IOS
using CrazySummerLab;
using UnityEngine;

public class AppTrackingExample : MonoBehaviour
{
    private void Start()
    {
        // Register your callback to get notiﬁed with the authorization result and Rrequest tracking authorization.
        AppTrackingTransparency.OnAuthorizationRequestDone += OnAuthorizationRequestDoneHandle;
        AppTrackingTransparency.RequestTrackingAuthorization();
    }

    /// <summary>
    /// Callback invoked with the user's decision 
    /// </summary>
    /// <param name="status"></param>
    private void OnAuthorizationRequestDoneHandle(AuthorizationStatus status)
    {
        switch (status)
        {
            case AuthorizationStatus.NOT_DETERMINED:
                Debug.Log("AuthorizationStatus: NOT_DETERMINED");
                break;
            case AuthorizationStatus.RESTRICTED:
                Debug.Log("AuthorizationStatus: RESTRICTED");
                break;
            case AuthorizationStatus.DENIED:
                Debug.Log("AuthorizationStatus: DENIED");
                break;
            case AuthorizationStatus.AUTHORIZED:
                Debug.Log("AuthorizationStatus: AUTHORIZED");
                break;
        }
        // Obtain IDFA
        Debug.Log($"IDFA: {AppTrackingTransparency.IdentifierForAdvertising()}");
    }
}
#endif