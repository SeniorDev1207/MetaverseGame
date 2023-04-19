﻿namespace BehaviorTree
{
    [NodeAttribute(NodeType.Select, typeof(Selector))]
    public class Selector: Node
    {
        public Selector(NodeConfig config): base(config)
        {
        }

        public override bool Run(BlackBoard blackBoard)
        {
            foreach (var child in this.children)
            {
                if (child.Run(blackBoard))
                {
                    return true;
                }
            }
            return false;
        }
    }
}