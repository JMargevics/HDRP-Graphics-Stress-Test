using System;
using UnityEngine;

class CumulativeHistogram : ProfilerVisualizer
{
    Material mat;

    float maxFrameTime = 0.0001f; //To prevent NaN first frame
    int maxFrameLifetime = 0;

    int samplingSize = 0;
    Mesh mesh;
    Vector3[] vertices;
    Color[] colors;
    float[] previousPosition;
    bool isDirty;
    bool[] shouldRender;
    GraphLegendInfo legend;
    VisualizerLayout layout;
    private CumulativeHistogram()
    {
        Shader shaders = Shader.Find("Hidden/HistogramSimple");
        if (shaders == null)
        {
            throw new Exception("Failed to locate shader");
        }
        legend = new GraphLegendInfo();
        isDirty = true;
        mat = new Material(shaders);
    }

    private CumulativeHistogram(VisualizerLayout layout) : this()
    {
        this.layout = layout;
    }

    public static VisualizerLayout Create(Profiler profiler, VisualizerLayout layout = null)
    {
        if (layout == null)
        {
            layout = new VisualizerLayout();
        }
        profiler.AttachVisualizer(new CumulativeHistogram(layout));
        return layout;
    }

    private class GraphLegendInfo
    {
        public float[] positions;
        public string[] descriptions
        {
            set
            {
                labelSizes = new Vector2[value.Length];
                labels = new GUIContent[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    labels[i] = new GUIContent("<b><color=#FFFFFF>" + value[i] + "</color></b>");
                    labelSizes[i] = style.CalcSize(labels[i]);
                }
            }
        }
        public GUIStyle style = GUIStyle.none;
        public Vector2[] labelSizes
        {
            private set;
            get;
        }
        public GUIContent[] labels
        {
            private set;
            get;
        }
    }
    void CreateBuffers(Profiler.ProfilerData data)
    {
        previousPosition = new float[data.samplingSize];
        samplingSize = data.samplingSize;
        colors = new Color[data.recorderData.Count + 1];
        colors[data.recorderData.Count] = Color.red; //Last color is used for the frametime lines
        shouldRender = new bool[data.recorderData.Count + 1];
        shouldRender[data.recorderData.Count] = true; //Always render the runtime labels
        vertices = new Vector3[2 * data.samplingSize + 2 * data.samplingSize * data.recorderData.Count + 2 * 3];
        mesh = new Mesh();
        mesh.MarkDynamic();
        mesh.subMeshCount = data.recorderData.Count + 1; //+1 for runtime labels
        mesh.vertices = vertices;
        int[] indices = new int[6 * data.samplingSize];

        for (int i = 1; i <= data.recorderData.Count; i++)
        {
            for (int j = 0; j < data.samplingSize; j++)
            {
                indices[0 + j * 6] = 0 + (i - 1) * (2 * data.samplingSize) + j * 2; //bottom left
                indices[1 + j * 6] = 0 + i * (2 * data.samplingSize) + j * 2; //top left
                indices[2 + j * 6] = 1 + i * (2 * data.samplingSize) + j * 2; //top right

                indices[3 + j * 6] = 0 + (i - 1) * (2 * data.samplingSize) + j * 2; //bottom left
                indices[4 + j * 6] = 1 + i * (2 * data.samplingSize) + j * 2; //top right
                indices[5 + j * 6] = 1 + (i - 1) * (2 * data.samplingSize) + j * 2; //bottom right
            }
            mesh.SetIndices(indices, MeshTopology.Triangles, i - 1);
        }

        int offset;
        int cacheSize = data.samplingSize;
        for (int j = 0; j < cacheSize; j++)
        {
            offset = j * 2;
            //bottom left
            vertices[0 + offset].x = cacheSize - j - 1;
            vertices[0 + offset].y = 0;
            //bottom right
            vertices[1 + offset].x = cacheSize - j;
            vertices[1 + offset].y = 0;
        }
    }

    void GenerateGraphLegend()
    {
        if (maxFrameTime < 1.5f)
        {
            legend.positions =  new float[] { 1f , 0.25f , 0.1f };
            legend.descriptions = new string[] { "1ms", "0.25ms", "0.1ms" };
        }
        else if (maxFrameTime < 10)
        {
            legend.positions = new float[] { 8f , 4f , 1f };
            legend.descriptions = new string[] { "8ms", "4ms", "1ms" };
        }
        else if (maxFrameTime < 30)
        {
            legend.positions = new float[] { 16f , 10f, 5f};
            legend.descriptions = new string[] { "16ms", "10ms", "5ms" };
        }
        else if (maxFrameTime < 100)
        {
            legend.positions = new float[] { 66f , 33 , 16 };
            legend.descriptions = new string[] { "66ms", "33ms", "16ms" };
        }
        else
        {
            legend.positions = new float[] { 500 , 200 , 66 };
            legend.descriptions = new string[] { "500ms", "200ms", "66ms" };
        }

        int toRender = 0;

        for (int i = 2; i >= 0; i--)
        {
            if ((legend.positions[i] <= maxFrameTime))
            {
                ++toRender;
            }
            else
            {
                break;
            }
        }

        int[] lines = new int[toRender * 2];

        toRender = 0;
        for (int i = 0; i < 3; i++)
        {
            if ((legend.positions[i] > maxFrameTime))
                continue;

            vertices[vertices.Length - 2 * i - 1].x = 0;
            vertices[vertices.Length - 2 * i - 1].y = legend.positions[i];
            vertices[vertices.Length - 2 * i - 2].x = samplingSize;
            vertices[vertices.Length - 2 * i - 2].y = legend.positions[i];
            lines[toRender++] = vertices.Length - 2 * i - 1;
            lines[toRender++] = vertices.Length - 2 * i - 2;
        }

        //We update this sub-mesh indices here because it allows us to hide lines that does not fit on the graph
        mesh.SetIndices(lines, MeshTopology.Lines, mesh.subMeshCount - 1);
    }

