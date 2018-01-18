using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InfoTexts : MonoBehaviour
{
    [SerializeField]
    public GameObject[] golist;

	// Use this for initialization
	void Start ()
    {
        if (Application.isPlaying)
        {
            UpdateList();
            HideInfoText();
        }
    }

    void Update()
    {
        if(!Application.isPlaying)
        {
            UpdateList();
        }
    }

    public void UpdateList()
    {
        golist = GameObject.FindGameObjectsWithTag("InfoCam");
    }


    public void HideInfoText()
    {
        for (int i = 0; i < golist.Length; i++)
        {
            if(golist[i].activeSelf)
            golist[i].SetActive(false);
        }
    }

    public void ShowInfoText()
    {
        for (int i = 0; i < golist.Length; i++)
        {
            if (!golist[i].activeSelf)
                golist[i].SetActive(true);
        }
    }
}
