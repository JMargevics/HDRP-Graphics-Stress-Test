using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSDisplay : ProfilerVisualizer
{
    float currentFPS;
    VisualizerLayout layout;
	float drawTimer;
	GUIContent cache;

    void ProfilerVisualizer.OnGUI()
    {
		if(layout.visible)
		{
			if (drawTimer > 1)
			{
				cache = new GUIContent(String.Format("<color=#00ff00ff><b>{0:F2}</b></color>", currentFPS));
				drawTimer = 0;
			}
			GUI.Label(layout.getPosition(), cache);
		}
        
		
		drawTimer += Time.deltaTime;
    }

    private FPSDisplay(VisualizerLayout layout)
    {
        this.layout = layout;
		drawTimer = 5000;

	}

    public static VisualizerLayout Create(Profiler prof, VisualizerLayout layout = null, List<int> labelIndents = null)
    {
        if (layout == null)
        {
            layout = new VisualizerLayout();
            layout.layout.width = 350;
        }
        FPSDisplay fps = new FPSDisplay(layout);
        prof.AttachVisualizer(fps);
        return layout;
    }

    void ProfilerVisualizer.OnRecorderAttached(Profiler.RecorderData newLabel)
    {
    }

    void ProfilerVisualizer.OnRecorderChanged(Profiler.ProfilerData data)
    {
    }

    void ProfilerVisualizer.OnRecorderDetached(Profiler.RecorderData remLabel)
    {
    }

    void ProfilerVisualizer.OnUpdate(Profiler.ProfilerData data)
    {
        currentFPS = 1.0f / data.m_AvgDeltaTime;
    }
}
