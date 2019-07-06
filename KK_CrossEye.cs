using BepInEx;
using UnityEngine;
using System.ComponentModel;

[BepInPlugin(nameof(KK_CrossEye), nameof(KK_CrossEye), "1.0")]
public class KK_CrossEye : BaseUnityPlugin {
    private Camera mainCamera;
    private GameObject leftCameraObject;
    private GameObject rightCameraObject;

    private Vector3 offset = Vector3.left * 0.18f;

    private float oldFocus;
    private float currentFocus;

    private bool CrossEye_Enabled = false;

    private Camera LeftCamera;
    private Camera RightCamera;

    #region Config properties
        [DisplayName("CrossEye mode IPD")]
        [Description("Eye separation distance.")]
        [AcceptableValueRange(0f, 1f, false)]
        public static ConfigWrapper<float> CrossEye_IPD { get; private set; }

        public static SavedKeyboardShortcut CrossEye_EnableKey { get; private set; }
    #endregion

    void Awake() {
        CrossEye_EnableKey = new SavedKeyboardShortcut("CrossEye mode Key", this, new KeyboardShortcut(KeyCode.Keypad1));
        CrossEye_IPD = new ConfigWrapper<float>("CrossEye mode IPD", this, 0.18f);
    }

    void Update() {
        if (CrossEye_EnableKey.IsDown()) {
            CrossEye_Enabled = !CrossEye_Enabled;

            offset = Vector3.left * CrossEye_IPD.Value;

            if (CrossEye_Enabled) {

                oldFocus = 0f;
                currentFocus = 0f;

                CrossEye_Init();
            } else { 
                CrossEye_Kill(); 
            }
        }

        if (CrossEye_Enabled) {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && (hit.distance <= 1.2f) && (hit.transform.name.Contains("cf_") || hit.transform.name.Contains("aibu_"))) {
                oldFocus = currentFocus;
                currentFocus = 12f - (hit.distance * 10);

                LeftCamera.transform.Rotate(0, oldFocus, 0);
                RightCamera.transform.Rotate(0, -oldFocus, 0);

                LeftCamera.transform.Rotate(0, -currentFocus, 0);
                RightCamera.transform.Rotate(0, currentFocus, 0);
            } else {
                if (currentFocus > 0f) {
                    LeftCamera.transform.Rotate(0, currentFocus, 0);
                    RightCamera.transform.Rotate(0, -currentFocus, 0);

                    oldFocus = 0f;
                    currentFocus = 0f;
                }
            }
        }

    }

    void CrossEye_Init() {
        mainCamera = Camera.main;

        leftCameraObject = GameObject.Instantiate(mainCamera.gameObject);
        rightCameraObject = GameObject.Instantiate(mainCamera.gameObject);

        var baddies = new[] {
            typeof(Studio.CameraControl),
            typeof(BaseCameraControl_Ver2),
            typeof(CameraEffector),
            typeof(CameraEffectorConfig),
            typeof(GUILayer),
            typeof(CapsuleCollider),
            typeof(Rigidbody)
        };

        foreach (var b in baddies) {
            GameObject.DestroyImmediate(leftCameraObject.gameObject.GetComponent(b));
            GameObject.DestroyImmediate(rightCameraObject.gameObject.GetComponent(b));
        }

        LeftCamera = leftCameraObject.GetComponent<Camera>();
        RightCamera = rightCameraObject.GetComponent<Camera>();

        LeftCamera.CopyFrom(mainCamera);
        RightCamera.CopyFrom(mainCamera);

        leftCameraObject.transform.SetParent(mainCamera.gameObject.transform, false);
        leftCameraObject.transform.localPosition = Vector3.zero;
        leftCameraObject.transform.localRotation = Quaternion.identity;
        leftCameraObject.transform.localScale = Vector3.one;

        leftCameraObject.transform.localPosition -= offset / 2;
        LeftCamera.rect = new Rect(0, 0, 0.5f, 1f);

        rightCameraObject.transform.SetParent(mainCamera.gameObject.transform, false);
        rightCameraObject.transform.localPosition = Vector3.zero;
        rightCameraObject.transform.localRotation = Quaternion.identity;
        rightCameraObject.transform.localScale = Vector3.one;

        rightCameraObject.transform.localPosition += offset / 2;
        RightCamera.rect = new Rect(0.5f, 0, 1f, 1f);

        mainCamera.rect = new Rect(0.45f, 0.45f, 0.1f, 0.1f);
    }

    void CrossEye_Kill() {
        GameObject.DestroyImmediate(leftCameraObject);
        GameObject.DestroyImmediate(rightCameraObject);

        mainCamera.rect = new Rect(0, 0, 1f, 1f);
    }
}
