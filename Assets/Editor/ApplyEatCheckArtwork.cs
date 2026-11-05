using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class ApplyEatCheckArtwork
{
    const string Art = "Assets/Resources/FoodTasteEatCheck/";
    const string Scene = "Assets/Scenes/MainFuncScene.unity";
    static readonly Color Ink = new Color(.35f, .17f, .07f, 1f);
    static Sprite Load(string name) { var s = AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png"); if (s == null) throw new Exception("Missing sprite: " + name); return s; }
    static void ImageArt(Transform t, string name) { var i = t.GetComponent<Image>(); if (i == null) i = t.gameObject.AddComponent<Image>(); i.sprite = Load(name); i.color = Color.white; i.type = Image.Type.Simple; i.preserveAspect = false; }
    static void Place(Transform t, float x, float y, float w, float h) { var r=(RectTransform)t; r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f); r.anchoredPosition=new Vector2(x,y); r.sizeDelta=new Vector2(w,h); r.localScale=Vector3.one; }
    static void Style(TMP_Text t, float max) { if(t==null)return; t.color=Ink; t.enableAutoSizing=true; t.fontSizeMin=16; t.fontSizeMax=max; t.enableWordWrapping=false; t.overflowMode=TextOverflowModes.Ellipsis; }
    static void Backing(Transform target, string name, float x, float y, float w, float h) { var parent=target.parent; var bg=parent.Find(name+"_Art"); if(bg==null){ var go=new GameObject(name+"_Art",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image)); go.layer=target.gameObject.layer; bg=go.transform; bg.SetParent(parent,false); } Place(bg,x,y,w,h); ImageArt(bg,name); bg.GetComponent<Image>().raycastTarget=false; bg.SetSiblingIndex(target.GetSiblingIndex()); target.SetAsLastSibling(); }
    static void ButtonArt(Button b, string name, float x,float y,float w,float h, bool icon) { Place(b.transform,x,y,w,h); var ownText=b.GetComponent<TMP_Text>(); if(ownText!=null) { Backing(b.transform,name,x,y,w,h); Style(ownText,25); ownText.alignment=TextAlignmentOptions.Center; ownText.margin=new Vector4(w*.22f,0,12,0); } else { ImageArt(b.transform,name); foreach(var t in b.GetComponentsInChildren<TMP_Text>(true)) { if(icon)t.gameObject.SetActive(false); else { Style(t,25); t.alignment=TextAlignmentOptions.Center; } } } }
    static void Input(TMP_InputField field,string name,float y) { Transform container=field.transform.parent; Place(container,-465,y,420,125); ImageArt(container,name); Place(field.transform,30,0,315,60); var inner=field.GetComponent<Image>(); if(inner!=null) inner.color=new Color(1,1,1,0); Style(field.textComponent,27); if(field.placeholder is TMP_Text)Style((TMP_Text)field.placeholder,25); var area=field.textViewport; if(area!=null) { area.anchorMin=Vector2.zero; area.anchorMax=Vector2.one; area.offsetMin=new Vector2(12,5); area.offsetMax=new Vector2(-12,-5); } }
    [MenuItem("Tools/FoodTaste/Apply Eat Check Artwork")]
    public static void Apply() {
        string[] names={"EatCheckPanel","Morning","AfterNoon","Night","MorningText","AfternoonText","NightText","MorningInputBtn","MorningDeleteBtn","AfternoonInputBtn","AfternoonDeleteBtn","NightInputBtn","NightDeleteBtn","TodayEatenText","TotalEatenText","EatCheck_ScrollView","EatCheck_FoodRow","TotalCountBtn","CheckEatenDayBtn","ExitBtn","FoodCheckMessage"};
        foreach(var name in names) { var imp=AssetImporter.GetAtPath(Art+name+".png") as TextureImporter; if(imp==null) throw new Exception("Missing image: "+name); if(imp.textureType!=TextureImporterType.Sprite || !imp.alphaIsTransparency){imp.textureType=TextureImporterType.Sprite; imp.spriteImportMode=SpriteImportMode.Single; imp.alphaIsTransparency=true; imp.SaveAndReimport();} }
        var scene=EditorSceneManager.GetActiveScene();
        var manager=UnityEngine.Object.FindObjectOfType<EatCheckPanelFunc>(true);
        if(manager==null) throw new Exception("EatCheckPanelFunc missing");
        Transform panel=manager.EatenPanel.transform.Find("EatCheckPanel"); if(panel==null)throw new Exception("Eat check panel missing");
        ImageArt(panel,"EatCheckPanel"); panel.localScale = new Vector3(.96f, .96f, 1f);
        float[] ys={170,-40,-250}; string[] rows={"Morning","AfterNoon","Night"};
        for(int i=0;i<3;i++){ var t=panel.Find(rows[i]); if(t==null)throw new Exception("Missing "+rows[i]); Place(t,-475,ys[i],820,205); ImageArt(t,rows[i]); t.GetComponent<Image>().raycastTarget=false; }
        Input(manager.MorningText,"MorningText",ys[0]); Input(manager.AfterNoonText,"AfternoonText",ys[1]); Input(manager.NightText,"NightText",ys[2]);
        ButtonArt(manager.MorningInputBtn,"MorningInputBtn",-165,ys[0],94,94,true); ButtonArt(manager.MorningDeleteBtn,"MorningDeleteBtn",-65,ys[0],94,94,true);
        ButtonArt(manager.AfterNoonInputBtn,"AfternoonInputBtn",-165,ys[1],94,94,true); ButtonArt(manager.AfterNoonDeleteBtn,"AfternoonDeleteBtn",-65,ys[1],94,94,true);
        ButtonArt(manager.NightInputBtn,"NightInputBtn",-165,ys[2],94,94,true); ButtonArt(manager.NightDeleteBtn,"NightDeleteBtn",-65,ys[2],94,94,true);
        Transform today=panel.Find("TodayEatenText"); Place(today,-475,335,490,55); Style(today.GetComponent<TMP_Text>(),32); Backing(today,"TodayEatenText",-475,345,600,155);
        Place(manager.HeadText.transform,475,335,490,55); Style(manager.HeadText,32); Backing(manager.HeadText.transform,"TotalEatenText",475,345,600,155);
        Place(manager.ScrollView.transform,475,-40,650,610); ImageArt(manager.ScrollView.transform,"EatCheck_ScrollView");
        var scroll=manager.ScrollView.GetComponent<ScrollRect>(); if(scroll!=null && scroll.viewport!=null){ scroll.horizontalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide; scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide; var viewportImage=scroll.viewport.GetComponent<Image>(); if(viewportImage!=null)viewportImage.color=new Color(1,1,1,.01f); }
        ButtonArt(manager.TotalCountBtn,"TotalCountBtn",310,-390,290,100,false); ButtonArt(manager.CheckEatenDayBtn,"CheckEatenDayBtn",640,-390,290,100,false);
        ButtonArt(manager.ExitBtn,"ExitBtn",840,425,105,105,true);
        Place(manager.FoodCheckMessage.transform,0,0,780,210); ImageArt(manager.FoodCheckMessage.transform,"FoodCheckMessage"); foreach(var t in manager.FoodCheckMessage.GetComponentsInChildren<TMP_Text>(true))Style(t,30); manager.ConfigureFoodCheckMessage();
        string prefabPath=AssetDatabase.GetAssetPath(manager.EatenFoodPrefab); var row=PrefabUtility.LoadPrefabContents(prefabPath);
        try { ImageArt(row.transform,"EatCheck_FoodRow"); foreach(var t in row.GetComponentsInChildren<TMP_Text>(true))Style(t,28); PrefabUtility.SaveAsPrefabAsset(row,prefabPath); } finally {PrefabUtility.UnloadPrefabContents(row);}
        EditorSceneManager.MarkSceneDirty(scene); if(!EditorSceneManager.SaveScene(scene))throw new Exception("Scene save failed"); AssetDatabase.SaveAssets(); Debug.Log("EAT_CHECK_ART_SUCCESS");
    }
    public static void ApplyBatch(){ EditorSceneManager.OpenScene(Scene); Apply(); }
}
