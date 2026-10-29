using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FIleLoadSystem : MonoBehaviour
{
    private ImageLoader imageLoader;

    private void Awake()
    {
        imageLoader = GetComponent<ImageLoader>();
    }
   
    public void LoadFile(FileInfo file)
    {
        Debug.Log("start");
        

        if (file.FullName.Contains(".jpg") || file.FullName.Contains(".png"))
        {
            Debug.Log("Image Test");
            
            imageLoader.OnLoad(file);
        }
        else
        {

        }


    }
    /*
    private void OffAllPanel()
    {
        imageLoader.OffLoad();
    }
    */
}
