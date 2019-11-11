using System.Collections;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KK_CrossEye
{
    
    public static class CrossEye_Data
    {
        public static bool CrossEye_ShouldStart;
    }

    [BepInPlugin(nameof(KK_CrossEye), nameof(KK_CrossEye), "1.6")]
    public class KK_CrossEye : BaseUnityPlugin
    {
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

        private readonly string hitgroup_1 = "cf_";
        private readonly string hitgroup_2 = "aibu_";
        private readonly string hitgroup_3 = "NPC(";

        private readonly string[] IgnoreScenes = {
            "NightMenu",
            "Check",
            "Config",
            "Title",
            "Init",
            "Logo",
            "Load",
            "Action",
            "Exit",
            "ClassRoomSelect",
            "Save",
            "LiveStage",
            "FixEventSceneEx",
            "ADV",
            "CustomScene",
            "NetworkCheckScene",
            "Uploader",
            "Downloader",
            "EntryPlayer",
            "FreeH",
            "FreeHCharaSelectFemale",
            "FreeHCharaSelectMale",
            "LiveCharaSelectFemale"
        };

        #region Config properties
        private static ConfigEntry<float> CrossEye_IPD { get; set; }
        private static ConfigEntry<float> CrossEye_InitialAngle { get; set; }
        
        private static ConfigEntry<float> CrossEye_FocusInSpeed { get; set; }
        private static ConfigEntry<float> CrossEye_FocusOutSpeed { get; set; }
        private static ConfigEntry<bool> CrossEye_FocusForceDisabled { get; set; }
        private static ConfigEntry<float> CrossEye_FocusDistance { get; set; }
        private static ConfigEntry<float> CrossEye_FocusMultiply { get; set; }
        private static ConfigEntry<float> CrossEye_FocusTotal { get; set; }
        
        private static ConfigEntry<KeyboardShortcut> CrossEye_EnableKey { get; set; }
        #endregion

        private void Awake()
        {
            CrossEye_EnableKey = Config.AddSetting(new ConfigDefinition("Main", "Enable Key"), new KeyboardShortcut(KeyCode.Keypad1));
            CrossEye_IPD = Config.AddSetting(new ConfigDefinition("Main", "IPD"), 0.18f, new ConfigDescription("Camera IPD in CrossEye mode"));
            CrossEye_InitialAngle = Config.AddSetting(new ConfigDefinition("Main", "Initial Angle"), 2.5f, new ConfigDescription("Camera initial angle in CrossEye mode"));
            
            CrossEye_FocusInSpeed = Config.AddSetting(new ConfigDefinition("Focus", "Focus-in speed"), 0.05f, new ConfigDescription("Camera focus-in speed in CrossEye mode"));
            CrossEye_FocusOutSpeed = Config.AddSetting(new ConfigDefinition("Focus", "Focus-out speed"), 0.05f, new ConfigDescription("Camera focus-out speed in CrossEye mode"));
            CrossEye_FocusForceDisabled = Config.AddSetting(new ConfigDefinition("Focus", "Disable focus"), false, new ConfigDescription("Camera focus toggle in CrossEye mode"));
            
            CrossEye_FocusDistance = Config.AddSetting(new ConfigDefinition("EXPERIMENTAL FOCUS", "Focus start distance"), 1f, new ConfigDescription("Camera focus start distance in CrossEye mode"));
            CrossEye_FocusMultiply = Config.AddSetting(new ConfigDefinition("EXPERIMENTAL FOCUS", "Focus multiply"), 10f, new ConfigDescription("Camera focus multiply in CrossEye mode"));
            CrossEye_FocusTotal = Config.AddSetting(new ConfigDefinition("EXPERIMENTAL FOCUS", "Focus total"), 10f, new ConfigDescription("Camera focus total in CrossEye mode"));
        }

        private void Update()
        {
            if (CrossEye_EnableKey.Value.IsDown())
            {
                CrossEye_Enabled = !CrossEye_Enabled;

                offset = Vector3.left * CrossEye_IPD.Value;

                if (CrossEye_Enabled)
                {
                    oldFocus = 0f;
                    currentFocus = 0f;

                    mainCamera = Camera.main;

                    CrossEye_Init();

                    LeftCamera.transform.Rotate(0, -CrossEye_InitialAngle.Value, 0);
                    RightCamera.transform.Rotate(0, CrossEye_InitialAngle.Value, 0);
                }
                else
                {
                    CrossEye_Kill();
                }
            }

            if (CrossEye_FocusForceDisabled.Value || !CrossEye_Enabled) 
                return;
            
            ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out hit, CrossEye_FocusDistance.Value) && (hit.distance <= CrossEye_FocusDistance.Value) && (hit.transform.name.Contains(hitgroup_1) || hit.transform.name.Contains(hitgroup_2) || hit.transform.name.Contains(hitgroup_3)))
            {
                oldFocus = currentFocus;
                currentFocus = Mathf.Lerp(currentFocus, CrossEye_FocusTotal.Value - (hit.distance * CrossEye_FocusMultiply.Value), CrossEye_FocusInSpeed.Value);
                LeftCamera.transform.Rotate(0, oldFocus - currentFocus, 0);
                RightCamera.transform.Rotate(0, -oldFocus + currentFocus, 0);
            }
            else
            {
                oldFocus = currentFocus;
                currentFocus = Mathf.Lerp(currentFocus, 0, CrossEye_FocusOutSpeed.Value);
                LeftCamera.transform.Rotate(0, oldFocus - currentFocus, 0);
                RightCamera.transform.Rotate(0, -oldFocus + currentFocus, 0);
            }

        }

        private void CrossEye_Init()
        {
            leftCameraObject = Instantiate(mainCamera.gameObject);

            var BadComponents = new[] {
                typeof(Studio.CameraControl),
                typeof(BaseCameraControl_Ver2),
                typeof(GUILayer),
                typeof(CapsuleCollider),
                typeof(Rigidbody),
                typeof(FlareLayer)
            };

            foreach (var b in BadComponents)
                DestroyImmediate(leftCameraObject.gameObject.GetComponent(b));

            rightCameraObject = Instantiate(leftCameraObject);

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

        private void CrossEye_Kill()
        {
            CrossEye_Enabled = false;

            mainCamera.enabled = true;
            mainCamera.rect = new Rect(0, 0, 1f, 1f);

            if (rightCameraObject != null)
                DestroyImmediate(rightCameraObject);

            if (leftCameraObject != null)
                DestroyImmediate(leftCameraObject);
        }

        private void OnDestroy()
        {
            if (CrossEye_Enabled)
                CrossEye_Kill();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
            SceneManager.sceneUnloaded += OnSceneUnload;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            SceneManager.sceneUnloaded -= OnSceneUnload;
        }

        private IEnumerator CrossEye_DelayedStart(float time)
        {
            yield return new WaitForSeconds(time);

            if (CrossEye_Enabled) 
                yield break;
            
            CrossEye_Enabled = true;

            oldFocus = 0f;
            currentFocus = 0f;

            mainCamera = Camera.main;

            CrossEye_Init();

            LeftCamera.transform.Rotate(0, -CrossEye_InitialAngle.Value, 0);
            RightCamera.transform.Rotate(0, CrossEye_InitialAngle.Value, 0);
        }

        private void OnSceneUnload(Scene scene)
        {
            if (CrossEye_Enabled)
            {
                CrossEye_Kill();
                CrossEye_Data.CrossEye_ShouldStart = true;
            }
            else
            {
                if (!CrossEye_Data.CrossEye_ShouldStart || IgnoreScenes.Contains(scene.name)) 
                    return;
                
                CrossEye_Data.CrossEye_ShouldStart = false;
                StartCoroutine(CrossEye_DelayedStart(0.25f));
            }
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (CrossEye_Enabled)
            {
                CrossEye_Kill();
                CrossEye_Data.CrossEye_ShouldStart = true;
            }
            else
            {
                if (!CrossEye_Data.CrossEye_ShouldStart || IgnoreScenes.Contains(scene.name)) 
                    return;
                
                CrossEye_Data.CrossEye_ShouldStart = false;
                StartCoroutine(CrossEye_DelayedStart(0.25f));
            }
        }
    }
}