using UnityEngine;
using UnityEditor;
using TMPro;
public class FontChanger
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Change All Fonts", menuItem = "CONTEXT/TextMeshProUGUI/Change All Fonts")]
    public static void ChangeAllFonts(MenuCommand command)
    {
        TextMeshProUGUI font = (TextMeshProUGUI)command.context;

        foreach (TextMeshProUGUI textObject in GameObject.FindObjectsOfType<TextMeshProUGUI>())
        {
            textObject.font = font.font;
            textObject.UpdateFontAsset();
        }


        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }
#endif
}