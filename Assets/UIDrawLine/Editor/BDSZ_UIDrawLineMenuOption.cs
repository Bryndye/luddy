using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BDSZ_2020;
namespace BDSZ_2020
{
	/// <summary>
	/// Add Particle System to UI menu option to the Unity Editor
	/// </summary>
	public class BDSZ_UIDrawLineMenuOption
    {


        [MenuItem("GameObject/UI/Line", false, 2101)]
        public static void AddUILine(MenuCommand menuCommand)
        {
            GameObject gameObject = new GameObject("Line");
            gameObject.AddComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);
            BDSZ_UIDrawLine line = gameObject.AddComponent<BDSZ_UIDrawLine>();
            line.raycastTarget = false;
           // line.AddLineSegment(new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), 4.0f, Color.white);
            PlaceUIElementRoot(gameObject, menuCommand);
        }
        [MenuItem("GameObject/UI/LineEx", false, 2102)]
        public static void AddUILineEx(MenuCommand menuCommand)
        {
            GameObject gameObject = new GameObject("LineEx");
            gameObject.AddComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);
            BDSZ_UIDrawLineEx line = gameObject.AddComponent<BDSZ_UIDrawLineEx>();
           
            line.raycastTarget = false;
            // line.AddLineSegment(new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), 4.0f, Color.white);
            PlaceUIElementRoot(gameObject, menuCommand);
            line.canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
        }
        [MenuItem("GameObject/UI/ShapeLine", false, 2103)]
        public static void AddUIShapeLine(MenuCommand menuCommand)
        {
            GameObject gameObject = new GameObject("ShapeLine");
            gameObject.AddComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);
            BDSZ_UIShapeLine line = gameObject.AddComponent<BDSZ_UIShapeLine>();

            line.raycastTarget = false;
            // line.AddLineSegment(new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), 4.0f, Color.white);
            PlaceUIElementRoot(gameObject, menuCommand);
            line.canvas.additionalShaderChannels |= (AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2|AdditionalCanvasShaderChannels.TexCoord3);
        }
        // ReSharper disable once InconsistentNaming
        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = GetOrCreateCanvasGameObject();
            }
            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name = uniqueName;
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);
            if (parent != menuCommand.context) // not a context click, so center in sceneview
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Selection.activeGameObject = element;
        }


        private static GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use just any canvas..
#if UNITY_6000_0_OR_NEWER
            canvas = Object.FindFirstObjectByType(typeof(Canvas)) as Canvas;
#else
            canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
#endif
            if (canvas != null && canvas.gameObject.activeInHierarchy)
            return canvas.gameObject;

            // No canvas in the scene at all? Then create a new one.
            return CreateNewUI();
        }

        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            // Find the best scene view
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
                return;

            // Create world space Plane from canvas position.
            Vector2 localPlanePosition;
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2f, camera.pixelHeight / 2f), camera, out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            itemTransform.anchoredPosition = position;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }

        // ReSharper disable once InconsistentNaming
        private static GameObject CreateNewUI()
        {
            // Root for the UI
            var root = new GameObject("Canvas")
            {
                layer = LayerMask.NameToLayer("UI")
            };
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            // if there is no event system add one...
            CreateEventSystem(false);
            return root;
        }

        private static void CreateEventSystem(bool select, GameObject parent = null)
        {
#if UNITY_6000_0_OR_NEWER
            var esys = Object.FindFirstObjectByType<EventSystem>();
#else
            var esys = Object.FindObjectOfType<EventSystem>();
#endif
            if (esys == null)
            {
                var eventSystem = new GameObject("EventSystem");
                GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }

            if (select && esys != null)
            {
                Selection.activeGameObject = esys.gameObject;
            }
        }
    }
 
}
