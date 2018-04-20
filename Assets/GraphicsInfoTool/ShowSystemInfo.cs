using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

[ExecuteInEditMode]

public class ShowSystemInfo : MonoBehaviour
{
    [Header("Text Objects")]
    public Text tm;
    public Text tm_hdr;
    public Text tm_msaa;
    public Text tm_vsync;
    public Text tm_renderpath;
    public Text tm_tier;
    public GameObject[] showHideObjects;
    public GameObject dontDestroyGO;

    [Header("FPS Counter")]
    public Text tm_fps;
    private double alltime = 0;
    private double dt = 0;
    private double avgt = 0;
    private int count = 1;
    public int skipcount = 1000;
    public int samplecount = 1000;
    private string result="";

    [SerializeField]
    [HideInInspector]
    private string colorspace = "not assigned";
    [SerializeField]
    [HideInInspector]
    private string grahicsjob = "not assigned";

    void Start()
    {
        Application.targetFrameRate = 999;
        Screen.SetResolution(1920, 1080, true);

        if (Application.isPlaying)
        {
            DontDestroyOnLoad(dontDestroyGO);
        }

        if (Application.isPlaying && SceneManager.sceneCountInBuildSettings > 1)
        {
            SceneManager.LoadScene(1);
            count = 1;
        }

        updateText();
    }

    public void updateText()
    {
#if UNITY_EDITOR
        colorspace = PlayerSettings.colorSpace.ToString();
        grahicsjob = PlayerSettings.graphicsJobMode.ToString();
#endif

        tm.text = "";

        
        #if UNITY_EDITOR
        tm.text = tm.text + TitleText("Unity : ") + InternalEditorUtility.GetFullUnityVersion() + "\n";
        tm.text = tm.text + TitleText("Branch : ") + InternalEditorUtility.GetUnityBuildBranch() + "\n";
        #else
        tm.text = tm.text + TitleText("Unity : ") + Application.unityVersion + "\n";
        #endif
        tm.text = tm.text + TitleText("Device : ") + SystemInfo.deviceModel + "\n";
        tm.text = tm.text + TitleText("OS : ") + SystemInfo.operatingSystem + "\n";
        tm.text = tm.text + TitleText("CPU : ") + SystemInfo.processorType + "\n";
        tm.text = tm.text + TitleText("GPU : ") + SystemInfo.graphicsDeviceName + "\n";
        tm.text = tm.text + TitleText("GPU Version : ") + SystemInfo.graphicsDeviceVersion + "\n";
        tm.text = tm.text + TitleText("API : ") + SystemInfo.graphicsDeviceType + "\n";
        tm.text = tm.text + TitleText("Platform : ") + Application.platform.ToString() + "\n";
        tm.text = tm.text + "<i>" + TitleText("Color Space : ") + colorspace + "</i>" + "\n";
        tm.text = tm.text + "<i>" + TitleText("GraphicsJob Mode : ") + grahicsjob + "</i>" + "\n";
        tm.text = tm.text + TitleText("Multi-thread : ") + BooleanText(SystemInfo.graphicsMultiThreaded) + "\n";
        tm.text = tm.text + TitleText("VSync : ") + QualitySettings.vSyncCount.ToString() + "\n";
        tm.text = tm.text + TitleText("S.M. Support : ") + SystemInfo.graphicsShaderLevel.ToString() + "\n";
        tm.text = tm.text + TitleText("Tier : ") + Graphics.activeTier.ToString() + "\n";

        if (Camera.main != null)
        {
            tm.text = tm.text + TitleText("Camera Rendering Path : ") + Camera.main.renderingPath.ToString() + "\n";
            #if UNITY_5_6_OR_NEWER
                if (Camera.main.allowHDR) tm_hdr.text = "HDR On"; else tm_hdr.text = "HDR Off";
                if (Camera.main.allowMSAA) tm_msaa.text = "MSAA On"; else tm_msaa.text = "MSAA Off";
            #endif
            tm_renderpath.text = Camera.main.renderingPath.ToString();
        }
        else
        {
            tm.text = tm.text + WarningText("Camera is null") + "\n";
            tm_hdr.text = "HDR --";
            tm_msaa.text = "MSAA --";
            tm_renderpath.text = "Change Render Path";
        }

        tm.text = tm.text + H2Text("Compute Shader : ") + BooleanText(SystemInfo.supportsComputeShaders) + "\n";
        tm.text = tm.text + H2Text("GPU Instancing : ") + BooleanText(SystemInfo.supportsInstancing) + "\n";
        tm.text = tm.text + H2Text("2D Array Texture : ") + BooleanText(SystemInfo.supports2DArrayTextures) + "\n";
        tm.text = tm.text + H2Text("3D Texture : ") + BooleanText(SystemInfo.supports3DTextures) + "\n";
        #if UNITY_5_6_OR_NEWER
                tm.text = tm.text + H2Text("3D Render Texture : ") + BooleanText(SystemInfo.supports3DRenderTextures) + "\n";
        #endif
        tm.text = tm.text + H2Text("Cubemap Array Texture : ") + BooleanText(SystemInfo.supportsCubemapArrayTextures) + "\n";
        //tm.text = tm.text + H2Text("Image Effect : ") + BooleanText(SystemInfo.) + "\n";
        // tm.text = tm.text + H2Text("Motion Vector : ") + BooleanText(SystemInfo.supportsMotionVectors) + "\n";
        // tm.text = tm.text + H2Text("Raw Shadow Depth Sampling : ") + BooleanText(SystemInfo.supportsRawShadowDepthSampling) + "\n";
        tm.text = tm.text + H2Text("Sparse Textures : ") + BooleanText(SystemInfo.supportsSparseTextures) + "\n";
        tm.text = tm.text + H2Text("Max Cubemap Size : ") + SystemInfo.maxCubemapSize + "\n";
        tm.text = tm.text + H2Text("Max Texture Size : ") + SystemInfo.maxTextureSize + "\n";
        tm.text = tm.text + H2Text("Render Target Count : ") + SystemInfo.supportedRenderTargetCount + "\n";

        //tm.text = tm.text + "MSG"  + "\n";
        if (SceneManager.sceneCount > 1)
        {
            float count = SceneManager.sceneCountInBuildSettings - 1;
            tm.text = tm.text + TitleText(SceneManager.GetSceneAt(1).buildIndex + "/" + count) + " " +
            SceneManager.GetSceneAt(1).name.ToString() + "\n";
        }
        else
        {
            tm.text = tm.text + TitleText(SceneManager.GetActiveScene().buildIndex + "/" +
            SceneManager.sceneCountInBuildSettings) + " " +
            SceneManager.GetActiveScene().name.ToString() + "\n";
        }

        tm.text = tm.text + "Click Here to Show/Hide";

        //
        tm_vsync.text = "VSync " + QualitySettings.vSyncCount;
        tm_tier.text = Graphics.activeTier.ToString();
    }


