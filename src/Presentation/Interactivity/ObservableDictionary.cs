using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYSoft.Presentation.Interactivity
{
    public class ObservableDictionary<TKey, TValue> :
        Dictionary<TKey, TValue>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public new TValue this[TKey key]
        {
            get => base[key];
            set
            {
                bool exists = base.ContainsKey(key);
                TValue? oldValue = exists ? base[key] : default;

                base[key] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));

                if (exists)
                {
                    // Replace 이벤트
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Replace,
                            new KeyValuePair<TKey, TValue>(key, value),
                            new KeyValuePair<TKey, TValue>(key, oldValue!)));
                }
                else
                {
                    // Add 이벤트
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            new KeyValuePair<TKey, TValue>(key, value)));
                }
            }
        }

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    new KeyValuePair<TKey, TValue>(key, value)));
        }

        public new bool Remove(TKey key)
        {
            if (base.TryGetValue(key, out TValue value) && base.Remove(key))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        new KeyValuePair<TKey, TValue>(key, value)));
                return true;
            }
            return false;
        }

        public new void Clear()
        {
            base.Clear();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
