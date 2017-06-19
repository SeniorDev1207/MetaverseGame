﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESVector3Field : UESDisplayObject
{

    protected Vector3 m_Value;
    protected int drawingStyle;
    protected string m_LabelStr;
    protected GUIContent m_Label;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("Vector3Field");
        Rect = new Rect(Vector2.zero, new Vector2(INIT_WIDTH, 32));
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Value = EditorGUI.Vector3Field(m_DrawingRect, m_Label, m_Value);
                break;


            case 2:
                m_Value = EditorGUI.Vector3Field(m_DrawingRect, m_LabelStr, m_Value);
                break;
        }
    }

    public void SetDrawingStyle(GUIContent label)
    {
        drawingStyle = 1;

        m_Label = label;
    }

    public void SetDrawingStyle(string label)
    {
        drawingStyle = 2;

        m_LabelStr = label;
    }

    public Vector3 Value
    {
        get
        {
            return m_Value;
        }
        set
        {
            m_Value = value;
        }
    }
}
