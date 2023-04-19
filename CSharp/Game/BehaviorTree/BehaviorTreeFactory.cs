﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Model;

namespace BehaviorTree
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

        private readonly Dictionary<NodeType, Func<NodeConfig, Node>> dictionary =
                new Dictionary<NodeType, Func<NodeConfig, Node>>();

        private BehaviorTreeFactory()
        {
            Assembly assembly = typeof(BehaviorTreeFactory).Assembly;
            this.RegisterNodes(assembly);
        }

        public void RegisterNodes(Assembly assembly)
        {
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
                        config =>
                                (Node)Activator.CreateInstance(classType, new object[] { config }));
            } 
        }

        private Node CreateNode(NodeConfig config)
        {
            if (!this.dictionary.ContainsKey((NodeType) config.Id))
            {
                throw new KeyNotFoundException(string.Format("CreateNode cannot found: {0}",
                        config.Id));
            }
            return this.dictionary[(NodeType) config.Id](config);
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