// from
// https://answers.unity.com/questions/321615/code-to-mimic-find-references-in-scene.html?page=1&pageSize=5&sort=votes

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FindGameObjRef
{
	[MenuItem("CONTEXT/Component/Find Component References")]
	private static void FindReferences(MenuCommand data)
	{
		Object context = data.context;
		if (context)
		{
			var comp = context as Component;
			if (comp)
				FindReferencesTo(comp);
		}
	}

	[MenuItem("GameObject/Find GameObject References", false, 0)]
	public static void FindReferencesToAsset()
	{
		var selected = Selection.activeObject;
		if (selected)
			FindReferencesTo(selected);
	}

	private static void FindReferencesTo(Object target)
	{
		bool isGameObject = target is GameObject;
		Component[] toComponents = isGameObject ? ((GameObject)target).GetComponents<Component>() : null;
		string toName = isGameObject ? target.name : string.Format("{0}.{1}", target.name, target.GetType().Name);
		Scene curScene = GetBelongingScene(target);

		var referencedBy = new List<Object>();
		var allObjects = FindAllGameObjects(curScene).ToArray();
		for (int j = 0; j < allObjects.Length; j++)
		{
			GameObject go = allObjects[j];
			if (PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance)
			{
				if (PrefabUtility.GetPrefabParent(go) == target)
				{
					Debug.Log(string.Format("referenced by {0}, {1}", go.name, go.GetType()), go);
					referencedBy.Add(go);
				}
			}
			var components = go.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++)
			{
				var component = components[i];
				if (!component)
					continue;
				var so = new SerializedObject(component);
				var sp = so.GetIterator();
				while (sp.NextVisible(true))
				{
					if (sp.propertyType == SerializedPropertyType.ObjectReference)
					{
						if (sp.objectReferenceValue == target)
						{
							Debug.Log(string.Format("'{0}' referenced by '{1}' (Component: '{2}')", toName, component.name, component.GetType().Name), component);
							referencedBy.Add(component.gameObject);
						}
						else if (toComponents != null)
						{
							bool found = false;
							foreach (Component toComponent in toComponents)
							{
								if (sp.objectReferenceValue == toComponent)
								{
									found = true;
									referencedBy.Add(component.gameObject);
								}
							}
							if (found)
								Debug.Log(string.Format("'{0}' referenced by '{1}' (Component: '{2}')", toName, component.name, component.GetType().Name), component);
						}
					}
				}
			}
		}
		if (referencedBy.Count > 0)
			Selection.objects = referencedBy.ToArray();
		else
			Debug.Log(string.Format("'{0}': no references in scene", toName));
	}

	private static Scene GetBelongingScene(Object obj)
	{
		Scene curScene;
		if (obj is GameObject)
		{
			curScene = (obj as GameObject).scene;
		}
		else
		{
			curScene = (obj as Component).gameObject.scene;
		}

		return curScene;
	}

	/// <summary>
	/// Make as Scene Extension?
	/// </summary>
	public static List<GameObject> FindAllGameObjects(Scene scene)
	{
		List<GameObject> results = new List<GameObject>();
		List<Transform> tree = new List<Transform>();
		if (scene.isLoaded)
		{
			var allGameObjects = scene.GetRootGameObjects();
			for (int j = 0; j < allGameObjects.Length; j++)
			{
				var go = allGameObjects[j];
				tree.AddRange(go.transform.GetComponentsInChildren<Transform>(true));
			}
		}
		foreach (var trans in tree)
		{
			results.Add(trans.gameObject);
		}
		
		return results;
	}
}


