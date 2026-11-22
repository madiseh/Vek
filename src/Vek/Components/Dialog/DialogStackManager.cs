namespace Vek
{
    /// <summary>
    /// Tracks dialog activation order for modal stacking.
    /// </summary>
    internal sealed class DialogStackManager
    {
        private readonly List<Guid> _dialogs = [];

        internal int GetIndex(Guid id) => _dialogs.IndexOf(id);

        internal void Add(Guid id)
        {
            _dialogs.Remove(id);
            _dialogs.Add(id);
        }

        internal bool Activate(Guid id)
        {
            var index = _dialogs.IndexOf(id);
            if (index < 0 || index == _dialogs.Count - 1)
            {
                return false;
            }

            _dialogs.RemoveAt(index);
            _dialogs.Add(id);
            return true;
        }

        internal void Remove(Guid id) => _dialogs.Remove(id);

        internal bool IsBackdropOwner(Guid id) => _dialogs.Count > 0 && _dialogs[^1] == id;
    }
}
