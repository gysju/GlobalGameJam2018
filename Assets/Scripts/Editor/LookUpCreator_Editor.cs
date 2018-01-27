using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LookUpCreator))]
public class LookUpCreator_Editor : Editor
{
    private LookUpCreator lutCrea;

    public override void OnInspectorGUI()
    {
        lutCrea = (LookUpCreator) target;
        base.DrawDefaultInspector();
        if (GUILayout.Button("Save Textures"))
        {
            string path = AssetDatabase.GetAssetPath(lutCrea.Target);
            path = path.Remove(path.Length - 4);
            AssetDatabase.CreateAsset(lutCrea.GradientToLUT(lutCrea.Gradient), path + lutCrea.Name + ".asset");
            lutCrea.Target.SetTexture(lutCrea.Name, AssetDatabase.LoadAssetAtPath<Texture>(path + lutCrea.Name + ".asset"));

            AssetDatabase.Refresh();
        }
    }
	
}
