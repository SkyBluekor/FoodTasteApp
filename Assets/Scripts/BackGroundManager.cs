using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundManager : MonoBehaviour
{
    public AudioClip bgm1;
    public AudioClip bgm2;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayLoop());
    }

    IEnumerator PlayLoop()
    {
        while(true)
        {
            audioSource.clip = Random.value > 0.5 ? bgm1 : bgm2;
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
