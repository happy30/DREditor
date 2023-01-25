#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using DREditor.Utility.Editor;
using System.Linq;

namespace DREditor.Characters.Editor
{
    using Debug = UnityEngine.Debug;
    [CustomEditor(typeof(Student))]
    public class StudentEditor : UnityEditor.Editor
    {
        private Student stu;
        private void OnEnable() => stu = target as Student;

        public override void OnInspectorGUI()
        {
            HandyFields.Label("Character Editor");
            
            NameForm();
            StudentLabel();
            StudentCard();

            DefaultSprite();
            BlackExpression(); //*
            //TrialMaterial(); //*
            //CharacterPortrait();
            MissingSprites();

            HandyFields.Label("Sprites");

            using (new EditorGUILayout.HorizontalScope()) //*
            {
                GUILayout.Label("Show Sprites Mode", GUILayout.Width(120));
                stu.showSprites = EditorGUILayout.Toggle(stu.showSprites);
            }

            if (GUILayout.Button("Import Materials"))
            {
                ImportMaterials();
            }
            if (GUILayout.Button("Import Sprites"))
            {
                ImportSprites();
            }

            if (!stu.showSprites) //* Was Sprites();
                Sprites();
            else
                Unlits();
            
            EditorUtility.SetDirty(stu);
            
        }
        private void ImportMaterials()
        {
            var materials = Selection.GetFiltered(typeof(Material), SelectionMode.Assets).Cast<Material>();
            var mats = materials.ToList();
            /*if(materials.Count() >= 0)
            {
                Debug.LogWarning("Sweden has Disabled this function, Refer to him and " +
                    "don't touch this code.");
                return;
            }*/
            Debug.Log("Mats has " + mats.Count);
            try
            {
                foreach (var mat in mats)
                {
                    Expression e = new Expression
                    {
                        Sprite = mat,
                        Name = mat.name
                    };
                    stu.Expressions.Add(e);
                    stu.Sprites.Add(null);//*
                }
            }
            finally
            {

            }
        }
        private void ImportSprites()
        {
            var sprites = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets).Cast<Texture2D>();
            var spr = sprites.ToList();
            /*if (sprites.Count() >= 0)
            {
                Debug.LogWarning("Sweden has Disabled this function, Refer to him and " +
                    "don't touch this code.");
                return;
            }*/
            List<Sprite> spri = new List<Sprite>();
            Debug.Log("spr has " + spr.Count);
            try
            {
                for (int i = 0; i < spr.Count; i++)
                {
                    spri.Add(AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(spr[i])));
                }

