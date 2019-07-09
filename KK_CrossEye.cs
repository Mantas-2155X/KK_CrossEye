#define DEBUG
#undef DEBUG

using System.ComponentModel;
using BepInEx;
using UnityEngine;

#if DEBUG
    using BepInEx.Logging;
#endif

[BepInPlugin(nameof(KK_CrossEye), nameof(KK_CrossEye), "1.4")]
public class KK_CrossEye : BaseUnityPlugin {
    private Camera mainCamera;

    private GameObject leftCameraObject;
    private GameObject rightCameraObject;

    private Camera LeftCamera;
    private Camera RightCamera;

    private float oldFocus;
    private float currentFocus;

    private bool CrossEye_Enabled = false;

    private Vector3 offset;
    private RaycastHit hit;
    private Ray ray;

    protected string hitgroup_1 = "cf_";
    protected string hitgroup_2 = "aibu_";
    protected string hitgroup_3 = "NPC(";

    protected string[] moreBaddies = {
        "ImageEffects.GlobalFog",
        //"ImageEffects.BloomAndFlares",
        "ImageEffects.SunShafts",
        "ImageEffects.VignetteAndChromaticAberration",
        "ImageEffects.DepthOfField",
        "ImageEffects.Blur",
        "ImageEffects.SepiaTone"
    };

    #region Config properties
        [DisplayName("CrossEye mode IPD")]
        [Description("Eye separation distance.")]
        [AcceptableValueRange(0f, 1f, false)]
        public static ConfigWrapper<float> CrossEye_IPD { get; private set; }

        [DisplayName("CrossEye mode initial angle")]
        [Description("Eye initial angle.")]
        [AcceptableValueRange(0f, 45f, false)]
        public static ConfigWrapper<float> CrossEye_InitialAngle { get; private set; }

        [DisplayName("CrossEye mode focus in speed")]
        [Description("Eye focus-in speed.")]
        [AcceptableValueRange(0.01f, 0.99f, false)]
        public static ConfigWrapper<float> CrossEye_FocusInSpeed { get; private set; }

        [DisplayName("CrossEye mode focus out speed")]
        [Description("Eye focus-out speed.")]
        [AcceptableValueRange(0.01f, 0.99f, false)]
        public static ConfigWrapper<float> CrossEye_FocusOutSpeed { get; private set; }

        [DisplayName("CrossEye mode focus force disable")]
        public ConfigWrapper<bool> CrossEye_FocusForceDisabled { get; private set; }

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
        CrossEye_InitialAngle = new ConfigWrapper<float>("CrossEye mode initial angle", this, 2.5f);
        CrossEye_FocusInSpeed = new ConfigWrapper<float>("CrossEye mode focus-in speed", this, 0.07f);
        CrossEye_FocusOutSpeed = new ConfigWrapper<float>("CrossEye mode focus-out speed", this, 0.07f);
        CrossEye_FocusForceDisabled = new ConfigWrapper<bool>("CrossEye mode focus force disable", this, false);
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

                mainCamera = Camera.main;

                CrossEye_Init();

                LeftCamera.transform.Rotate(0, -CrossEye_InitialAngle.Value, 0);
                RightCamera.transform.Rotate(0, CrossEye_InitialAngle.Value, 0);
            } else {
                CrossEye_Kill();
            }
        }

        if (!CrossEye_FocusForceDisabled.Value && CrossEye_Enabled) {
            ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
#if DEBUG
            if (Physics.Raycast(ray, out hit)) {
                BepInEx.Logger.Log(LogLevel.Debug, $"Distance: {hit.distance} Target: {hit.transform.name}");
#else
            if (Physics.Raycast(ray, out hit, CrossEye_FocusDistance.Value) && (hit.distance <= CrossEye_FocusDistance.Value) && (hit.transform.name.Contains(hitgroup_1) || hit.transform.name.Contains(hitgroup_2) || hit.transform.name.Contains(hitgroup_3))) {
#endif
                oldFocus = currentFocus;
                currentFocus = Mathf.Lerp(currentFocus, CrossEye_FocusTotal.Value - (hit.distance * CrossEye_FocusMultiply.Value), CrossEye_FocusInSpeed.Value);
#if !DEBUG
                LeftCamera.transform.Rotate(0, oldFocus - currentFocus, 0);
                RightCamera.transform.Rotate(0, -oldFocus + currentFocus, 0);
#endif
            } else {
                oldFocus = currentFocus;
                currentFocus = Mathf.Lerp(currentFocus, 0, CrossEye_FocusOutSpeed.Value);
#if !DEBUG
                LeftCamera.transform.Rotate(0, oldFocus - currentFocus, 0);
                RightCamera.transform.Rotate(0, -oldFocus + currentFocus, 0);
#endif
            }
        }

    }

    void CrossEye_Init() {
        leftCameraObject = GameObject.Instantiate(mainCamera.gameObject);

        var baddies = new[] {
            typeof(Studio.CameraControl),
            typeof(BaseCameraControl_Ver2),
            typeof(GUILayer),
            typeof(CapsuleCollider),
            typeof(Rigidbody),
            typeof(FlareLayer)
        };

        foreach (var b in baddies) {
            GameObject.DestroyImmediate(leftCameraObject.gameObject.GetComponent(b));
        }

        foreach (var leftComp in leftCameraObject.GetComponents<UnityEngine.Component>()) {
            foreach (var cName in moreBaddies) {
                if (leftComp.GetType().FullName.Contains(cName)) {
                    GameObject.DestroyImmediate(leftComp);
                }
            }
        }

        rightCameraObject = GameObject.Instantiate(leftCameraObject);

        LeftCamera = leftCameraObject.GetComponent<Camera>();
        RightCamera = rightCameraObject.GetComponent<Camera>();

        LeftCamera.CopyFrom(mainCamera);
        RightCamera.CopyFrom(LeftCamera);

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
        RightCamera.rect = new Rect(0.5f, 0, 0.5f, 1f);

        mainCamera.rect = new Rect(0, 0, 0.5f, 1f);
        mainCamera.enabled = false;
    }

    void CrossEye_Kill() {
        mainCamera.enabled = true;
        mainCamera.rect = new Rect(0, 0, 1f, 1f);

        GameObject.DestroyImmediate(leftCameraObject);
        GameObject.DestroyImmediate(rightCameraObject);
    }
}
