using System;
using UnityEngine;

namespace Neurable.Interactions
{
    /*
 * The required compenents of a Neurable Tag are Outlined in the INeurableSelectable Interface
 */
    public interface INeurableSelectable
    {
        API.Tag NeuroTag { get; } // Neurable API Object
        int NeurableID { get; }

        void NeurableSelectionFunction(int tagID,
                                       IntPtr descritption,
                                       IntPtr p); // Function given to the API that is called on Tag Selection

        void NeurableAnimationFunction(int tagID,
                                       IntPtr descritption,
                                       IntPtr p); // Function given to the API that is called on Tag Animation (BETA feature)

        bool NeurableEnabled { get; set; } // Is the Tag Selectable in the Scene
        bool NeurableVisible { get; set; } // Is the Tag Visible (and therefore Selectable) in the Scene

        Vector2
            ProjectedPosition
        {
            get;
        } // 2D Projection of the objects position in screen space. Should coincide with Eyetracker coordinate system.

        void UpdatePosition(bool force); // Should update the Projected Position
    }
}
