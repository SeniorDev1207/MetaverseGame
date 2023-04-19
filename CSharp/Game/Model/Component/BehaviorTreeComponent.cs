﻿using System.Collections.Generic;
using System.Reflection;
using Common.Base;

namespace Model
{
    public class BehaviorTreeComponent: Component<World>
    {
        private Dictionary<int, BehaviorTree> trees = new Dictionary<int, BehaviorTree>();

        public void Load(Assembly assembly)
        {
            BehaviorTreeFactory behaviorTreeFactory = BehaviorTreeFactory.Instance;
            behaviorTreeFactory.Load(assembly);

            NodeConfig[] nodeConfigs = World.Instance.GetComponent<ConfigComponent>().GetAll<NodeConfig>();
            foreach (NodeConfig nodeConfig in nodeConfigs)
            {
                BehaviorTree behaviorTree = behaviorTreeFactory.CreateTree(nodeConfig);
                this.trees[nodeConfig.Id] = behaviorTree;
            }
        }
    }
}
