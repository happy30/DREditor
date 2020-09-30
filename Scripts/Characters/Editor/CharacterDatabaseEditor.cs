using DREditor.Utility;
using UnityEditor;
using UnityEngine;

namespace DREditor.Characters.Editor
{
    [CustomEditor(typeof(CharacterDatabase))]
    public class CharacterDatabaseEditor : UnityEditor.Editor
    {
        private CharacterDatabase cdb;

        private void OnEnable() => cdb = target as CharacterDatabase;

        public override void OnInspectorGUI()
        {
            CreateForm();
            EditorUtility.SetDirty(cdb);
        }

        private void CreateForm()
        {
            Utility.Editor.HandyFields.Label("Character Database");

            if (GUILayout.Button("Add New Character"))
            {
                cdb.Characters.Add(CreateInstance<Character>());
            }

            if (cdb.Characters == null)
            {
                return;
            }
            for (int i = 0; i < cdb.Characters.Count; i++)
            {
                var character = cdb.Characters[i];
                using (new EditorGUILayout.HorizontalScope("Box"))
                {
                    ShowBaseExpression(character);
                    ShowCharacterInfo(character, i);
                    GUILayout.FlexibleSpace();
                }
            }
        }
        private void ShowBaseExpression(Character character)
        {
            if (character == null)
            {
                return;
            }
            GUIStyle expr = new GUIStyle();
            if (!character.Expressions.Empty() && character.Expressions[0].Sprite)
            {
                var tex = Utility.Editor.HandyFields.GetMaterialTexture(character.Expressions[0].Sprite);
                if (tex)
                {
                    expr.normal.background = tex;
                }
            }
            EditorGUILayout.LabelField(GUIContent.none, expr, GUILayout.Width(100), GUILayout.Height(100));
        }
        private void ShowCharacterInfo(Character character, int idx)
        {
            using (new EditorGUILayout.VerticalScope("Box"))
            {
                var bigLabelStyle = new GUIStyle
                {
                    fontSize = 25,
                    fontStyle = FontStyle.Bold
                };
                if (character != null) GUILayout.Label(character.LastName + " " + character.FirstName, bigLabelStyle);

                GUILayout.FlexibleSpace();

                if (character != null) cdb.Characters[idx] = Utility.Editor.HandyFields.UnityField(character, 100);

                if (GUILayout.Button("Remove", GUILayout.Width(100)))
                {
                    cdb.Characters.Remove(character);
                }
            }
        }
    }
}
