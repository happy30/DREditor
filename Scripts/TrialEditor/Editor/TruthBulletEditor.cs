using UnityEditor;
using UnityEngine;
using DREditor.Utility.Editor;

namespace DREditor.TrialEditor.Editor
{
    [CustomEditor(typeof(TruthBullet))]
    public class TruthBulletEditor : UnityEditor.Editor
    {
        private TruthBullet bullet;

        private void OnEnable()
        {
            bullet = (TruthBullet)target;
        }

        public override void OnInspectorGUI()
        {
            var titleStyle = new GUIStyle();
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label("Truth Bullet", titleStyle);
            GUILayout.BeginHorizontal(GUILayout.Width(170));
            bullet.Picture = HandyFields.SpriteField("Picture: ", bullet.Picture);
            if (GUILayout.Button("X", GUILayout.Width(18)))
            {
                bullet.Picture = null;
            }
            GUILayout.EndHorizontal();
            bullet.Title = HandyFields.StringField("Title: ", bullet.Title);
            bullet.Description = HandyFields.StringArea("Description: ", bullet.Description);
            EditorUtility.SetDirty(bullet);
        }
    }

}

