//To use this, just drag the script to the object having a Renderer

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

[ExecuteInEditMode]
public class ShaderInfo : MonoBehaviour 
{
	private Material mat;
	private Shader shader;
    public Font font;

	private TextMesh tm;
    private MeshRenderer mr;
	private Material tm_mat;

    [SerializeField]
    private GameObject go;

	void Awake ()
	{
        mat = GetComponent<Renderer> ().sharedMaterial;
		shader = mat.shader;

        if(go == null || Application.isPlaying)
        { 
            if(go != null)
            {
                DestroyImmediate(go);
            }
            //Setup the TextMesh object
		    go = new GameObject ();
		    tm = new TextMesh ();
		    go.AddComponent (tm.GetType ());
		    tm = go.GetComponent<TextMesh> ();
		    tm_mat = tm.GetComponent<Renderer> ().sharedMaterial;
		    tm_mat.renderQueue += 100;
            go.layer = 10;
            go.tag = "InfoCam";
            go.name = "ShaderInfo";
		    //TextMesh settings
		    tm.fontSize = 30;
            if (font != null )
            {
                tm.font = font;
                mr = go.GetComponent<MeshRenderer>();
                mr.material = font.material;
            
            }
		    tm.characterSize = 1.00f / tm.fontSize;
		    tm.anchor = TextAnchor.MiddleCenter;
		    tm.alignment = TextAlignment.Center;
		    //TextMesh Position
		    Vector3 thisscreenpos = Camera.main.WorldToScreenPoint (this.transform.position);
		    go.transform.position = Camera.main.ScreenToWorldPoint(thisscreenpos);
		    go.transform.LookAt (Camera.main.transform);
		    float distance = Mathf.Abs(Vector3.Distance (go.transform.position, Camera.main.transform.position));
		    distance /= 5.0f;
		    go.transform.localScale = new Vector3 (
			    -1 * go.transform.localScale.x * distance,
			    go.transform.localScale.y * distance,
			    go.transform.localScale.z * distance);
		    //Initial TextMesh Value
		    tm.text = "error";

            UpdateShaderInfo();
        }

    }

    /*
    void OnGUI()
    {
        GUIStyle gs = new GUIStyle(GUI.skin.label);
        gs.fontSize = 20;
        gs.alignment = TextAnchor.MiddleCenter;
        //gs.stretchHeight = true;
       // gs.stretchWidth = true;

        Vector3 thisscreenpos = Camera.main.WorldToScreenPoint(this.transform.position);

        GUI.color = Color.white;
        GUI.contentColor = Color.white;
        GUI.Label(new Rect(thisscreenpos.x, Screen.height - thisscreenpos.y, 100,100), "ress Me",gs);
    }
    */

    public void UpdateShaderInfo()
	{
        tm.text = "";

        //Shader is supported?
        if (shader.isSupported) 
        {
            tm.color = Color.green;
            tm.text += "supported" + System.Environment.NewLine;
        } 
        else 
        {
            tm.color = Color.red;
            tm.text += "not supported" + System.Environment.NewLine;
        }

        //tm.text += "EnableInstancing?" + mat.enableInstancing.ToString() + System.Environment.NewLine;
        //tm.text += "No. of pass: " + mat.passCount.ToString() + System.Environment.NewLine;
    }
}
