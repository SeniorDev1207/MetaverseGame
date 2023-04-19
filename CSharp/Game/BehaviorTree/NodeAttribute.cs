﻿using System;

namespace BehaviorTree
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute: Attribute
    {
        public NodeType NodeType { get; private set; }

        public NodeAttribute(NodeType nodeType)
        {
            this.NodeType = nodeType;
        }
    }
}