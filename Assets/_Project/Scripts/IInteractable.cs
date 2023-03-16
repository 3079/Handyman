using System;

namespace _Project.Scripts
{
    public interface IInteractable
    {
        public event Action<IInteractable> OnDestroyed;
        public void Interact();
    }
}
