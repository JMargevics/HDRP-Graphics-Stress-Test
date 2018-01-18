using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProfilerComponent : MonoBehaviour
{
    [SerializeField]
    private RecorderLabel[] recorders;
    [NonSerialized]
    List<string> recorderTracker;
    [SerializeField]
    private int samplingSize;

	[NonSerialized]
	float applyChangesTimer;

    //If this is false, then we know that a monoreload happened
    [NonSerialized]
    private bool initialized;

    [SerializeField]
    private GraphTypeProxy[] graphs;
    [System.Serializable]
    public class RecorderLabel
    {
        public string name;
        public Profiler.RecorderLayout layout;
    }
    private enum GraphTypes
    {
        Histogram,
        HUD,
        FPS
    }

    [System.Serializable]
    private class GraphTypeProxy
    {
        public GraphTypes type;
        public VisualizerLayout layout;
    }

    private Profiler profiler;
    private void AttachGraphs()
    {
        for (int i = 0; i < graphs.Length; i++)
        {
            switch (graphs[i].type)
            {
                case GraphTypes.Histogram:
                    CumulativeHistogram.Create(profiler, graphs[i].layout);
                    break;
                case GraphTypes.HUD:
                    HUD.Create(profiler, graphs[i].layout);
                    break;
                case GraphTypes.FPS:
                    FPSDisplay.Create(profiler, graphs[i].layout);
                    break;
                default:
                    break;
            }
        }
    }

    void OnEnable()
    {
        if (profiler != null && !initialized)   //We need to reload the recorders and reattach the graphs, as those are not serializable(recorded data is still kept)
        {
            profiler.ReloadRecorders();
            AttachGraphs();
            initialized = true;
            recorderTracker = new List<string>(recorders.Length);
            for (int i = 0; i < recorders.Length; i++)
            {
                recorderTracker.Add(recorders[i].name);
            }
            return;
        }
        initialized = true;
        recorderTracker = new List<string>(recorders.Length);
        for (int i = 0; i < recorders.Length; i++)
        {
            recorderTracker.Add(recorders[i].name);
        }
        profiler = new Profiler(recorders, samplingSize);
        AttachGraphs();
    }

    private bool validateAttributes()
    {
        bool error = false;
        if (samplingSize <= 0)
        {
            Debug.LogError("Sampling Size must be higher than 0");
            error = true;
        }
        for (int i = 0; i < graphs.Length; i++)
        {
            if (graphs[i].layout.layout.size.x == 0 || graphs[i].layout.layout.size.y == 0)
            {
                Debug.LogError("Graph window " + recorders[i].name + " is invisible. Is your width or height set to 0?");
                error = true;
            }
            if (samplingSize % graphs[i].layout.layout.width != 0)
            {
                Debug.LogWarning("sample size is not a multiple of graph width. Stuttering may accour.");
            }
        }
        return !error;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            profiler.visible = !profiler.visible;
        }
		if (applyChangesTimer > 0)
		{
			applyChangesTimer -= Time.deltaTime;
			if (applyChangesTimer < 0)
			{
				if (!validateAttributes())
				{
					gameObject.SetActive(false);
					return;
				}
				UpdateTrackedLabels();
			}
		}

		profiler.Update();
    }

    private void UpdateTrackedLabels()
    {
        if (recorders.Length == recorderTracker.Count)
        {
            for (int i = 0; i < recorders.Length; i++)
            {
                if (!recorders[i].name.Equals(recorderTracker[i]))
                {
                    profiler.ChangeRecorderLabel(i, recorders[i].name);
					recorderTracker[i] = recorders[i].name;
                }
            }
        }
        else
        {
            int remaining = recorders.Length - recorderTracker.Count;
            for (int i = 0; i < Mathf.Min(recorders.Length, recorderTracker.Count); i++)
            {
                if (!recorders[i].Equals(recorderTracker[i]))
                {
                    profiler.ChangeRecorderLabel(i, recorders[i].name);
					recorderTracker[i] = recorders[i].name;
				}
            }
            if (remaining > 0)
            {
                for (int i = recorders.Length - remaining; i < recorders.Length; i++)
                {
                    profiler.CreateRecorder(recorders[i].name, recorders[i].layout);
                    recorderTracker.Add(recorders[i].name);
                }
            }
            else
            {
                int count = recorderTracker.Count;
                for (int i = recorderTracker.Count - 1; i > recorders.Length - 1; i--)
                {
                    profiler.RemoveRecorder(recorderTracker[i]);
                    recorderTracker.RemoveAt(i);
                }
            }
        }
        for (int i = 0; i < profiler.GetData().recorderData.Count; i++)
        {
            profiler.GetData().recorderData[i].layout = recorders[i].layout;
        }
    }

    private void OnValidate()
    {
		applyChangesTimer = 1;
    }

    private void OnGUI()
    {
        profiler.OnGUI();
    }
}
