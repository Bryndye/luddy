using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FlexLayout.Edit
{
    public static class FlexLayoutEditorUtility
    {
        [MenuItem("GameObject/UI/Flex Item")]
        public static void AddFlexItem()
        {
            AddFlexComponent<UIFlexItem>("Flex Item");
        }

        [MenuItem("GameObject/UI/Flex Container")]
        public static void AddFlexContainer()
        {
            AddFlexComponent<UIFlexContainer>("Flex Container");
        }

        private static void AddFlexComponent<T>(string name) where T : MonoBehaviour
        {
            Transform parent;
            Canvas canvas;
            T component;

            if(Selection.activeGameObject == null || GameObject.FindAnyObjectByType<Canvas>() == null)
            {
                canvas = EnsureCanvasFind();
                parent = canvas.transform;
            }
            else
                parent = EnsureCanvasParent(Selection.activeGameObject);
            component = CreateNewItem<T>();
            component.transform.SetParent(parent, false);
            component.gameObject.name = name;
            Selection.activeGameObject = component.gameObject;
        }

        private static T CreateNewItem<T>() where T : MonoBehaviour
        {
            GameObject obj;
            T item;

            obj = new GameObject();
            obj.layer = LayerMask.NameToLayer("UI");
            item = obj.AddComponent<T>();
            return (item);
        }

        private static Canvas EnsureCanvasFind()
        {
            Canvas canvas;

            canvas = GameObject.FindAnyObjectByType<Canvas>();
            if (canvas != null)
                return (canvas);
            EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas");
            canvas = GameObject.FindAnyObjectByType<Canvas>();
            return (canvas);
        }

        private static Transform EnsureCanvasParent(GameObject parent)
        {
            Canvas canvas;

            canvas = parent.transform.GetComponentInParent<Canvas>();
            if (canvas != null)
                return (parent.transform);
            canvas = new GameObject("Canvas").AddComponent<Canvas>();
            canvas.transform.SetParent(parent.transform, false);
            return (canvas.transform);
        }
    }
}