    void Update()
    {
        #if UNITY_EDITOR
            if (!Application.isPlaying) updateText();
        #endif

        if (Application.isPlaying && Input.touchCount > 0 && Input.GetTouch(0).tapCount == 2 && Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetKeyDown(KeyCode.Space))
        {
            NextScene();
            updateText();
        }

        AverageFPSCounter();
    }

    private void AverageFPSCounter()
    {
            tm_fps.text = "";

        if(count > skipcount)
        {
            
            if(count == skipcount + samplecount) //Show result
            {
                avgt = alltime/((count-skipcount)*1.000000000f);

                result = GreenText("average FPS in " + samplecount + " frames = "+System.String.Format("{0:F2} FPS",avgt));

                tm_fps.text = result;
                tm_fps.text += "\n";

            }
            else if(count < skipcount + samplecount) //Do sampling
            {
                alltime += Time.timeScale/Time.deltaTime;
                tm_fps.text = WarningText("now sampling " + samplecount + " frames..."+count);
                tm_fps.text += "\n";
            }
            else
            {
                tm_fps.text = result;
                tm_fps.text += "\n";
            }
        }
        else
        {
            alltime = 0;
            tm_fps.text = ErrorText("skipping first "+ skipcount +" frames..."+count);
            tm_fps.text += "\n";
        }

        count++;

                //Memory
                long num = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024 / 1024;
                tm_fps.text += TitleText("AllocatedMemoryForGraphicsDriver = ")+num.ToString() + " mb";
                tm_fps.text += "\n";
                num = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024;
                tm_fps.text += TitleText("TotalAllocatedMemory = ")+num.ToString() + " mb";
                tm_fps.text += "\n";
                num = UnityEngine.Profiling.Profiler.GetTempAllocatorSize() / 1024 / 1024;
                tm_fps.text += TitleText("TempAllocatorSize = ")+num.ToString() + " mb";
                tm_fps.text += "\n";
                num = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1024 / 1024;
                tm_fps.text += TitleText("TotalReservedMemoryg = ")+num.ToString() + " mb";
                tm_fps.text += "\n";
                num = UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong() / 1024 / 1024;
                tm_fps.text += TitleText("TotalUnusedReservedMemory = ")+num.ToString() + " mb";
                tm_fps.text += "\n";

    }