    void ProfilerVisualizer.OnGUI()
    {
        if (!layout.visible || layout.transparancy < 0.05f)
            return;

        Rect area = layout.getPosition();

        if (layout.visible)
        {
            Vector2 position = area.position;
            position.x = (2 * position.x) / Screen.width - 1;
            position.y = ((2 * (Screen.height - position.y - area.height)) / Screen.height - 1);
            float scaleX = 2 * ((area.width / samplingSize) / Screen.width);
            float scaleY = 2 * ((area.height / maxFrameTime) / Screen.height);
            mat.SetMatrix("_MVP", Matrix4x4.TRS(position, Quaternion.identity, new Vector3(scaleX, scaleY)));
            mat.SetFloat("_Transparancy", layout.transparancy);
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                if (shouldRender[i])
                {
                    mat.color = colors[i];
                    mat.SetPass(0);
                    Graphics.DrawMeshNow(mesh, Vector2.zero, Quaternion.identity, i);
                }
            }
        }
        legend.style = GUI.skin.textArea;
        legend.style.richText = true;
        for (int i = 0; i < legend.labels.Length; i++)
        {
            Vector2 pos = new Vector2(area.x + 2, area.y + area.height - (legend.positions[i] / maxFrameTime) * area.height - legend.labelSizes[i].y);
            if (pos.x >= area.x && (pos.y + legend.labelSizes[i].y) >= area.y)
            {
                GUI.Box(new Rect(pos, legend.labelSizes[i]), legend.labels[i], legend.style);
            }
        }
    }

    void ProfilerVisualizer.OnUpdate(Profiler.ProfilerData data)
    {
        if (isDirty)
        {
            CreateBuffers(data);
            isDirty = false;
        }

        if (maxFrameTime > 1000)
        {
            maxFrameTime = 1000;
        }

        Array.Clear(previousPosition, 0, previousPosition.Length); //Zero out

        int offset;
        CircularBuffer buffer;
        int cacheSize;
        for (int i = 0; i < data.recorderData.Count; ++i)
        {
            colors[i] = data.recorderData[i].layout.color;
            buffer = data.recorderData[i].buffer;
            cacheSize = buffer.size;
            if (!data.recorderData[i].layout.visible)
            {
                shouldRender[i] = false;
                Array.Copy(vertices, (i - 1) * (2 * cacheSize) + 2 * cacheSize, vertices, i * (2 * cacheSize) + 2 * cacheSize, 2 * cacheSize); //We use the previous recorders height, which gives us a zero height
                continue;
            }
            shouldRender[i] = true;
            for (int j = 0; j < cacheSize; j++)
            {
                float bufferValue = previousPosition[j] + buffer.get(j);
                offset = i * (2 * cacheSize) + j * 2 + 2 * cacheSize;
                //top left
                vertices[0 + offset].x = cacheSize - j - 1;
                vertices[0 + offset].y = bufferValue;

                //top right
                vertices[1 + offset].x = cacheSize - j;
                vertices[1 + offset].y = bufferValue;
                previousPosition[j] = bufferValue;
            }
        }


        //Last max frame expired, recalculate it
        if (--maxFrameLifetime < 0)
        {
            maxFrameTime = 0.0001f;
            for (int i = 0; i < previousPosition.Length; i++)
            {
                if (previousPosition[i] > maxFrameTime)
                {
                    maxFrameTime = previousPosition[i];
                    maxFrameLifetime = previousPosition.Length - i;
                }
            }
            GenerateGraphLegend();
        }
        else if (previousPosition[0] > maxFrameTime)
        {
            maxFrameTime = previousPosition[0];
            maxFrameLifetime = previousPosition.Length;
            GenerateGraphLegend();
        }

        //Update vertices
        mesh.vertices = vertices;
        //Update mesh data
        mesh.UploadMeshData(false);
    }

    void ProfilerVisualizer.OnRecorderAttached(Profiler.RecorderData newLabel)
    {
        isDirty = true;
    }

    void ProfilerVisualizer.OnRecorderDetached(Profiler.RecorderData label)
    {
        isDirty = true;
    }

    void ProfilerVisualizer.OnRecorderChanged(Profiler.ProfilerData data)
    {
        // We dont care about this event..
    }
}
