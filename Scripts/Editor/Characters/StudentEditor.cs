using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using DREditor.Utility.Editor;

namespace DREditor.Characters.Editor
{
	[CustomEditor(typeof(Student))]
	public class StudentEditor : UnityEditor.Editor
	{
		private Student stu;

		private void OnEnable() => stu = target as Student;

		public override void OnInspectorGUI()
		{
			HandyFields.Label("Character Editor");

			StudentLabel();
			StudentCard();

			CharacterProperties();

			DefaultSprite();
			CharacterPortrait();

			HandyFields.Label("Sprites");

			Sprites();
			
			EditorUtility.SetDirty(stu);
		}
		
		private void StudentLabel()
		{
			var bigLabelStyle = new GUIStyle
			{
				fontSize = 25,
				fontStyle = FontStyle.Bold
			};

			bigLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

			GUI.backgroundColor = stu.StudentCard.Color;

			using (new EditorGUILayout.HorizontalScope("box"))
			{
				GUILayout.FlexibleSpace();

				var labelText = stu.LastName == "" && stu.FirstName == "" ? "No Name" : "";

				if (stu.Nameplate == null) GUILayout.Label(labelText + stu.LastName + " " + stu.FirstName, bigLabelStyle);
				else GUILayout.Label(stu.Nameplate);

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
			using (new EditorGUILayout.HorizontalScope("Box"))
			{
				if (stu.DefaultSprite != null) GUILayout.Label(stu.DefaultSprite, GUILayout.Width(Screen.width - 328), GUILayout.Height(410));
				using (new EditorGUILayout.VerticalScope())
				{
					if (stu.Headshot != null) GUILayout.Label(stu.Headshot, GUILayout.Height(140));
					stu.LastName = HandyFields.StringField("Last Name: ", stu.LastName);
					stu.FirstName = HandyFields.StringField("First Name: ", stu.FirstName);

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
		}

		private void CharacterProperties()
		{
			using (new EditorGUILayout.VerticalScope("Box"))
			{
				AliasList(stu.Aliases);
			}

			EditorGUILayout.BeginVertical("Box");
			stu.StudentCard.Color = HandyFields.ColorField(stu.StudentCard.Color);
			stu.Nameplate = TextureFieldLabeledHorizontal("Default Nameplate: ", stu.Nameplate, 200, 40);
			stu.Headshot = TextureFieldLabeledHorizontal("Default Headshot: ", stu.Headshot, 300, 150);
			stu.TrialNameplate = TextureFieldLabeledHorizontal("Trial Nameplate: ", stu.TrialNameplate);
			stu.TrialHeight = HandyFields.FloatField("Trial Height: ", stu.TrialHeight);
			EditorGUILayout.EndVertical();
		}

		private void DefaultSprite()
		{
			HandyFields.Label("Splash Art");

			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(140)))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						stu.DefaultSprite = TextureField(stu.DefaultSprite);
						if (GUILayout.Button(new GUIContent("x", "Remove Splash Art"), GUILayout.Width(18)))
						{
							stu.DefaultSprite = null;
						}
					}
				}
			}
		}

		private void CharacterPortrait()
		{
			HandyFields.Label("Character Portrait");

			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(140)))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						stu.TrialPortrait = TextureField(stu.TrialPortrait);
						if (GUILayout.Button("X", GUILayout.Width(18)))
						{
							stu.TrialPortrait = null;
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
					using (new EditorGUILayout.HorizontalScope("Box", GUILayout.Width(140)))
					{
						using (new EditorGUILayout.VerticalScope())
						{
							if (i == 0) EditorGUILayout.LabelField("Default Sprite", GUILayout.Width(120));
							else EditorGUILayout.LabelField($"{i} - {(expr.Name == "" ? "Undefined" : expr.Name)}", GUILayout.Width(120));

							if (expr.Sprite != null) GUILayout.Label(HandyFields.GetMaterialTexture(expr.Sprite), GUILayout.Width(120), GUILayout.Height(120));
							expr.Sprite = MaterialField(expr.Sprite);
							if (expr.Name == "") expr.Name = expr.Sprite.name;
							expr.Name = EditorGUILayout.TextField(expr.Name, GUILayout.Width(120));
						}

						if (GUILayout.Button("-", GUILayout.Width(18)))
						{
							stu.Expressions.Remove(expr);
						}
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
				}
			}
		}
		
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
				result = HandyFields.UnityField(mat, 120, 20);
			}

			if (mat != null)
			{
				var myTexture = AssetPreview.GetAssetPreview(HandyFields.GetMaterialTexture(mat));
			}
			return result;
		}

		private static Texture2D TextureFieldLabeledHorizontal(string label, Texture2D texture, int width = 70, int height = 70)
		{
			Texture2D result;
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField(label);
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				result = HandyFields.UnityField(texture, width, height);
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
					aliasList[i].Name = HandyFields.StringField("Alias " + i + ": ", aliasList[i].Name, 180);
					var removed = GUILayout.Button("x", GUILayout.Width(20));
					EditorGUILayout.EndHorizontal();
					aliasList[i].Nameplate = TextureFieldLabeledHorizontal("Nameplate:", aliasList[i].Nameplate);
					if (removed)
					{
						aliasList.RemoveAt(i);
					}
				}
			}
		}
	}
}
