﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Model
{
    public class BehaviorTreeFactory
    {
        private static readonly BehaviorTreeFactory instance = new BehaviorTreeFactory();

        public static BehaviorTreeFactory Instance
        {
            get
            {
                return instance;
            }
        }

        private Dictionary<int, Func<NodeConfig, Node>> dictionary;

        private BehaviorTreeFactory()
        {
        }

        public void Load(Assembly assembly)
        {
            this.dictionary = new Dictionary<int, Func<NodeConfig, Node>>();

            Type[] types = assembly.GetTypes();
            foreach (var type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(NodeAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }
                NodeAttribute attribute = (NodeAttribute)attrs[0];

                Type classType = type;
                this.dictionary.Add(attribute.NodeType,
                        config => (Node)Activator.CreateInstance(classType, new object[] { config }));
            } 
        }

        private Node CreateNode(NodeConfig config)
        {
            if (!this.dictionary.ContainsKey(config.Id))
            {
                throw new KeyNotFoundException(string.Format("CreateNode cannot found: {0}",
                        config.Id));
            }
            return this.dictionary[config.Id](config);
        }

        private Node CreateTreeNode(NodeConfig config)
        {
            var node = this.CreateNode(config);
            if (config.SubConfigs == null)
            {
                return node;
            }

            foreach (var subConfig in config.SubConfigs)
            {
                var subNode = this.CreateTreeNode(subConfig);
                node.AddChild(subNode);
            }
            return node;
        }

        public BehaviorTree CreateTree(NodeConfig config)
        {
            var node = this.CreateTreeNode(config);
            return new BehaviorTree(node);
        }
    }
}