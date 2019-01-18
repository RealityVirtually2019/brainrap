using System;

namespace Neurable.API
{
    public class Tag : Pointer
    {
        private Types.TagCallback _action, _animation;

        public Tag()
        {
            pointer = Library.CreateTag();
        }

        ~Tag()
        {
            Library.DeleteTag(pointer);
        }

        public bool SetDescription(string description)
        {
            return Library.TagSetDescription(pointer, description);
        }

        public string GetDescription()
        {
            return Library.TagGetDescription(pointer);
        }

        public bool SetCameraPerspective(float x, float y, float width, float height)
        {
            return Library.TagSetCameraPerspective(pointer, x, y, width, height);
        }

        public bool SetAction(Types.TagCallback callback)
        {
            _action = callback;
            return Library.TagSetAction(pointer, _action, new IntPtr());
        }

        public bool SimulateAction()
        {
            return Library.TagSimulateAction(pointer);
        }

        public bool SetAnimation(Types.TagCallback callback)
        {
            _animation = callback;
            return Library.TagSetAnimation(pointer, _animation, new IntPtr());
        }

        public bool SimulateAnimation()
        {
            return Library.TagSimulateAnimation(pointer);
        }

        public bool SetActive(bool active)
        {
            return Library.TagSetActive(pointer, active);
        }

        public bool GetActive()
        {
            return Library.TagGetActive(pointer);
        }
    };
}
