using UnityEngine;
using UnityEditor;
using InventoryEngine.Extra;

namespace InventoryEditor
{
    [CustomEditor(typeof(Inspect))]
    public class InspectEditor : Editor
    {
        Inspect Target;
        bool EditPosition = false, Interp = false, Layer;
        Vector3 OldPosition;

        private void OnEnable()
        {
            Target = target as Inspect;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(15);
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label("General");
            GUIContent Content = new GUIContent("Object", "Prefabricated object to be inspected by the system and rendered");
            Target.Object = (GameObject)EditorGUILayout.ObjectField(Content, Target.Object, typeof(GameObject), true);
            Content = new GUIContent("Camera", "Prefabricated camera responsible of rendering the whole object");
            Target.Camera = (GameObject)EditorGUILayout.ObjectField(Content, Target.Camera, typeof(GameObject), true);

            //Revisar inspector bug al jugar (variables reinician)
            EditorGUILayout.BeginHorizontal();
            Content = new GUIContent("Layer", "[Enable/Disable] custom layer set.");
            Target.Layer = EditorGUILayout.ToggleLeft(Content, Target.Layer, GUILayout.MaxWidth(50));
            if (Target.Layer)
            {
                Target.LayerIndex = (Target.LayerIndex < 0) ? 0 : Target.LayerIndex;
                Target.LayerIndex = EditorGUILayout.LayerField(Target.LayerIndex);
            }
            else { Target.LayerIndex = -1; }
            EditorGUILayout.EndHorizontal();

            if (!Target.Object || !Target.Camera)
                EditorGUILayout.HelpBox("<Object or Camera> are null", MessageType.Warning);

            //Editor StartPosition Handler
            EditorGUILayout.BeginVertical("Box");
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Edit Position"))
            {
                EditPosition = !EditPosition;
                OldPosition = Target.StartPosition;
            }
            GUILayout.Label("Start Position");
            if (GUILayout.Button("Last Position"))
                Target.StartPosition = OldPosition;
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3);
            GUI.enabled = EditPosition;
            Target.StartPosition = EditorGUILayout.Vector3Field("", Target.StartPosition);
            if (EditPosition)
            {
                EditorGUILayout.HelpBox("Click to confirm actual position on scene view.", MessageType.Info);
            }
            GUI.enabled = true;
            GUI.skin.label.alignment = TextAnchor.LowerLeft;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            EditorGUILayout.EndVertical();
            //Editor StartPosition HandlerEnd
            Content = new GUIContent("Sensibility", "Rotate sensiblity multiplier");
            Target.Sensibility = EditorGUILayout.Slider(Content, Target.Sensibility, 0.001f, 10.0f);
            Content = new GUIContent("Scroll Sensibility", "Zoom sensibility multiplier");
            Target.ScrollSensibility = EditorGUILayout.Slider(Content, Target.ScrollSensibility, 0.001f, 10.0f);

            Target.ZoomMin = EditorGUILayout.FloatField("Zoom Min", Target.ZoomMin);
            Target.ZoomMax = EditorGUILayout.FloatField("Zoom Max", Target.ZoomMax);

            Content = new GUIContent("Raycast", "[Enable/Disable] Camera raycast to detect object bounds.");
            Target.Raycast = EditorGUILayout.Toggle(Content, Target.Raycast);
            if (Target.Raycast)
            {
                Target.Radius = EditorGUILayout.FloatField("Radius", Target.Radius);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Mask"));
            }

            EditorGUILayout.BeginHorizontal();
            Content = new GUIContent("Interpolation (Smooth)", "Interpolated value of the camera movement, which is only active if its value is greater than 0");
            Interp = EditorGUILayout.ToggleLeft(Content, Interp);
            GUI.enabled = Interp;
            Target.Smooth = EditorGUILayout.FloatField(Target.Smooth);
            Target.Smooth = (Interp) ? Target.Smooth : 0.0f;
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label("Axis");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MouseX"));
            Content = new GUIContent("Invert", "invert mouse delta on the horiontal axis");
            Target.InvertHorizontal = EditorGUILayout.ToggleLeft(Content, Target.InvertHorizontal, GUILayout.MaxWidth(60));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            Content = new GUIContent("Invert", "invert mouse delta on the vertical axis");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MouseY"));
            Target.InvertVertical = EditorGUILayout.ToggleLeft(Content, Target.InvertVertical, GUILayout.MaxWidth(60));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("RotateKey"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ResetKey"));

            serializedObject.ApplyModifiedProperties();
        }

        [DrawGizmo(GizmoType.Selected)]
        private void OnSceneGUI()
        {
            if (EditPosition)
            {
                int controlId = GUIUtility.GetControlID(FocusType.Passive);
                //Events overwrite on scene gui
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        EditPosition = false;
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                        break;

                    case EventType.MouseMove:
                        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        RaycastHit hit = new RaycastHit();
                        if (Physics.Raycast(ray, out hit, 100000.0f))
                        {
                            Target.StartPosition = hit.point;
                        }
                        break;
                }
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void OnDrawGizmo(Inspect scr, GizmoType gizmoType)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(scr.StartPosition, 0.8f);
        }


    }
}