                for (int i = 0; i < stu.Sprites.Count; i++)
                {
                    //stu.Sprites[i].Sprite = spri[i];
                    for (int x = 0; x < spri.Count; x++)
                    {
                        if (stu.Sprites[i].Name == spri[x].name)
                        {
                            stu.Sprites[i].Sprite = spri[x];
                            Debug.Log("Succsessfully Loaded " + spri[x].name);
                            break;
                        }
                    }
                }
            }
            finally
            {

            }
        }
        private void NameForm()
        {
            EditorGUILayout.BeginVertical("Box");
            stu.LastName = HandyFields.StringField("Last Name: ", stu.LastName);
            stu.FirstName = HandyFields.StringField("First Name: ", stu.FirstName);
            stu.StudentCard.Color = HandyFields.ColorField(stu.StudentCard.Color);
            stu.Nameplate = TextureFieldLabeledHorizontal("Default Nameplate: ", stu.Nameplate);
            stu.Headshot = TextureFieldLabeledHorizontal("Default Headshot: ", stu.Headshot);
            stu.TrialNameplate = TextureFieldLabeledHorizontal("Trial Nameplate: ", stu.TrialNameplate);
            stu.TrialPortrait = TextureFieldLabeledHorizontal("Trial Portrait: ", stu.TrialPortrait);
            stu.NSDPortrait = TextureFieldLabeledHorizontal("NSD Nameplate: ", stu.NSDPortrait);
            stu.TrialHeight = HandyFields.FloatField("Trial Height: ", stu.TrialHeight);
            stu.TrialPosition = HandyFields.IntField("Trial Position: ", stu.TrialPosition, 30);
            EditorGUILayout.LabelField("Actor Prefab: ");
            stu.ActorPrefab = HandyFields.UnityField(stu.ActorPrefab, 120, 30);
            using (new EditorGUILayout.HorizontalScope()) //*
            {
                GUILayout.Label("Is Dead : ", GUILayout.Width(55));
                stu.IsDead = EditorGUILayout.Toggle(stu.IsDead);
            }

            
            stu.FriendshipLvl = HandyFields.IntField("Friendship Level: ", stu.FriendshipLvl, 30);
            

            EditorGUILayout.EndVertical();
            using (new EditorGUILayout.VerticalScope("Box"))
            {
                AliasList(stu.Aliases);
            }
        }
        private void StudentLabel()
        {
            var bigLabelStyle = new GUIStyle
            {
                fontSize = 25,
                fontStyle = FontStyle.Bold
            };
            GUI.backgroundColor = stu.StudentCard.Color;
            GUILayout.Space(15);
            using (new EditorGUILayout.HorizontalScope("box"))
            {
                GUILayout.FlexibleSpace();

                var labelText = stu.LastName == "" && stu.FirstName == "" ? "No Name" : "";

                GUILayout.Label(labelText + stu.LastName + " " + stu.FirstName, bigLabelStyle);
                GUILayout.FlexibleSpace();
            }
            bigLabelStyle.fontSize = 15;

            using (new EditorGUILayout.HorizontalScope("box"))
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Ultimate " + stu.StudentCard.Talent, bigLabelStyle);
                GUILayout.FlexibleSpace();
            }
            GUILayout.Space(15);
        }

        private void StudentCard()
        {
            using (new EditorGUILayout.VerticalScope("Box"))
            {
                stu.StudentCard.Talent = HandyFields.StringField("Talent: ", stu.StudentCard.Talent);
                stu.StudentCard.Height = HandyFields.StringField("Height: ", stu.StudentCard.Height);
                stu.StudentCard.Weight = HandyFields.StringField("Weight: ", stu.StudentCard.Weight);
                stu.StudentCard.Chest = HandyFields.StringField("Chest: ", stu.StudentCard.Chest);
                stu.StudentCard.BloodType = HandyFields.StringField("Blood Type: ", stu.StudentCard.BloodType);
                stu.StudentCard.DateOfBirth = HandyFields.StringField("D.O.B.: ", stu.StudentCard.DateOfBirth);
                stu.StudentCard.Likes = HandyFields.StringField("Likes: ", stu.StudentCard.Likes);
                stu.StudentCard.Dislikes = HandyFields.StringField("Dislikes: ", stu.StudentCard.Dislikes);
                stu.StudentCard.Notes = HandyFields.StringArea("Description: ", stu.StudentCard.Notes);
            }
        }

        private void DefaultSprite()
        {
            HandyFields.Label("Default Sprite");

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(140)))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        stu.DefaultSprite = TextureField(stu.DefaultSprite);
                        if (GUILayout.Button("X", GUILayout.Width(18)))
                        {
                            stu.DefaultSprite = null;
                        }
                    }
                }
            }
        }
        #region Black Expression
        private void BlackExpression() //*
        {
            HandyFields.Label("Black Cutout Material");

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(140)))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        stu.BlackExpression = MaterialField(stu.BlackExpression);
                        if (GUILayout.Button("X", GUILayout.Width(18)))
                        {
                            stu.BlackExpression = null;
                        }
                    }
                }
            }
        }
        #endregion
        /*
        private void TrialMaterial() //*
        {
            HandyFields.Label("Trial Material");

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(140)))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        stu.TrialMaterial = MaterialField(stu.TrialMaterial);
                        if (GUILayout.Button("X", GUILayout.Width(18)))
                        {
                            stu.BlackExpression = null;
                        }
                    }
                }
            }
        }
        */
        
        private void MissingSprites()
        {
            EditorGUILayout.LabelField("Missing Sprite Material and Texture");
            using (new EditorGUILayout.HorizontalScope())
            {
                stu.MissingMat = HandyFields.UnityField(stu.MissingMat,150,20);
                stu.MissingTex = HandyFields.UnityField(stu.MissingTex,150,20);
                if (GUILayout.Button("Fill Missing Sprites"))
                {
                    if (stu.MissingTex == null || stu.MissingMat == null)
                    {
                        Debug.LogError("You must fill the material and texture fields to fill sprite list");
                        return;
                    }
                    else
                    {
                        for (var i = 0; i < stu.Expressions.Count; i++)
                        {
                            var expr = stu.Expressions[i];
                            if (expr.Sprite == null)
                            {
                                expr.Sprite = stu.MissingMat;
                                stu.Sprites[i].Sprite = stu.MissingTex;
                            }
                        }
                    }
                }
                if (GUILayout.Button("Clear Missing Sprites"))
                {
                    for (var i = 0; i < stu.Expressions.Count; i++)
                    {
                        var expr = stu.Expressions[i];
                        if (expr.Sprite == stu.MissingMat)
                        {
                            expr.Sprite = null;
                            stu.Sprites[i].Sprite = null;
                        }
                    }
                }
            }
        }

        private void Sprites()
        {
            var count = 0;
            using (new EditorGUILayout.HorizontalScope())
            {
                for (var i = 0; i < stu.Expressions.Count; i++)
                {
                    var expr = stu.Expressions[i];
                    using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(140)))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            expr.Sprite = MaterialField(expr.Sprite);
                            using (new EditorGUILayout.VerticalScope())
                            {
                                if (GUILayout.Button("X", GUILayout.Width(18)))
                                {
                                    stu.Expressions.Remove(expr);
                                    stu.Sprites.Remove(stu.Sprites[i]);//*
                                }
                                if (i != 0 && GUILayout.Button("^", GUILayout.Width(18)))//*
                                {
                                    Expression ex = expr;
                                    stu.Expressions[i] = stu.Expressions[i - 1];
                                    stu.Expressions[i - 1] = ex;
                                    Unlit x = stu.Sprites[i];
                                    stu.Sprites[i] = stu.Sprites[i - 1];
                                    stu.Sprites[i - 1] = x;
                                }
                                if (stu.MissingMat != null && stu.MissingTex != null && GUILayout.Button("+^", GUILayout.Width(25)))//*
                                {
                                    stu.Expressions.Insert(i, new Expression());
                                    stu.Sprites.Insert(i, new Unlit());
                                    //stu.Expressions[i].Sprite = stu.MissingMat;
                                    //stu.Sprites[i].Sprite = stu.MissingTex;
                                    //stu.Expressions[i].Name = "";
                                    //stu.Sprites[i].Name = "";
                                }
                                if (stu.MissingMat != null && stu.MissingTex != null && GUILayout.Button("+v", GUILayout.Width(25)))//*
                                {
                                    stu.Expressions.Insert(i, new Expression());
                                    stu.Sprites.Insert(i, new Unlit());
                                    Expression ex = stu.Expressions[i];
                                    stu.Expressions[i] = stu.Expressions[i + 1];
                                    stu.Expressions[i + 1] = ex;
                                    Unlit x = stu.Sprites[i];
                                    stu.Sprites[i] = stu.Sprites[i + 1];
                                    stu.Sprites[i + 1] = x;

                                    //stu.Expressions[i + 1].Sprite = stu.MissingMat;
                                    //stu.Sprites[i + 1].Sprite = stu.MissingTex;
                                    //stu.Expressions[i + 1].Name = "";
                                    //stu.Sprites[i + 1].Name = "";
                                }
                                if (i != stu.Expressions.Count - 1 && GUILayout.Button("v", GUILayout.Width(18)))//*
                                {
                                    Expression ex = expr;
                                    stu.Expressions[i] = stu.Expressions[i + 1];
                                    stu.Expressions[i + 1] = ex;
                                    Unlit x = stu.Sprites[i];
                                    stu.Sprites[i] = stu.Sprites[i + 1];
                                    stu.Sprites[i + 1] = x;
                                }
                            }
                        }
                        expr.Name = EditorGUILayout.TextField(expr.Name, GUILayout.Width(120));
                    }

                    count++;
                    if (count > 1)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        count = 0;
                    }
                }

                if (GUILayout.Button("Add \n Sprite", GUILayout.Width(100), GUILayout.Height(40)))
                {
                    stu.Expressions.Add(new Expression());
                    stu.Sprites.Add(null);//*
                }
                
            }
        }
        #region Unlits
        //* Unlits
        private void Unlits()
        {
            var count = 0;
            using (new EditorGUILayout.HorizontalScope())
            {
                for (var i = 0; i < stu.Sprites.Count; i++)
                {
                    var unlit = stu.Sprites[i];
                    var expr = stu.Expressions[i];
                    using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(140)))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            unlit.Sprite = HandyFields.SpriteField(expr.Name, unlit.Sprite);

                            /*if (GUILayout.Button("X", GUILayout.Width(18)))
                            {
                                stu.Expressions.Remove(expr);
                                stu.Sprites.Remove(stu.Sprites[i]);//*
                            }*/
                        }
                        GUILayout.Label(expr.Name, GUILayout.Width(120));
                        unlit.Name = expr.Name;
                    }

                    count++;
                    if (count > 1)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        count = 0;
                    }
                }

                

            }
        }
        #endregion
        private static Texture2D TextureField(Texture2D texture)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    fixedWidth = 70
                };
                return HandyFields.UnityField(texture);
            }
        }

        private static Material MaterialField(Material mat)
        {
            Material result;
            using (new EditorGUILayout.HorizontalScope())
            {
                result = HandyFields.UnityField(mat, height:140);
            }

            if (mat != null)
            {
                var myTexture = AssetPreview.GetAssetPreview(HandyFields.GetMaterialTexture(mat));
                if(myTexture)
                {
                    GUILayout.Label(myTexture);
                }
            }
            return result;
        }

        private static Texture2D TextureFieldLabeledHorizontal(string label, Texture2D texture)
        {
            Texture2D result;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                result = HandyFields.UnityField(texture, 70, 70);
            }
            return result;
        }

        private static void AliasList(List<Alias> aliasList)
        {
            if (GUILayout.Button("Add Alias", GUILayout.Width(120)))
            {
                aliasList.Add(new Alias());
            }
            if (aliasList != null)
            {
                for (int i = 0; i < aliasList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    aliasList[i].Name = HandyFields.StringField("Alias " + i + ": ",aliasList[i].Name);
                    var removed = GUILayout.Button("x", GUILayout.Width(20));
                    EditorGUILayout.EndHorizontal();
                    aliasList[i].Nameplate = TextureFieldLabeledHorizontal("Nameplate:", aliasList[i].Nameplate);
                    aliasList[i].TrialNameplate = TextureFieldLabeledHorizontal("Trial Nameplate:", aliasList[i].TrialNameplate);
                    aliasList[i].TrialPortrait = TextureFieldLabeledHorizontal("Trial Portrait:", aliasList[i].TrialPortrait);
                    if (removed)
                    {
                        aliasList.RemoveAt(i);
                    }
                }
            }
        }
    }
}
#endif