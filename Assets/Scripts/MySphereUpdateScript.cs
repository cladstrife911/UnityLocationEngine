using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySphereUpdateScript : MonoBehaviour
{
    public GameObject sphere;
    public MeshRenderer myRenderer;

    private void Start()
    {
        Debug.Log("MySphereUpdateScript started");
    }

    public void OnClickChangeColor()
    {
        Debug.Log("OnClickChangeColor");
        myRenderer = sphere.GetComponent<MeshRenderer>();
        myRenderer.enabled = !myRenderer.enabled;

        
    }

    public void OnClickBigger()
    {
        Debug.Log("OnClickBigger");
        sphere.transform.localScale += new Vector3(1, 1, 1);
    }

    public void OnClickSmaller()
    {
        Debug.Log("OnClickSmaller");
        sphere.transform.localScale -= new Vector3(1, 1, 1);
    }

    public void OnClickLessTransparent()
    {
        Debug.Log("OnClickMoreTransparent");
        Color color = this.GetComponent<MeshRenderer>().material.color;
        if(color.a<1.0f)
            color.a += 0.1f;
        this.GetComponent<MeshRenderer>().material.color = color;
    }

    public void OnClickMoreTransparent()
    {
        Debug.Log("OnClickLessTransparent");
        Color color = this.GetComponent<MeshRenderer>().material.color;
        if (color.a > 0.0f)
            color.a -= 0.1f;
        this.GetComponent<MeshRenderer>().material.color = color;
    }
}
