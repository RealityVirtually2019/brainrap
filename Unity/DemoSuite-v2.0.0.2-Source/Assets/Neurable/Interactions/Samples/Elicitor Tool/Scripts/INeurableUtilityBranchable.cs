using System.Collections.Generic;

namespace Neurable.Interactions.Samples
{
    // NeurableUtilityBranchable Objects are objects with multiple controllable components
    // that can be swapped and activated by the utility
    public interface INeurableUtilityBranchable : INeurableUtilityControllable
    {
        List<INeurableUtilityControllable> ControllableComponents { get; set; }
        List<string> ControllableComponentStrings { get; }
        INeurableUtilityControllable ActiveComponent { get; set; }
        void ChangeComponent(int ComponentIndex);
    };
}
