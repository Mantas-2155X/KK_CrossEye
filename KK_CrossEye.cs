using BepInEx;
using UnityEngine;
using System.ComponentModel;

[BepInPlugin(nameof(KK_CrossEye), nameof(KK_CrossEye), "1.1")]
public class KK_CrossEye : BaseUnityPlugin {
    private Camera mainCamera;
    private GameObject leftCameraObject;
    private GameObject rightCameraObject;

    private Vector3 offset = Vector3.left * 0.15f;

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

        [DisplayName("CrossEye mode focus initial angle")]
        [Description("Eye focus initial angle.")]
        [AcceptableValueRange(0f, 45f, false)]
        public static ConfigWrapper<float> CrossEye_FocusInitialAngle { get; private set; }

        [DisplayName("EXPERIMENTAL CrossEye mode focus start distance")]
        [Description("EXPERIMENTAL Eye focus start distance.")]
        [AcceptableValueRange(0.25f, 5f, false)]
        public static ConfigWrapper<float> CrossEye_FocusDistance { get; private set; }

        [DisplayName("EXPERIMENTAL CrossEye mode focus multiply")]
        [Description("EXPERIMENTAL Eye focus multiply.")]
        [AcceptableValueRange(2.5f, 50f, false)]
        public static ConfigWrapper<float> CrossEye_FocusMultiply { get; private set; }

        [DisplayName("EXPERIMENTAL CrossEye mode focus total")]
        [Description("EXPERIMENTAL Eye focus total.")]
        [AcceptableValueRange(2.5f, 50f, false)]
        public static ConfigWrapper<float> CrossEye_FocusTotal { get; private set; }

        public static SavedKeyboardShortcut CrossEye_EnableKey { get; private set; }
    #endregion

    void Awake() {
        CrossEye_EnableKey = new SavedKeyboardShortcut("CrossEye mode key", this, new KeyboardShortcut(KeyCode.Keypad1));
        CrossEye_IPD = new ConfigWrapper<float>("CrossEye mode IPD", this, 0.18f);
        CrossEye_FocusInitialAngle = new ConfigWrapper<float>("CrossEye mode focus initial angle", this, 2.5f);
        CrossEye_FocusDistance = new ConfigWrapper<float>("EXPERIMENTAL CrossEye mode focus start distance", this, 1f);
        CrossEye_FocusMultiply = new ConfigWrapper<float>("EXPERIMENTAL CrossEye mode focus multiply", this, 10f);
        CrossEye_FocusTotal = new ConfigWrapper<float>("EXPERIMENTAL CrossEye mode focus total", this, 10f);
    }

    void Update() {
        if (CrossEye_EnableKey.IsDown()) {
            CrossEye_Enabled = !CrossEye_Enabled;

            offset = Vector3.left * CrossEye_IPD.Value;

            if (CrossEye_Enabled) {

                oldFocus = 0f;
                currentFocus = 0f;

                CrossEye_Init();

                LeftCamera.transform.Rotate(0, -CrossEye_FocusInitialAngle.Value, 0);
                RightCamera.transform.Rotate(0, CrossEye_FocusInitialAngle.Value, 0);
            } else { 
                CrossEye_Kill(); 
            }
        }

        if (CrossEye_Enabled) {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && (hit.distance <= CrossEye_FocusDistance.Value) && (hit.transform.name.Contains("cf_") || hit.transform.name.Contains("aibu_"))) {
                oldFocus = currentFocus;
                currentFocus = (CrossEye_FocusTotal.Value - (hit.distance * CrossEye_FocusMultiply.Value));

                LeftCamera.transform.Rotate(0, oldFocus - currentFocus, 0);
                RightCamera.transform.Rotate(0, -oldFocus + currentFocus, 0);
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

        mainCamera.rect = new Rect(0.499f, 0.499f, 0.002f, 0.002f);
    }

    void CrossEye_Kill() {
        GameObject.DestroyImmediate(leftCameraObject);
        GameObject.DestroyImmediate(rightCameraObject);

        mainCamera.rect = new Rect(0, 0, 1f, 1f);
    }
}
