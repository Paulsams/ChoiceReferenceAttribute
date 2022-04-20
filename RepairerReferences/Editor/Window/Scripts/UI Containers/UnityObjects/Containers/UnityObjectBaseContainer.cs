using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChoiceReferenceEditor.Repairer
{
    public abstract class UnityObjectBaseContainer
    {
        protected readonly VisualElement _container;

        protected UnityObjectBaseContainer(VisualElement container)
        {
            _container = container;
            Disable();
        }

        public virtual void Enable()
        {
            _container.style.display = DisplayStyle.Flex;
        }

        public virtual void Disable()
        {
            _container.style.display = DisplayStyle.None;
        }
    }
}
