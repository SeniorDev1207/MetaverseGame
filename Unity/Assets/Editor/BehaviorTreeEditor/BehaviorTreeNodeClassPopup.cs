﻿using System;
using System.Collections.Generic;
using Model;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	public class BehaviorTreeNodeClassPopup
	{
		private string mSearchNode = "";
		private Vector2 mTreeScrollPos = Vector2.zero;
		private int mNodeCount;
		private readonly float mWidth = 400f;

		public GraphDesigner GraphDesigner;
		private SubWinType mSubWinType;
		public Rect windowRect; //子窗口的大小和位置
		string[] mEnumNodeTypeArr;
		Rect toolbarRect = new Rect(0f, 0f, 0, 0);
		private int mEnumNodeTypeSelection;

		public string DrawSearchList()
		{
			List<string> targetList = new List<string>();
			if (mSubWinType == SubWinType.CreateNode)
			{
				targetList = GraphDesigner.GetCanCreateList();
			}
			else if (mSubWinType == SubWinType.ReplaceNode)
			{
				targetList = GraphDesigner.GetCanRepalceList();
			}

			List<string> nodeNameList = Filter(targetList, mSearchNode);

			GUILayout.BeginHorizontal();
			GUI.SetNextControlName("Search");
			this.mSearchNode = GUILayout.TextField(this.mSearchNode, GUI.skin.FindStyle("ToolbarSeachTextField"));
			GUI.FocusControl("Search");
			GUILayout.EndHorizontal();
			//
			toolbarRect = new Rect(0f, 15f + 20, mWidth, 25f);
			Rect boxRect = new Rect(0f, toolbarRect.height, this.mWidth, (Screen.height - toolbarRect.height) - 21f + 10);
			GUILayout.BeginArea(toolbarRect, EditorStyles.toolbar);
			GUILayout.BeginHorizontal();

			GUILayout.Label("Filter");
			Array strArr = Enum.GetValues(typeof (NodeClassifyType));
			List<string> strList = new List<string>();
			strList.Add("All");
			foreach (var str in strArr)
			{
				strList.Add(str.ToString());
			}
			mEnumNodeTypeArr = strList.ToArray();
			mEnumNodeTypeSelection = EditorGUILayout.Popup(mEnumNodeTypeSelection, mEnumNodeTypeArr);
			if (GUILayout.Button("Clear"))
			{
				ClearNodes();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			//

			GUILayout.BeginArea(new Rect(0, 0, windowRect.width, windowRect.height));
			float topSpace = 60;
			this.mTreeScrollPos = GUI.BeginScrollView(new Rect(0f, topSpace, windowRect.width, windowRect.height - topSpace), this.mTreeScrollPos,
					new Rect(0f, 0f, windowRect.width - 20f, nodeNameList.Count * 19), false, true);

			foreach (var name in nodeNameList)
			{
				ClientNodeTypeProto proto = ExportNodeTypeConfig.GetNodeTypeProtoFromDll(name);
				if (GUILayout.Button(name + $"({proto.describe})", GetButtonStyle()))
				{
					if (SubWinType.CreateNode == mSubWinType)
					{
						GraphDesigner.onCreateNode(name, Vector2.zero);
					}
					else if (SubWinType.ReplaceNode == mSubWinType)
					{
						GraphDesigner.onChangeNodeType(name, Vector2.zero);
					}
					BehaviorDesignerWindow.Instance.CloseSubWin();
				}
			}

			GUI.EndScrollView();
			GUILayout.EndArea();

			return "";
		}

		private void ClearNodes()
		{
			mEnumNodeTypeSelection = 0;
			mSearchNode = "";
		}

		public List<string> Filter(List<string> list, string text)
		{
			List<string> result1 = new List<string>();
			string selectType;
			if (mEnumNodeTypeSelection == 0)
			{
				selectType = "All";
				result1 = list;
			}
			else
			{
				selectType = Enum.GetName(typeof (NodeClassifyType), mEnumNodeTypeSelection - 1);
				foreach (var name in list)
				{
					ClientNodeTypeProto proto = ExportNodeTypeConfig.GetNodeTypeProtoFromDll(name);
					if (selectType == proto.classify)
					{
						result1.Add(name);
					}
				}
			}

			if (string.IsNullOrEmpty(text))
			{
				return result1;
			}

			List<string> result2 = new List<string>();
			foreach (var name in result1)
			{
				ClientNodeTypeProto proto = ExportNodeTypeConfig.GetNodeTypeProtoFromDll(name);
				if (name.ToUpper().Contains(text.ToUpper()) || proto.describe.ToUpper().Contains(text.ToUpper()))
				{
					result2.Add(name);
				}
			}
			return result2;
		}

		//private readonly Color textColor = new Color(200f / 255f, 200f / 255f, 200f / 255f);
		private readonly Color textColor = new Color(100f / 255f, 100f / 255f, 0f, 1);
		private readonly Color textHighLightColor = new Color(100f / 255f, 100f / 255f, 0f, 1);

		public GUIStyle GetButtonStyle()
		{
			GUIStyle style = new GUIStyle();
			style.fontSize = 15;
			style.alignment = TextAnchor.MiddleLeft;
			GUIStyleState onHoverStyleState = new GUIStyleState();
			//onHoverStyleState.textColor = textHighLightColor;
			onHoverStyleState.background = BehaviorDesignerUtility.GetTexture("blue");
			style.hover = onHoverStyleState;

			GUIStyleState onNormalStyleState = new GUIStyleState();
			//onNormalStyleState.textColor = textColor;
			style.normal = onNormalStyleState;
			return style;
		}

		public void Show(Rect rect, SubWinType subWinType)
		{
			windowRect = rect;
			mSubWinType = subWinType;
		}
	}
}