    //========Text Styles========
    private string TitleText(string text)
    {
        return "<color=#00ffff>" + text + "</color>";
    }

    private string WarningText(string text)
    {
        return "<color=#ffff00>" + text + "</color>";
    }

    private string ErrorText(string text)
    {
        return "<color=#ff0099>" + text + "</color>";
    }

    private string GreenText(string text)
    {
        return "<color=#00ff00>" + text + "</color>";
    }

    private string H2Text(string text)
    {
        return "<color=#aaaaaa>" + text + "</color>";
    }

    private string BooleanText(bool b)
    {
        if (b)
        {
            return "<color=#00ff00>" + b.ToString() + "</color>";
        }
        else
        {
            return "<color=#ff0000>" + b.ToString() + "</color>";
        }
    }
    //============Change Setting Functions===========
    public void NextScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex < SceneManager.sceneCountInBuildSettings - 1)
            SceneManager.LoadScene(sceneIndex + 1);
        else
            SceneManager.LoadScene(1);

        count = 1;
    }

    public void PrevScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex > 1)
            SceneManager.LoadScene(sceneIndex - 1);
        else
            SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);

        count = 1;
    }

    public void ToggleShowInfo()
    {
        if (showHideObjects[0].activeSelf)
        {
            for (int i = 0; i < showHideObjects.Length; i++)
                showHideObjects[i].SetActive(false);

            InfoTexts infotextlist = (InfoTexts)FindObjectOfType(typeof(InfoTexts));
            if (infotextlist != null)
            {
                Debug.Log(infotextlist.gameObject.name + " " + infotextlist.golist.Length);
                infotextlist.ShowInfoText();
            }
        }
        else
        {
            for (int i = 0; i < showHideObjects.Length; i++)
                showHideObjects[i].SetActive(true);

            InfoTexts infotextlist = (InfoTexts)FindObjectOfType(typeof(InfoTexts));
            if (infotextlist != null)
            {
                Debug.Log(infotextlist.gameObject.name + " " + infotextlist.golist.Length);
                infotextlist.HideInfoText();
            }
        }
    }

    public void changeVsync()
    {
        int i = QualitySettings.vSyncCount;
        if (i < 2) i++;
        else i = 0;

        QualitySettings.vSyncCount = i;

    }

    public void ChangeRenderPath()
    {
        switch (Camera.main.renderingPath)
        {
            case RenderingPath.Forward:
                Camera.main.renderingPath = RenderingPath.DeferredShading;
                break;
            case RenderingPath.DeferredShading:
                Camera.main.renderingPath = RenderingPath.Forward;
                break;
            default:
                Camera.main.renderingPath = RenderingPath.Forward;
                break;
        }
    }

    public void HDR_Toggle()
    {
#if UNITY_5_6_OR_NEWER
        if (Camera.main.allowHDR) Camera.main.allowHDR = false;
        else Camera.main.allowHDR = true;
#endif
    }
    public void MSAA_Toggle()
    {
#if UNITY_5_6_OR_NEWER
        if (Camera.main.allowMSAA) Camera.main.allowMSAA = false;
        else Camera.main.allowMSAA = true;
#endif
    }
    public void ChangeTier()
    {
        switch (Graphics.activeTier)
        {
            case UnityEngine.Rendering.GraphicsTier.Tier1:
                Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier2;
                break;
            case UnityEngine.Rendering.GraphicsTier.Tier2:
                Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier3;
                break;
            default:
                Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier1;
                break;
        }

    }
}

