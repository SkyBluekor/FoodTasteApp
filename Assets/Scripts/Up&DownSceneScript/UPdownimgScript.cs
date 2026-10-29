using UnityEngine;

public class UPdownimgScript : MonoBehaviour
{
    public float animationSpeed = 1.0f;
    public float maxHeight = 1.0f;
    public float minHeight = 0.2f;

    private bool isIncreasing = true;

    // Update is called once per frame
    void Update()
    {
        float currentY = transform.localScale.y;

        if(currentY >= maxHeight)
        {
            isIncreasing = false;
        }
        else if(currentY <= minHeight) 
        { 
            isIncreasing = true;
        }

        float newY = transform.localScale.y + (isIncreasing ? 1: -1) * animationSpeed * Time.deltaTime;

        newY = Mathf.Clamp(newY, minHeight, maxHeight);

        transform.localScale = new Vector3(transform.localScale.x, newY, transform.localScale.z);
    }
}
