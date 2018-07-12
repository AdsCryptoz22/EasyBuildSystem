using EasyBuildSystem.Codes.Editor.Extensions;
using EasyBuildSystem.Runtimes;
using EasyBuildSystem.Runtimes.Extensions;
using EasyBuildSystem.Runtimes.Internal.Addons;
using EasyBuildSystem.Runtimes.Internal.Managers;
using EasyBuildSystem.Runtimes.Internal.Part;
using EasyBuildSystem.Runtimes.Internal.Socket;
using EasyBuildSystem.Runtimes.Internal.Socket.Data;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Codes.Editor.Internal.Part
{
    [CustomEditor(typeof(PartBehaviour))]
    public class PartBehaviourInspector : UnityEditor.Editor
    {
        #region Public Fields

        public static Rect BoundsEditingWindowRect = new Rect(50, 50, 300, 145);

        #endregion Public Fields

        #region Private Fields

        private PartBehaviour Target;

        private string BoundsValue;

        private Bounds CurrentBounds;

        private GameObject CurrentPreview;

        private int PreviewIndex;

        private GameObject AppearanceToAdd;

        private bool[] AppearancesFolds = new bool[999];

        private List<UnityEditor.Editor> AppearancesPreview = new List<UnityEditor.Editor>();

        private List<GameObject> Previews = new List<GameObject>();

        #region Foldout(s)

        private static bool AllIsOpen = false;

        private static bool BasePartFoldout;

        private static bool PreviewPartFoldout;

        private static bool AppearanceFoldout;

        private static bool MeshFoldout;

        private static bool PhysicsFoldout;

        private static bool TerrainFoldout;

        private static bool UtilitiesFoldout;

        private static bool AddonsFoldout;

        private static bool ShowPreviewEditor;

        private static bool ShowPartBoundsEditor;

        private static bool ShowTerrainBoundsEditor;

        private static bool Help;

        #endregion Foldout(s)

        #endregion Private Fields

        #region Public Methods

        public void OnSceneGUI()
        {
            if (SceneView.lastActiveSceneView.camera == null)
                return;

            if (ShowPreviewEditor)
            {
                if (CurrentPreview == null)
                {
                    int Index = 0;

                    foreach (SocketBehaviour ChildSocket in Target.transform.GetComponentsInChildren<SocketBehaviour>())
                    {
                        if (ChildSocket != null)
                        {
                            if (ChildSocket.PartOffsets.Count > PreviewIndex)
                            {
                                PartOffset Offset = ChildSocket.PartOffsets[PreviewIndex];

                                CurrentPreview = Instantiate(Offset.Part.gameObject);

                                foreach (SocketBehaviour Socket in CurrentPreview.GetComponentsInChildren<SocketBehaviour>())
                                    DestroyImmediate(Socket);

                                CurrentPreview.transform.position = ChildSocket.transform.TransformPoint(Offset.Position);

                                CurrentPreview.transform.rotation = ChildSocket.transform.rotation * Quaternion.Euler(Offset.Rotation);

                                CurrentPreview.transform.localScale = Offset.UseCustomScale ? Offset.Scale : ChildSocket.transform.localScale;

                                CurrentPreview.hideFlags = HideFlags.HideInHierarchy;

                                Material PreviewMaterial = new Material(Shader.Find(Constants.DIFFUSE_SHADER_NAME));

                                CurrentPreview.ChangeAllMaterialsInChildren(CurrentPreview.GetComponentsInChildren<MeshRenderer>(), PreviewMaterial);

                                Previews.Add(CurrentPreview);

                                Index++;
                            }
                        }
                    }

                    SceneHelper.Focus(target, DrawCameraMode.Textured);
                }
            }

            if (PreviewPartFoldout)
            {
                if (Target.UseGroundUpper)
                {
                    Handles.color = Color.green;

                    Handles.DrawLine(Target.transform.position, Target.transform.position + Vector3.down * Target.GroundUpperHeight);
                }
            }

            if (!serializedObject.FindProperty("AdvancedFeatures").boolValue)
                return;

            if (MeshFoldout || PhysicsFoldout || TerrainFoldout)
            {
                if (MeshFoldout)
                {
                    Handles.color = Color.green;
                    CurrentBounds = Target.MeshBounds;

                    Handles.matrix = Matrix4x4.TRS(Target.transform.position, Target.transform.rotation, Target.transform.localScale);
                    Handles.DrawWireCube(CurrentBounds.center, CurrentBounds.size);
                }

                if (PhysicsFoldout && Target.UseConditionalPhysics)
                {
                    Handles.color = Color.cyan;

                    if (Target.CustomDetections.Length != 0)
                    {
                        for (int i = 0; i < Target.CustomDetections.Length; i++)
                        {
                            Handles.matrix = Matrix4x4.TRS(Target.transform.position, Target.transform.rotation, Target.transform.localScale);

                            Handles.DrawWireCube(Target.CustomDetections[i].Position, Target.CustomDetections[i].Size * 2);
                        }
                    }
                }

                if (TerrainFoldout && Target.UseTerrainPrevention)
                {
                    Handles.color = Color.yellow;
                    CurrentBounds = Target.TerrainBounds;

                    Handles.matrix = Matrix4x4.TRS(Target.transform.position, Target.transform.rotation, Target.transform.localScale);
                    Handles.DrawWireCube(CurrentBounds.center, CurrentBounds.size);
                }
            }

            if (ShowPartBoundsEditor || ShowTerrainBoundsEditor)
            {
                Handles.color = Color.white;

                Handles.matrix = Matrix4x4.TRS(Target.transform.position, Target.transform.rotation, Target.transform.localScale);

                Handles.DrawWireCube(CurrentBounds.center, CurrentBounds.size);

                Handles.BeginGUI();

                BoundsEditingWindowRect = GUI.Window(0, BoundsEditingWindowRect, OnBoundsEditingWindow, "Easy Build System - Bounds Editing");

                Handles.EndGUI();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(10);

            #region Inspector

            GUILayout.BeginVertical("Easy Build System - Part Behaviour", "window", GUILayout.Height(1));

            GUILayout.BeginVertical("box");

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.BeginHorizontal();

            GUILayout.Label("Part Behaviour Settings", EditorStyles.boldLabel);

            #region Part Behaviour Settings

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(AllIsOpen ? "Fold In" : "Fold Out", GUILayout.Width(80)))
            {
                BasePartFoldout = !BasePartFoldout;
                PreviewPartFoldout = !PreviewPartFoldout;
                AppearanceFoldout = !AppearanceFoldout;
                MeshFoldout = !MeshFoldout;
                PhysicsFoldout = !PhysicsFoldout;
                TerrainFoldout = !TerrainFoldout;
                UtilitiesFoldout = !UtilitiesFoldout;
                AddonsFoldout = !AddonsFoldout;
                AllIsOpen = !AllIsOpen;
            }

            if (GUILayout.Button(Help ? "Hide Help" : "Show Help", GUILayout.Width(100)))
                Help = !Help;

            GUI.color = Color.white;

            GUILayout.EndHorizontal();

            if (Help)
            {
                EditorGUILayout.HelpBox("This component contains all the settings used by the Builder Behaviour component attached on your Main Camera.\n" +
                    "It is important to configure this component correctly in order to have an optimal behavior during the runtime.", MessageType.Info);

                EditorGUILayout.HelpBox("The component uses basic features, which means that physics, terrain detection and appearance are disabled.\n" +
                    "The mesh bounds is automatically generated on scene startup.\n" +
                    "It is recommended to use the advanced features if you've an complex part.", MessageType.Info);

                EditorGUILayout.HelpBox("Important : Every change to import on the Part Behaviour do not forget to apply the changes so that it is updated.\n" +
                    "All the socket(s) contained in the part as well as all the object(s) will be as well.", MessageType.Warning);

                GUI.color = MainEditor.GetEditorColor;

                if (GUILayout.Button("Open Documentation Link"))
                    Application.OpenURL(Constants.DOCS_LINK);

                GUI.color = Color.white;
            }

            GUI.enabled = false;

            EditorGUILayout.ObjectField("Script", target, typeof(PartBehaviour), true);

            GUI.enabled = true;

            GUI.color = Color.white;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("AdvancedFeatures"), new GUIContent("Use Advanced Feature(s) :",
                "This allows to use the advanced features on this part.\n" +
                "It is important to know the system before using the advanced feature(s)."));

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            BasePartFoldout = EditorGUILayout.Foldout(BasePartFoldout, "Base Section Settings");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            GUILayout.BeginVertical();

            if (BasePartFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Id"), new GUIContent("Part Id :", "This allows to identified this part.\nCareful not to put an identical id on another part."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Name"), new GUIContent("Part Name :", "This allows to named this part."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Type"), new GUIContent("Part Type :", "This allows to define the type of this part.\nIf an type is not present in the list, choose the type (None), this will not importance."));

                GUILayout.BeginHorizontal();

                GUILayout.Space(13);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("OccupancyParts"), new GUIContent("Part Occupancy :", "This allows to define occupancy of some part(s) when will placed."), true);

                GUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AvoidClipping"), new GUIContent("Avoid Clipping :", "This allows to prevents the preview from entering other objects placed in the scene."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AvoidClippingOnSocket"), new GUIContent("Avoid Clipping On Socket :", "This allows to prevents the preview from entering other objects placed in the scene (Only on socket)."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PhysicsOnlyStablePlacement"), new GUIContent("Can Place Only On Stable Support :", "(Only if use Conditional Physics) This allows to allows the placement only if the placement surface is stable."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RequireSockets"), new GUIContent("Can Place Only On Socket(s) :", "This allows to define if this part has need a socket for be placed."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AvoidAnchoredOnSocket"), new GUIContent("Avoid Anchored On Socket(s) :", "This allows to avoid the clipping on the socket(s) who contains this part in offsets list."));
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            #endregion Part Behaviour Settings

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Part Preview Settings", EditorStyles.boldLabel);

            #region Part Preview Settings

            GUI.color = Color.white;

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            PreviewPartFoldout = EditorGUILayout.Foldout(PreviewPartFoldout, "Preview Section Settings");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            GUILayout.BeginVertical();

            if (PreviewPartFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseGroundUpper"), new GUIContent("Use Ground Upper :", "This allows to to raise from ground the preview on the Y axis."));

                if (serializedObject.FindProperty("UseGroundUpper").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("GroundUpperHeight"), new GUIContent("Ground Upper Height Limit :", "This allows to define the maximum limit not to exceed on the Y axis."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewOffset"), new GUIContent("Position Offset :", "This allows to define the preview offset position."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewUseColorLerpTime"), new GUIContent("Use Color Lerp Time :", "This allows to lerp the preview color when the placement is possible or no."));

                if (serializedObject.FindProperty("PreviewUseColorLerpTime").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewColorLerpTime"), new GUIContent("Part Preview Color Lerp Time :", "This allows to define the transition speed on the preview color."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RotateOnSockets"), new GUIContent("Can Rotate On Socket(s) :", "This allows to define if the preview can be rotated on socket."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RotateAccordingSlope"), new GUIContent("Rotate According Slope :", "This allows to rotate the preview according to slope terrain angle."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RotationAxis"), new GUIContent("Rotate Axis :", "This allows to define on what axis the preview will be rotated."));

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewDisableObjects"), new GUIContent("Disable Object(s) In Preview State :", "This allows to disable some object(s) when the part is in preview state."), true);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewDisableBehaviours"), new GUIContent("Disable Mono Behaviour(s) In Preview State :", "This allows to disable some monobehaviour(s) when the part is in preview/queue/remove/edit state."), true);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewDisableColliders"), new GUIContent("Disable Collider(s) In Preview State :", "This allows to disable some collider(s) when the part is in preview state."), true);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            #endregion Part Preview Settings

            if (serializedObject.FindProperty("AdvancedFeatures").boolValue)
            {
                GUI.color = MainEditor.GetEditorColor;

                GUILayout.Label("Part Appearances Settings", EditorStyles.boldLabel);

                #region Part Appearance Settings

                GUI.color = Color.white;

                GUILayout.BeginHorizontal();

                GUILayout.Space(13);

                EditorGUI.BeginChangeCheck();

                AppearanceFoldout = EditorGUILayout.Foldout(AppearanceFoldout, "Appearances Section Settings");

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(target);
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Space(13);

                GUILayout.BeginVertical();

                if (AppearanceFoldout)
                {
                    GUILayout.BeginVertical();

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("UseAppearances"), new GUIContent("Use Appearance(s) :", "This allows to define if many appearance(s) can will be displayed."));

                    if (serializedObject.FindProperty("UseAppearances").boolValue)
                    {
                        bool flag = false;

                        if (AppearancesFolds == null)
                            AppearancesFolds = new bool[Target.Appearances.Count];

                        for (int i = 0; i < Target.Appearances.Count; i++)
                        {
                            if (Target.Appearances[i] == null)
                                flag = true;
                        }

                        if (flag)
                            Target.Appearances.Clear();

                        if (Target.Appearances.Count == 0)
                        {
                            GUILayout.BeginHorizontal("box");

                            GUI.color = new Color(1.5f, 1.5f, 0f);

                            GUILayout.Label("The list does not contains of appearance(s).");

                            GUI.color = Color.white;

                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            int Index = 0;

                            EditorGUILayout.BeginVertical("box");

                            GUILayout.Space(2);

                            foreach (GameObject Appearance in Target.Appearances)
                            {
                                if (Appearance == null)
                                    return;

                                GUILayout.BeginHorizontal();

                                GUILayout.Space(13);

                                EditorGUI.BeginChangeCheck();

                                AppearancesFolds[Index] = EditorGUILayout.Foldout(AppearancesFolds[Index], string.Format("[{0}] ", Index) + Appearance.name);

                                if (EditorGUI.EndChangeCheck())
                                {
                                    if (AppearancesFolds[Index] == true)
                                    {
                                        for (int i = 0; i < AppearancesFolds.Length; i++)
                                        {
                                            if (i != Index)
                                            {
                                                AppearancesFolds[i] = false;
                                            }
                                        }

                                        for (int x = 0; x < Target.Appearances.Count; x++)
                                        {
                                            if (x == Index)
                                                Target.Appearances[x].SetActive(true);
                                            else
                                                Target.Appearances[x].SetActive(false);
                                        }
                                    }
                                    else
                                    {
                                        for (int x = 0; x < Target.Appearances.Count; x++)
                                        {
                                            if (x == Target.AppearanceIndex)
                                                Target.Appearances[x].SetActive(true);
                                            else
                                                Target.Appearances[x].SetActive(false);
                                        }
                                    }

                                    SceneHelper.Focus(Appearance, DrawCameraMode.Textured, false);

                                    AppearancesPreview.Clear();
                                }

                                GUI.color = MainEditor.GetEditorColor;

                                if (Target.AppearanceIndex == Index)
                                    GUI.enabled = false;

                                if (GUILayout.Button("Define As Default Appearance", GUILayout.Width(190)))
                                {
                                    for (int i = 0; i < AppearancesFolds.Length; i++)
                                        AppearancesFolds[i] = false;

                                    Target.ChangeAppearance(Index);

                                    for (int x = 0; x < Target.Appearances.Count; x++)
                                    {
                                        if (x == Target.AppearanceIndex)
                                            Target.Appearances[x].SetActive(true);
                                        else
                                            Target.Appearances[x].SetActive(false);
                                    }

                                    Repaint();

                                    EditorUtility.SetDirty(target);
                                }

                                GUI.enabled = true;

                                GUI.color = Color.white;

                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();

                                GUILayout.Space(5);

                                if (AppearancesFolds[Index])
                                {
                                    GUILayout.BeginHorizontal();

                                    GUILayout.BeginVertical("box");

                                    GUILayout.BeginHorizontal("box");

                                    GUILayout.Label("(Index:" + Index + ") " + Appearance.name);

                                    GUILayout.EndHorizontal();

                                    if (Appearance != null)
                                    {
                                        UnityEditor.Editor PreviewEditor = null;

                                        if (AppearancesPreview.Count > Index)
                                            PreviewEditor = AppearancesPreview[Index];

                                        if (PreviewEditor == null)
                                        {
                                            PreviewEditor = CreateEditor(Appearance);

                                            AppearancesPreview.Add(PreviewEditor);

                                            PreviewEditor.OnPreviewGUI(GUILayoutUtility.GetRect(128, 128), EditorStyles.textArea);
                                        }
                                        else
                                        {
                                            PreviewEditor.OnPreviewGUI(GUILayoutUtility.GetRect(128, 128), EditorStyles.textArea);
                                        }
                                    }

                                    GUILayout.FlexibleSpace();

                                    GUILayout.EndVertical();

                                    GUILayout.BeginVertical("box");

                                    GUI.color = MainEditor.GetEditorColor;

                                    if (GUILayout.Button("Up Index", GUILayout.Width(150)))
                                    {
                                        try
                                        {
                                            ListExtension.Move<GameObject>(Target.Appearances, Target.Appearances.IndexOf(Appearance), ListExtension.MoveDirection.Up);

                                            AppearancesFolds[Index] = false;

                                            AppearancesFolds[Target.Appearances.IndexOf(Appearance)] = true;

                                            Repaint();

                                            EditorUtility.SetDirty(target);
                                        }
                                        catch
                                        {
                                        }

                                        Repaint();

                                        EditorUtility.SetDirty(target);

                                        AppearancesPreview.Clear();

                                        break;
                                    }

                                    if (GUILayout.Button("Down Index", GUILayout.Width(150)))
                                    {
                                        try
                                        {
                                            int NewIndex = Target.Appearances.IndexOf(Appearance);

                                            ListExtension.Move<GameObject>(Target.Appearances, NewIndex, ListExtension.MoveDirection.Down);

                                            AppearancesFolds[Index] = false;

                                            AppearancesFolds[Target.Appearances.IndexOf(Appearance)] = true;

                                            Repaint();

                                            EditorUtility.SetDirty(target);
                                        }
                                        catch
                                        {
                                        }

                                        Repaint();

                                        EditorUtility.SetDirty(target);

                                        AppearancesPreview.Clear();

                                        break;
                                    }

                                    GUI.color = Color.white;

                                    GUI.color = new Color(1.5f, 0, 0);

                                    if (GUILayout.Button("Remove", GUILayout.Width(150)))
                                    {
                                        Target.Appearances.Remove(Appearance);

                                        Repaint();

                                        EditorUtility.SetDirty(target);

                                        AppearancesPreview.Clear();

                                        break;
                                    }

                                    GUI.color = Color.white;

                                    GUILayout.EndVertical();

                                    GUILayout.EndHorizontal();
                                }

                                GUILayout.EndHorizontal();

                                Index++;
                            }

                            EditorGUILayout.EndVertical();
                        }

                        try
                        {
                            GUILayout.BeginVertical("box");

                            GUILayout.BeginVertical();

                            GUILayout.BeginHorizontal();

                            AppearanceToAdd = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Appearance :", "This allows to add a apperance in the list."), AppearanceToAdd, typeof(GameObject), true);

                            GUILayout.EndHorizontal();

                            GUI.enabled = AppearanceToAdd != null;

                            GUI.color = MainEditor.GetEditorColor;

                            if (GUILayout.Button("Add Appearance"))
                            {
                                if (AppearanceToAdd == null)
                                {
                                    Debug.LogError("<b><color=cyan>[Easy Build System]</color></b> : Empty field.");
                                    return;
                                }

                                if (Target.Appearances.Contains(AppearanceToAdd) == false)
                                {
                                    Target.Appearances.Add(AppearanceToAdd);

                                    for (int x = 0; x < Target.Appearances.Count; x++)
                                    {
                                        if (x == Target.AppearanceIndex)
                                            Target.Appearances[x].SetActive(true);
                                        else
                                            Target.Appearances[x].SetActive(false);
                                    }

                                    AppearanceToAdd = null;

                                    EditorUtility.SetDirty(target);

                                    Repaint();
                                }
                                else
                                    Debug.LogError("<b><color=cyan>[Easy Build System]</color></b> : This part already exists in the collection.");
                            }

                            GUI.color = Color.white;

                            GUI.enabled = Target.Appearances.Count > 0;

                            GUI.color = new Color(1.5f, 1.5f, 0);

                            if (GUILayout.Button("Clear All Appearance(s) List"))
                            {
                                if (EditorUtility.DisplayDialog("Easy Build System - Information", "Do you want remove all the Appearance(s) from the collection ?", "Ok", "Cancel"))
                                {
                                    Target.Appearances.Clear();

                                    AppearancesPreview.Clear();

                                    Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : The collection has been clear !.");
                                }
                            }

                            GUI.enabled = true;

                            GUI.color = Color.white;

                            GUILayout.EndVertical();

                            GUILayout.Space(3);

                            GUILayout.EndVertical();
                        }
                        catch { }
                    }

                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                #endregion Part Appearance Settings

                GUI.color = MainEditor.GetEditorColor;

                GUILayout.Label("Part Mesh Settings", EditorStyles.boldLabel);

                #region Part Mesh Settings

                GUI.color = Color.white;

                GUILayout.BeginHorizontal();

                GUILayout.Space(13);

                MeshFoldout = EditorGUILayout.Foldout(MeshFoldout, "Mesh Section Settings");

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Space(13);

                GUILayout.BeginVertical();

                if (MeshFoldout)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("MeshBounds"), new GUIContent("Mesh Bounds :", "This allows to define the bounds part of the child mesh."));

                    GUILayout.BeginHorizontal();

                    if (!ShowPartBoundsEditor)
                    {
                        GUI.color = MainEditor.GetEditorColor;

                        if (GUILayout.Button("Edit Mesh Part Bounds"))
                        {
                            SceneHelper.Focus(target);

                            BoundsValue = "MeshBounds";

                            CurrentBounds = serializedObject.FindProperty(BoundsValue).boundsValue;

                            ShowPartBoundsEditor = true;
                            ShowTerrainBoundsEditor = false;
                        }

                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUI.color = new Color(0f, 1.5f, 1.5f);

                        if (GUILayout.Button("Close"))
                        {
                            SceneHelper.UnFocus();

                            ShowPartBoundsEditor = false;

                            Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : The bounds changes were saved.");
                        }

                        GUI.color = Color.white;
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                #endregion Part Mesh Settings

                GUI.color = MainEditor.GetEditorColor;

                GUILayout.Label("Part Physics Settings", EditorStyles.boldLabel);

                #region Part Physics Settings

                GUI.color = Color.white;

                GUILayout.BeginHorizontal();

                GUILayout.Space(13);

                PhysicsFoldout = EditorGUILayout.Foldout(PhysicsFoldout, "Physics Section Settings");

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Space(13);

                GUILayout.BeginVertical();

                if (PhysicsFoldout)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("UseConditionalPhysics"), new GUIContent("Use Conditional Physics :", "This allows to use the physics of system."));

                    if (serializedObject.FindProperty("UseConditionalPhysics").boolValue)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PhysicsLayers"), new GUIContent("Physics Layer(s) :", "This allows to define the layer(s) that the physics raycast take into consideration."));

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PhysicsLifeTime"), new GUIContent("Affected Life Time :", "This allows to define the destroy time after that the part has been affected by physics."));

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PhysicsConvexOnAffected"), new GUIContent("Affected To Convex Collider(s) :", "This allows to convex all the mesh colliders in children when the part is affected by the physics."));

                        GUILayout.BeginHorizontal();

                        GUILayout.Space(13);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PhysicsIgnoreTags"), new GUIContent("Physics Ignore Collision(s) By Tag :", "This allows to define objects with a tag where the collision will be ignored."), true);

                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();

                        GUILayout.Space(13);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("CustomDetections"), new GUIContent("Physics Custom Detection(s) :", "This allows the placement only if the placement surface is stable."), true);

                        GUILayout.EndHorizontal();

                        GUI.color = MainEditor.GetEditorColor;

                        if (Target.UseConditionalPhysics)
                        {
                            if (GUILayout.Button("Check Current Support Stability"))
                                Debug.Log("Stability : " + (Target.CheckStability() ? "<b><color=#00ff00>Stable</color></b>" : "<b><color=red>Unstable</color></b>"));
                        }

                        GUI.color = Color.white;
                    }
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                #endregion Part Physics Settings

                GUI.color = MainEditor.GetEditorColor;

                GUILayout.Label("Part Terrain Settings", EditorStyles.boldLabel);

                #region Part Terrain Settings

                GUI.color = Color.white;

                GUILayout.BeginHorizontal();

                GUILayout.Space(13);

                TerrainFoldout = EditorGUILayout.Foldout(TerrainFoldout, "Terrain Section Settings");

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Space(13);

                GUILayout.BeginVertical();

                if (TerrainFoldout)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("UseTerrainPrevention"), new GUIContent("Use Terrain Detection :", "This allows to check if this part does not enter in the terrain elements (excepted details)."));

                    if (serializedObject.FindProperty("UseTerrainPrevention").boolValue)
                    {
                        if (ShowTerrainBoundsEditor)
                            EditorGUILayout.HelpBox("Made sure to leave an offset on the center (Y) to prevent your bounds from entering the terrain.\nOtherwise the placement will be refused because the bounds will hitting constantly the terrain.", MessageType.Warning);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("TerrainBounds"), new GUIContent("Part Terrain Bounds :", "This allows to define the terrain bounds of the part, the placement will be denied when the bounds entering the terrain."));

                        GUILayout.BeginHorizontal();

                        if (!ShowTerrainBoundsEditor)
                        {
                            GUI.color = MainEditor.GetEditorColor;

                            if (GUILayout.Button("Edit Terrain Part Bounds"))
                            {
                                SceneHelper.Focus(target);

                                BoundsValue = "TerrainBounds";

                                CurrentBounds = serializedObject.FindProperty(BoundsValue).boundsValue;

                                ShowPartBoundsEditor = false;
                                ShowTerrainBoundsEditor = true;
                            }

                            GUI.color = Color.white;
                        }
                        else
                        {
                            GUI.color = new Color(0f, 1.5f, 1.5f);

                            if (GUILayout.Button("Close"))
                            {
                                SceneHelper.UnFocus();

                                ShowTerrainBoundsEditor = false;

                                Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : The bounds changes were saved.");
                            }

                            GUI.color = Color.white;
                        }

                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                #endregion Part Terrain Settings
            }

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Part Utilities Settings", EditorStyles.boldLabel);

            #region Part Utilities Settings

            GUI.color = Color.white;

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            UtilitiesFoldout = EditorGUILayout.Foldout(UtilitiesFoldout, "Utilities Section Settings");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            GUILayout.BeginVertical();

            if (UtilitiesFoldout)
            {
                GUI.color = MainEditor.GetEditorColor;

                bool EmptyOffset = false;

                for (int i = 0; i < Target.transform.GetComponentsInChildren<SocketBehaviour>().Length; i++)
                    if (!EmptyOffset)
                        EmptyOffset = Target.transform.GetComponentsInChildren<SocketBehaviour>()[i].PartOffsets.Count > 0;

                GUI.enabled = (Target.transform.GetComponentsInChildren<SocketBehaviour>().Length != 0) && EmptyOffset;

                if (!ShowPreviewEditor)
                {
                    if (GUILayout.Button("Preview Offset Part(s)"))
                    {
                        ShowPreviewEditor = true;
                    }
                }
                else
                {
                    GUI.enabled = true;

                    GUILayout.BeginHorizontal();

                    EditorGUI.BeginChangeCheck();

                    if (GUILayout.Button("<", GUILayout.Width(130)))
                    {
                        if (PreviewIndex != 0)
                        {
                            PreviewIndex--;
                        }
                    }

                    GUILayout.FlexibleSpace();

                    GUI.color = Color.white;

                    try
                    {
                        if (Previews != null)
                            if (Previews[0].GetComponent<PartBehaviour>() != null)
                                GUILayout.Label("Offset Part : " + Previews[0].GetComponent<PartBehaviour>().Name);
                    }
                    catch { }

                    GUI.color = MainEditor.GetEditorColor;

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(">", GUILayout.Width(130)))
                    {
                        if (PreviewIndex < Target.transform.GetComponentsInChildren<SocketBehaviour>()[0].PartOffsets.Count - 1)
                        {
                            PreviewIndex++;
                        }
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        ClearPreviews();
                    }

                    GUILayout.EndHorizontal();

                    GUI.color = new Color(1.5f, 1.5f, 0f);

                    if (GUILayout.Button("Cancel Preview"))
                    {
                        ShowPreviewEditor = false;

                        ClearPreviews();
                    }
                }

                GUI.enabled = true;

                GUI.color = MainEditor.GetEditorColor;

                if (GUILayout.Button("Create New Socket Point"))
                {
                    MainEditor.CreateNewSocket();
                }

                if (GUILayout.Button("Add This To Parts Collection"))
                {
                    BuildManager Manager = FindObjectOfType<BuildManager>();

                    if (Manager == null)
                    {
                        Debug.LogWarning("<b><color=cyan>[Easy Build System]</color></b> : Can't add this because the Build Manager could not be found.");

                        return;
                    }

                    if (Manager.PartsCollection == null)
                    {
                        Debug.LogWarning("<b><color=cyan>[Easy Build System]</color></b> : Can't add this because the Build Manager has not of Parts Collection.");

                        return;
                    }

#if UNITY_2018_1
                    GameObject Prefab = PrefabUtility.GetCorrespondingObjectFromSource(Target.gameObject) as GameObject;
#else
                    GameObject Prefab = PrefabUtility.FindPrefabRoot(Target.gameObject) as GameObject;
#endif
                    if (Prefab == null)
                    {
                        Prefab = Target.gameObject;

                        if (Prefab == null)
                        {
                            Debug.LogWarning("<b><color=cyan>[Easy Build System]</color></b> : Can't found the root prefab.");

                            return;
                        }
                    }

                    Manager.PartsCollection.Parts.Add(Prefab.GetComponent<PartBehaviour>());

                    AssetDatabase.Refresh();

                    Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : The parts <b>" + Target.Name + "</b> has been added to collection.");
                }

                GUI.color = Color.white;
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            #endregion Part Utilities Settings

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Part Add-Ons Settings", EditorStyles.boldLabel);

            #region Part Add-Ons Settings

            GUI.color = Color.white;

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            AddonsFoldout = EditorGUILayout.Foldout(AddonsFoldout, "Add-Ons Section Settings");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            GUILayout.BeginVertical();

            if (AddonsFoldout)
            {
                MainEditor.DrawAddons(Target, AddOnTarget.PartBehaviour);
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            #endregion Part Add-Ons Settings

            GUILayout.EndVertical();

            GUILayout.EndVertical();

            #endregion Inspector

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(10);
        }

        #endregion Public Methods

        #region Private Methods

        private void OnEnable()
        {
            Target = (PartBehaviour)target;

            MainEditor.LoadAddons(Target, AddOnTarget.PartBehaviour);
        }

        private void OnDisable()
        {
            SceneHelper.UnFocus();

            ShowPreviewEditor = false;

            ClearPreviews();
        }

        private void ClearPreviews()
        {
            foreach (GameObject Preview in Previews)
                DestroyImmediate(Preview);

            Previews.Clear();

            Previews = new List<GameObject>();
        }

        private void OnBoundsEditingWindow(int windowId)
        {
            GUILayout.BeginVertical("box");

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Edit Bounds Settings", EditorStyles.boldLabel);

            GUI.color = Color.white;

            EditorGUI.BeginChangeCheck();

            CurrentBounds = EditorGUILayout.BoundsField("Bounds (" + BoundsValue + ") : ", CurrentBounds);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.FindProperty(BoundsValue).boundsValue = CurrentBounds;

                serializedObject.ApplyModifiedProperties();
            }

            GUI.color = MainEditor.GetEditorColor;

            if (GUILayout.Button("Generate Bounds"))
            {
                serializedObject.FindProperty(BoundsValue).boundsValue = Target.gameObject.GetChildsBounds();

                Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : Bounds generated " + Target.gameObject.GetChildsBounds().size.ToString() + ".");

                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Close"))
            {
                ShowPartBoundsEditor = false;
                ShowTerrainBoundsEditor = false;

                SceneHelper.UnFocus();

                Repaint();

                Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : The bounds changes were saved.");
            }

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
        }

        #endregion Private Methods
    }
}