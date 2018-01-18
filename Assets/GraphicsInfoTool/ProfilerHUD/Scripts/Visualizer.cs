using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VisualizerLayout
{
    public Rect layout = new Rect(0, 0, 200, 200);
    public AnchorPosition align;
    public bool visible = true;
    [Range(0.0f, 1f)]
    public float transparancy = 1f;
    public enum AnchorPosition
    {
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_LEFT,
        BOTTOM_RIGHT
    }
    public Rect getPosition(Rect pos)
    {
        pos.position += getOrigo(align, pos);
        pos.height = pos.height;
        pos.width = pos.width;
        return pos;
    }

    public Rect getPosition()
    {
        Rect rect = layout;
        return getPosition(rect);
    }

    private Vector2 getOrigo(AnchorPosition anchor, Rect viewport)
    {
        Vector2 origo = new Vector2();
        switch (anchor)
        {
            case AnchorPosition.BOTTOM_LEFT:
                origo.y = Screen.height - viewport.height;
                break;
            case AnchorPosition.BOTTOM_RIGHT:
                origo.x = Screen.width - viewport.width;
                origo.y = Screen.height - viewport.height;
                break;
            case AnchorPosition.TOP_LEFT:
                //Do nothing..
                break;
            case AnchorPosition.TOP_RIGHT:
                origo.x = Screen.width - viewport.width;
                break;
        }
        return origo;
    }
}
public interface ProfilerVisualizer
{
    void OnRecorderAttached(Profiler.RecorderData newLabel);
    void OnRecorderDetached(Profiler.RecorderData remLabel);
    void OnRecorderChanged(Profiler.ProfilerData data);
    void OnUpdate(Profiler.ProfilerData data);
    void OnGUI();
}
