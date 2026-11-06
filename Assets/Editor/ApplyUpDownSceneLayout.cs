using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class ApplyUpDownSceneLayout
{
    [MenuItem("Tools/FoodTaste/Apply Up Down Scene Layout")]
    public static void Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/UP&DownGameScene.unity")
        {
            Debug.LogError("Open UP&DownGameScene before applying its layout.");
            return;
        }

        GamePlayFunc game = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<GamePlayFunc>(true)).Single();
        GameBackGroundFunc menu = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<GameBackGroundFunc>(true)).Single();
        Undo.RegisterFullObjectHierarchyUndo(scene.GetRootGameObjects().First(root => root.GetComponentInChildren<GamePlayFunc>(true) != null), "Apply UP & DOWN layout");
        game.ApplySceneLayout();
        menu.AlignRankingRows();
        Transform playBackground = game.GamePlayPanel.transform.Find("GamePlayBackGround");
        playBackground.gameObject.SetActive(true);
        SetCardHitArea(playBackground, "LeftFoodCardFrame", "LeftFoodHitArea", game.LeftFood);
        SetCardHitArea(playBackground, "RightFoodCardFrame", "RightFoodHitArea", game.RightFood);
        EditorSceneManager.MarkSceneDirty(scene);

        // Leave the editor and saved scene in the same menu starting state.
        game.GameOverPanel.SetActive(false);
        game.GamePlayPanel.SetActive(false);
        bool saved = EditorSceneManager.SaveScene(scene);
        Debug.Log(saved ? "UP & DOWN scene layout saved." : "UP & DOWN scene layout could not be saved.");
    }

    private static void SetCardHitArea(Transform background, string frameName, string areaName, Button target)
    {
        RectTransform frame = background.Find(frameName) as RectTransform;
        if (frame == null) throw new System.InvalidOperationException("Card frame not found: " + frameName);
        Transform existing = background.Find(areaName);
        GameObject area = existing != null ? existing.gameObject :
            new GameObject(areaName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(FoodCardHitArea));
        RectTransform rect = (RectTransform)area.transform;
        rect.SetParent(background, false);
        rect.anchorMin = frame.anchorMin;
        rect.anchorMax = frame.anchorMax;
        rect.pivot = frame.pivot;
        rect.anchoredPosition = frame.anchoredPosition;
        rect.sizeDelta = frame.sizeDelta;
        rect.localScale = frame.localScale;
        rect.SetAsLastSibling();
        Image image = area.GetComponent<Image>();
        image.color = Color.clear;
        image.raycastTarget = true;
        area.GetComponent<FoodCardHitArea>().foodButton = target;
    }
}
