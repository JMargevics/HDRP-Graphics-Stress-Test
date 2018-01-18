using System.Runtime.InteropServices;
using UnityEngine;

public class PrintMemoryInfo : MonoBehaviour
{
    [DllImport("GetMemInfo")]
    private static extern void GetD3D12MemoryInfo(ref ulong usage, ref ulong budget);

    public GameObject ResultsUI;

    public void TriggerResults()
    {
        ResultsUI.SetActive(true);
    }

    void Start()
    {
        ulong usage = 0;
        ulong budget = 0;
        GetD3D12MemoryInfo(ref usage, ref budget);

        Debug.Log(usage);
    }


    void Update()
    {
        if (Input.GetKeyDown("f"))
        {
            Debug.Log("going");
            Debug.Log(GetCurrentMemory(true));
            Debug.Log(GetCurrentMemory(false));
        }
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(50, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        ulong usage = 0;
        ulong budget = 0;
        GetD3D12MemoryInfo(ref usage, ref budget);
        string text = string.Format("{0}/{1} Mb", usage / (1024 * 1024), budget / (1024 * 1024));
        GUI.Label(rect, text, style);
       
    }

    public float GetCurrentMemory(bool Vmem)
    {
        ulong usage = 0;
        ulong budget = 0;
        GetD3D12MemoryInfo(ref usage, ref budget);
        float val = usage;
        if (Vmem)
        {
            return val;
        }
        else
        {
            val = budget;
            return val;
        }
    }
}
