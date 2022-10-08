using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
 
public static class ShadowCaster2DExtensions
{
    /// <param name="shadowCaster">The object to modify.</param>
    /// <param name="path">The new path to define the shape of the shadow caster.</param>
    public static void SetPath(this ShadowCaster2D shadowCaster, Vector3[] path)
    {
        FieldInfo shapeField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);
        shapeField.SetValue(shadowCaster, path);
    }
    public static void SetPathHash(this ShadowCaster2D shadowCaster, int hash)
    {
        FieldInfo hashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash", BindingFlags.NonPublic | BindingFlags.Instance);
        hashField.SetValue(shadowCaster, hash);
    } 
}

public class ShadowGenerator
{
 
#if UNITY_EDITOR
 
    [UnityEditor.MenuItem("Generate Shadow Casters", menuItem = "Tools/Generate Shadow Casters")]
    public static void GenerateShadowCasters()
    {
        CompositeCollider2D[] colliders = GameObject.FindObjectsOfType<CompositeCollider2D>();
 
        foreach (CompositeCollider2D collider in colliders) GenerateTilemapShadowCasterInEditor(collider, false);
    }
 
    [UnityEditor.MenuItem("Generate Shadow Casters (Self Shadows)", menuItem = "Tools/Generate Shadow Casters (Self Shadows)")]
    public static void GenerateShadowCastersSelfShadows()
    {
        CompositeCollider2D[] colliders = GameObject.FindObjectsOfType<CompositeCollider2D>();
 
        foreach (CompositeCollider2D collider in colliders) GenerateTilemapShadowCasterInEditor(collider, true);
    }
 
    public static void GenerateTilemapShadowCasterInEditor(CompositeCollider2D collider, bool selfShadows)
    {
        GenerateTilemapShadowCaster(collider, selfShadows);
 
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }

    [UnityEditor.MenuItem("Destroy All Shadow Casters", menuItem = "Tools/Destroy All Shadow Casters")]
    public static void DestroyShadowCasters()
    {
        CompositeCollider2D[] colliders = GameObject.FindObjectsOfType<CompositeCollider2D>();
 
        foreach (CompositeCollider2D collider in colliders) 
        {
            ShadowCaster2D[] currentShadows = collider.GetComponentsInChildren<ShadowCaster2D>();
            foreach (ShadowCaster2D shadowCaster in currentShadows) GameObject.DestroyImmediate(shadowCaster.gameObject);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }
 
#endif

    public static void GenerateTilemapShadowCaster(CompositeCollider2D collider, bool selfShadows)
    {
        ShadowCaster2D[] currentShadows = collider.GetComponentsInChildren<ShadowCaster2D>();
 
        foreach (ShadowCaster2D shadowCaster in currentShadows)
        {
            if(shadowCaster.transform.parent != collider.transform) continue;
            GameObject.DestroyImmediate(shadowCaster.gameObject);
        }
 
        int pathCount = collider.pathCount;
        List<Vector2> pointsInPath = new List<Vector2>();
        List<Vector3> pointsInPath3D = new List<Vector3>();
 
        for (int i = 0; i < pathCount; i++)
        {
            collider.GetPath(i, pointsInPath);
 
            GameObject newShadowCaster = new GameObject("ShadowCaster2D");
            newShadowCaster.isStatic = true;
            newShadowCaster.transform.SetParent(collider.transform, false);
 
            for(int j = 0; j < pointsInPath.Count; j++) pointsInPath3D.Add(pointsInPath[j]);
 
            ShadowCaster2D component = newShadowCaster.AddComponent<ShadowCaster2D>();
            component.SetPath(pointsInPath3D.ToArray());
            component.SetPathHash(Random.Range(int.MinValue, int.MaxValue));
            component.selfShadows = selfShadows;
            component.Update();
 
            pointsInPath.Clear();
            pointsInPath3D.Clear();
        }
    }
}