using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;


[System.Serializable]
public class Profiler
{
    /// <summary>
    /// Stops the profiler from collecting data. This will essentially "pause" the visualizers on the current dataset.
    /// </summary>
    public bool m_Enable = true;

    [SerializeField]
    public bool visible = true;

    /// <summary>
    /// Constructs a new profiler object
    /// </summary>
    /// <param name="samplingSize">How many samples the profiler should keep</param>
    public Profiler(int samplingSize) : this()
    {
        if (samplingSize <= 0)
        {
            throw new System.ArgumentException("Sampling size must be a positive number");
        }
        profilerData.samplingSize = samplingSize;
    }

    public Profiler(ProfilerComponent.RecorderLabel[] labels, int samplingSize) : this(samplingSize) {
        for (int i = 0; i < labels.Length; i++)
        {
            CreateRecorder(labels[i].name, labels[i].layout);
        }
    }
    public Profiler()
    {
        visualizers = new List<ProfilerVisualizer>();
        recorders = new List<Recorder>();
        profilerData = new ProfilerData();
        isCustom = new List<bool>();
        profilerData.samplingSize = 120;
    }

    private void AddOther(int slot)
    {
        recorders[slot] = null;
        isCustom[slot] = false;
        otherTime = profilerData.recorderData[slot];
        otherTime.label = "Other";
    }

    /// <summary>
    /// Gets the current data tracked by the profiler
    /// </summary>
    /// <returns>A reference to the tracked data</returns>
    public ProfilerData GetData()
    {
        return profilerData;
    }

    /// <summary>
    /// Creates a custom recorder which displays the runtime between its Begin() and End() calls.
    /// Useful for tracking performance of enemy AI or other resource intensive tasks
    /// </summary>
    /// <param name="label">Name to appear on visualisers</param>
    /// <param name="layout">The layout on visualisers</param>
    /// <returns></returns>
    public CustomSampler CreateCustomRecorder(string label, RecorderLayout layout)
    {
        int index = GetFreeSpot();
        profilerData.recorderData[index].layout = layout;
        CustomSampler ret = AttachCustomRecorder(label, index);
        for (int j = 0; j < visualizers.Count; j++)
        {
            visualizers[j].OnRecorderAttached(profilerData.recorderData[index]);
        }
        return ret;
    }

    /// <summary>
    /// Makes the profiler track a new profiler label
    /// </summary>
    /// <param name="label">The name of the profiler label to track</param>
    /// <param name="c">The color that this particular label should show up as on the visualizers</param>
    /// <param name="visible">If the label should be drawn from the start</param>
    /// <returns>false if the label is already tracked</returns>
    public bool CreateRecorder(string label, RecorderLayout layout)
    {
        int index = GetFreeSpot();
        profilerData.recorderData[index].layout = layout;
        AttachRecorder(label, index);

        for (int j = 0; j < visualizers.Count; j++)
        {
            visualizers[j].OnRecorderAttached(profilerData.recorderData[index]);
        }
        return true;
    }

    private int GetFreeSpot()
    {
        recorders.Add(null);
        isCustom.Add(false);
        RecorderData newRec = new RecorderData();
        newRec.buffer = new CircularBuffer(profilerData.samplingSize);
        profilerData.recorderData.Add(newRec);
        return recorders.Count - 1;
    }

    /// <summary>
    /// Hides a recorder by its label, it will still continue to gather data
    /// </summary>
    /// <param name="label">The label to hide</param>
    public void HideRecorder(string label)
    {
        for (int i = 0; i < profilerData.recorderData.Count; i++)
        {
            if (profilerData.recorderData[i].label.Equals(label))
            {
                profilerData.recorderData[i].layout.visible = false;
                return;
            }
        }
        Debug.LogWarningFormat("Failed to disable label {0}(maybe it is already disabled?)", label);
    }

