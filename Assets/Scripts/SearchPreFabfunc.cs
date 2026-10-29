using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchPreFabfunc : MonoBehaviour
{
    private For_Search_history for_Search_History; 
    
    // Start is called before the first frame update
    private void Start()
    {
        GameObject FunctionManager = GameObject.Find("FunctionManager");
        for_Search_History = FunctionManager.GetComponent<For_Search_history>();
        Button SearchPrefabBtn = GetComponent<Button>();

        if (SearchPrefabBtn != null)
        {
            SearchPrefabBtn.onClick.AddListener(()=> 
            {
                if (for_Search_History != null)
                {
                    
                    for_Search_History.ForPrePanelObjFunction();
                }
            });
        }
        else
        {
            Debug.LogWarning("Button component not found on the game object.");
        }
    }

}
