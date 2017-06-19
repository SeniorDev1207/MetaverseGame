﻿using Model;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	[CustomEditor(typeof (BehaviorTreeConfig))]
	public class BehaviorTreeConfigEditor: Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			BehaviorTreeConfig config = target as BehaviorTreeConfig;

			if (GUILayout.Button("打开行为树"))
			{
				BehaviorManager.Instance.OpenBehaviorEditor(config.gameObject);
			}
			EditorUtility.SetDirty(config);
		}
	}
}