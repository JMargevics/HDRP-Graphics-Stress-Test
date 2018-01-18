using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
class HUD : ProfilerVisualizer
{
    VisualizerLayout layout;
    private StringBuilder dataBuilder = new StringBuilder(header); //We store the string builder here to keep its underlying buffer when reset
    private GUIContent profilerLabel = new GUIContent("Profiler HUD");
    private GUIContent dataLayout = new GUIContent();
    private GUIContent labelLayout = new GUIContent();
    private GUIContent fpsLayout = new GUIContent();
    private GUIContent memoryLabel = new GUIContent();
    private static string header = String.Format("\n\n<b>{0,8}\t{1,5}\t{2,8}</b>\n", "Avg(ms)", "Count", "Frame(ms)");
    private float passedTime = 0;
    private int labelsRendered;
    private Vector2 scrollPos = new Vector2();
    List<int> labelIndents;
    private float updateInterval = 1; //In seconds
    bool isDirty = true;
    private HUD(VisualizerLayout layout)
    {
        this.layout = layout;
    }

    public static VisualizerLayout Create(Profiler prof, VisualizerLayout layout = null, List<int> labelIndents = null)
    {
        if (layout == null)
        {
            layout = new VisualizerLayout();
            layout.layout.width = 350;
        }
        HUD hud = new HUD(layout);
        hud.labelIndents = labelIndents;
        prof.AttachVisualizer(hud);
        return layout;
    }

    private void GenerateLabelLayout(Profiler.ProfilerData data)
    {
        const string labelFormat = "<b><color=#{0:X2}{1:X2}{2:X2}FF>{3}</color></b>\n";
        StringBuilder builder = new StringBuilder(labelFormat.Length * data.recorderData.Count);
        builder.AppendLine("\n"); // To make it match with column headers of the data
        if (labelIndents != null)
        {
            for (int i = 0; i < data.recorderData.Count; i++)
            {
                if (data.recorderData[i].layout.visible)
                {
                    Color32 col = data.recorderData[i].layout.color;
                    builder.AppendFormat(labelFormat, col.r, col.g, col.b, '·' + new String(' ', labelIndents[i] * 3) + data.recorderData[i].label);
                }
            }
        }
        else
        {
            for (int i = 0; i < data.recorderData.Count; i++)
            {
                if (data.recorderData[i].layout.visible)
                {
                    Color32 col = data.recorderData[i].layout.color;
                    builder.AppendFormat(labelFormat, col.r, col.g, col.b, data.recorderData[i].label);
                }
            }
        }
        labelLayout.text = builder.ToString();
    }

    void ProfilerVisualizer.OnRecorderChanged(Profiler.ProfilerData data)
    {
        GenerateLabelLayout(data);
    }

    void ProfilerVisualizer.OnRecorderAttached(Profiler.RecorderData newLabel)
    {
        isDirty = true;
        if (labelIndents != null)
            labelIndents.Add(0);
    }

    void ProfilerVisualizer.OnRecorderDetached(Profiler.RecorderData label)
    {
        isDirty = true;
        if (labelIndents != null)
            labelIndents.Remove(labelIndents.Count - 1);
    }

    void ProfilerVisualizer.OnUpdate(Profiler.ProfilerData data)
    {
        if (isDirty)
        {
            GenerateLabelLayout(data);
            isDirty = false;
        }
        if ((passedTime + updateInterval < Time.realtimeSinceStartup))
        {
            //Reset the builder, but keep the header format string
            dataBuilder.Length = header.Length;
            int lastFrameRendered = labelsRendered;
            labelsRendered = 0;
            const string dataFormat = "{0,6:F2}\t{1,4:F2}\t{2,6:F2}\n";
            for (int i = 0; i < data.recorderData.Count; i++)
            {
                if (data.recorderData[i].layout.visible)
                {
                    Profiler.RecorderData sample = data.recorderData[i];
                    dataBuilder.AppendFormat(dataFormat, sample.avgTime, sample.avgCount, sample.time);
                    ++labelsRendered;
                }
            }
            if (labelsRendered != lastFrameRendered) //We must redraw the "static" labels if one of them has been hidden since last frame
                isDirty = true;
            dataLayout.text = dataBuilder.ToString();
            fpsLayout.text = String.Format("<b>{0:F2} FPS ({1:F2}ms)</b>", 1.0f / data.m_AvgDeltaTime, data.m_AvgDeltaTime * 1000.0f);
            passedTime = Time.realtimeSinceStartup;

            long monoTotal = UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong();
            long monoUsed = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
            long nativeTotal = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
            long nativeUsed = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            const float toMB = 1f / 1000000f;
            memoryLabel.text = "Memory(Mono): " + Math.Round(monoUsed * toMB) + "MB / " + Math.Round(monoTotal * toMB) + "MB\nMemory(Native): " + Math.Round(nativeUsed * toMB) + "MB / " + Math.Round(nativeTotal * toMB) + "MB";
        }
    }

    void ProfilerVisualizer.OnGUI()
    {
        if (!layout.visible)
            return;
        GUI.color = new Color(1, 1, 1, .75f);
        Vector2 size = GUI.skin.label.CalcSize(profilerLabel) + GUI.skin.label.CalcSize(memoryLabel) + GUI.skin.label.CalcSize(dataLayout);
        size.y += 10; //Extra padding to avoid the scrollbars from showing up too often
        Rect rect = layout.layout;
        rect.height = size.y > rect.height ? rect.height : size.y;
        Rect pos = layout.getPosition(rect);
        GUILayout.BeginArea(pos, profilerLabel, GUI.skin.window);
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label(fpsLayout);

        GUILayout.Label(dataLayout);
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Label(memoryLabel);
        GUILayout.Label(labelLayout);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
