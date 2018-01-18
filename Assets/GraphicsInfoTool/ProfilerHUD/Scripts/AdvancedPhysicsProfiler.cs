using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedPhysicsProfiler : MonoBehaviour
{
    Profiler profiler;
    [System.NonSerialized]
    bool initialized = false;
    struct LabelLayout
    {
        public string label;
        public int indent
        {
            get;
            set;
        }
    }

    static  LabelLayout[] labels = new LabelLayout[]
    {
        new LabelLayout {indent = 0, label =   "Physics.Processing" },

        new LabelLayout {indent = 1, label =   "PhysX.PxsDynamics.solverStart" },
        new LabelLayout {indent = 2, label =   "PhysX.PxsAABBManager.aggregateOverlap" },
        new LabelLayout {indent = 2, label =   "PhysX.PxsAABBManager.aggregateAABB" },
        new LabelLayout {indent = 2, label =   "PhysX.PxsAABBManager.singleAABB"    },
        new LabelLayout {indent = 2, label =   "PhysX.PxsAABBManager.actorAABB" },
        new LabelLayout {indent = 2, label =   "PhysX.PxsAABBManager.broadphaseWork", },
        new LabelLayout {indent = 2, label =   "PhysX.PxsAABBManager.processBroadphaseResults", },
        new LabelLayout {indent = 2, label =   "PhysX.PxsDynamics.solverCreateFinalizeContraints", },

        new LabelLayout {indent = 1, label =   "PhysX.ScScene.islandGen", },
        new LabelLayout {indent = 1, label =   "PhysX.scScene.collision", },
        new LabelLayout {indent = 1, label =   "PhysX.PxsSap.sapUpdateWork", },
        new LabelLayout {indent = 1, label =   "PhysX.PhysX.PxsAABBManager.aggregateAABB" },
    };


    // Use this for initialization
    void OnEnable()
    {
        if (!initialized)
        {
            profiler = new Profiler(240);
            for (int i = 0; i < labels.Length; i++)
            {
                Profiler.RecorderLayout layout = new Profiler.RecorderLayout();
                layout.color = new Color(Random.Range(0.50f, 0.75f), Random.Range(0.5f, 0.75f), Random.Range(0.5f, 0.75f));
                layout.visible = true;
                profiler.CreateRecorder(labels[i].label, layout);
            }
            initialized = true;
        }
        else
        {
            profiler.ReloadRecorders();
        }
        List<int> indents = new List<int>(labels.Length);
        for (int i = 0; i < labels.Length; i++)
        {
            indents.Add(labels[i].indent);
        }
        VisualizerLayout graphLayout = HUD.Create(profiler, null, indents);
        graphLayout.align = VisualizerLayout.AnchorPosition.BOTTOM_LEFT;
        graphLayout.layout.size = new Vector2(720, 768);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            profiler.visible = !profiler.visible;
        }
        profiler.Update();
    }

    private void OnGUI()
    {
        profiler.OnGUI();
    }
}
