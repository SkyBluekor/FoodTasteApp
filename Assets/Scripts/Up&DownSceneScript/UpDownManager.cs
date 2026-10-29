using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UpDownManager : MonoBehaviour
{
    public GameObject SunObj,MoonObj;
    private Vector3 speed = new Vector3(0f,700f,0f);
    private float distance = 780f;
    private float startingY = -390f;
    private float endY = 390f;
    private bool MoveDirection = true;
    // Start is called before the first frame update
    void Start()
    {
    }   

    // Update is called once per frame
    void Update()
    {
        if(MoveDirection)
        {
            SunObj.transform.localPosition -= speed*Time.deltaTime;
            MoonObj.transform.localPosition += speed * Time.deltaTime;
        }
        else
        {
            SunObj.transform.localPosition += speed * Time.deltaTime;
            MoonObj.transform.localPosition -= speed * Time.deltaTime;
        }

        if(SunObj.transform.localPosition.y >= endY)
        {
            MoveDirection = true;
        }
        else if(SunObj.transform.localPosition.y <= startingY)
        {
            MoveDirection = false;
        }

    }
}