    /// <summary>
    /// Removes a label by name. It will no longer gather data or consume memory
    /// </summary>
    /// <param name="label">the label to remove</param>
    /// <returns>returns true if the recorder was removed</returns>
    public bool RemoveRecorder(string label)
    {
        for (int i = 0; i < profilerData.recorderData.Count; i++)
        {
            if (profilerData.recorderData[i].label.Equals(label))
            {
                RecorderData profiler = profilerData.recorderData[i];
                profilerData.recorderData.RemoveAt(i);
                recorders.RemoveAt(i);
                isCustom.RemoveAt(i);
                for (int j = 0; j < visualizers.Count; j++)
                {
                    visualizers[j].OnRecorderDetached(profiler);
                }
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Attaches a new visualizer to display this profiler's data.
    /// </summary>
    /// <param name="visualizer">The visualizer instance to add</param>
    public void AttachVisualizer(ProfilerVisualizer visualizer)
    {
        visualizers.Add(visualizer);
    }

    /// <summary>
    /// Shows a recorder after is has been hidden
    /// </summary>
    /// <param name="label">The label to show</param>
    public void ShowRecorder(string label)
    {
        for (int i = 0; i < profilerData.recorderData.Count; i++)
        {
            if (profilerData.recorderData[i].label.Equals(label))
            {
                profilerData.recorderData[i].layout.visible = true;
                return;
            }
        }
        Debug.LogWarningFormat("Failed to enable label {0}(maybe it is already enabled?)", label);
    }

    /// <summary>
    /// Makes the profiler collect data
    /// Must only be called once a frame(preferably from Update)
    /// </summary>
    public void Update()
    {
        if (m_Enable)
        {
            RecorderData data;
            if (HUDUpdateTime != null)
                HUDUpdateTime.Begin();
            float sum = 0;
            // get timing & update average accumulators, skip first element
            for (int i = 1; i < profilerData.recorderData.Count; i++)
            {
                if (recorders[i] == null)
                    continue;
                data = profilerData.recorderData[i];
                data.time = recorders[i].elapsedNanoseconds / 1000000.0f;
                data.buffer.insert(data.time);
                data.count = recorders[i].sampleBlockCount;
                data.accTime += data.time;
                data.accCount += data.count;
                sum += data.time;
            }
            float deltaTime = Time.deltaTime;
            if (otherTime != null)
            {
                float delta = Mathf.Max(0, deltaTime * 1000f - sum);
                otherTime.time = delta;
                otherTime.buffer.insert(delta);
                otherTime.count = 1;
                otherTime.accTime += delta;
                otherTime.accCount += 1;
            }


            m_AccDeltaTime += deltaTime;
            frameCount++;
            // time to time, update average values & reset accumulators
            if (frameCount >= kAverageFrameCount)
            {
                for (int i = 0; i < profilerData.recorderData.Count; i++)
                {
                    data = profilerData.recorderData[i];
                    data.avgTime = data.accTime * (1.0f / kAverageFrameCount);
                    data.avgCount = data.accCount * (1.0f / kAverageFrameCount);
                    data.accTime = 0.0f;
                    data.accCount = 0;
                }

                profilerData.m_AvgDeltaTime = m_AccDeltaTime / kAverageFrameCount;
                m_AccDeltaTime = 0.0f;
                frameCount = 0;
            }
            for (int i = 0; i < visualizers.Count; i++)
            {
                visualizers[i].OnUpdate(profilerData);
            }
            if (HUDUpdateTime != null)
                HUDUpdateTime.End();
        }
    }

    /// <summary>
    /// Makes the profiler render all visualizers(if any). MUST be called from OnGUI.
    /// </summary>
    public void OnGUI()
    {
        if (!visible || recorders.Count == 0)
            return;

        if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
        {
            if (HUDRenderTime != null)
                HUDRenderTime.Begin();
            for (int i = 0; i < visualizers.Count; i++)
            {
                visualizers[i].OnGUI();
            }
            if (HUDRenderTime != null)
                HUDRenderTime.End();
        }
    }

    /// <summary>
    /// Reloads the currently tracked recorders. Useful for restoring the Recorders after a Mono reload.
    /// </summary>
    public void ReloadRecorders()
    {
        for (int i = profilerData.recorderData.Count - 1; i >= 0; i--) //we remove all custom recorders as it does not make sense to keep them, because the reference to them are invalid
        {
            if (isCustom[i])
            {
                profilerData.recorderData.RemoveAt(i);
                isCustom.RemoveAt(i);
            }
        }
        recorders = new List<Recorder>(profilerData.recorderData.Count);
        for (int i = 0; i < profilerData.recorderData.Count; i++)
        {
            recorders.Add(null);
            AttachRecorder(profilerData.recorderData[i].label, i);
        }
    }

    public void ChangeRecorderLabel(int slot, string label)
    {
        AttachRecorder(label, slot);
        for (int i = 0; i < visualizers.Count; i++)
        {
            visualizers[i].OnRecorderChanged(profilerData);
        }
    }

    [NonSerialized]
    private List<ProfilerVisualizer> visualizers;
    [NonSerialized]
    private List<Recorder> recorders;
    private List<bool> isCustom;
    private int frameCount = 0;
    private const int kAverageFrameCount = 64;
    private float m_AccDeltaTime;
    [SerializeField]
    private ProfilerData profilerData;
    [NonSerialized]
    private RecorderData otherTime;

    /// <summary>
    /// How many frames should be kept in memory?
    /// </summary>
    private int samplingSize
    {
        set { profilerData.samplingSize = value; }
        get { return profilerData.samplingSize; }
    }
    CustomSampler HUDUpdateTime;
    CustomSampler HUDRenderTime;

    [System.Serializable]
    public class ProfilerData
    {
        public List<RecorderData> recorderData = new List<RecorderData>();
        public float m_AvgDeltaTime;
        public int samplingSize;
    }


    [System.Serializable]
    public class RecorderLayout
    {
        public Color color;
        public bool visible;
    }

    [System.Serializable]
    public class RecorderData
    {
        public string label;
        public float time;
        public int count;
        public float avgTime;
        public float avgCount;
        public float accTime;
        public int accCount;
        public CircularBuffer buffer;
        public RecorderLayout layout;
    }

    private CustomSampler AttachCustomRecorder(string label, int slot)
    {
        CustomSampler newSampler = CustomSampler.Create(label);
        recorders[slot] = newSampler.GetRecorder();
        isCustom[slot] = true;
        profilerData.recorderData[slot].label = label;
        if (!newSampler.isValid)
        {
            Debug.LogWarningFormat("ProfilerHUD: recorder \"{0}\" is either invalid or temporarily unavailable", label);
        }
        return newSampler;
    }

    private void AttachRecorder(string label, int slot)
    {
		//we are replacing the "other" label
		if(otherTime != null && profilerData.recorderData[slot].label.Equals("Other"))
		{
			otherTime = null;
		}
        if (label.Equals("Other"))
        {
            AddOther(slot);
            return;
        }
        else if (label.Equals("ProfilerHUD.Update"))
        {
            HUDUpdateTime = AttachCustomRecorder("ProfilerHUD.Update", slot);
            isCustom[slot] = false;
            return;
        }
        else if (label.Equals("ProfilerHUD.Render"))
        {
            HUDRenderTime =  AttachCustomRecorder("ProfilerHUD.Render", slot);
            isCustom[slot] = false;
            return;
        }
        Recorder sampler = Recorder.Get(label);
        isCustom[slot] = false;
        profilerData.recorderData[slot].label = label;
        recorders[slot] = sampler;
        if (!sampler.isValid)
        {
            Debug.LogWarningFormat("ProfilerHUD: recorder \"{0}\" is either invalid or temporarily unavailable", label);
        }
    }
}
