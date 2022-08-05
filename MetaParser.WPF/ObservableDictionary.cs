using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace MetaParser.WPF
{
    public class ObservableDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>,
        ICollection<KeyValuePair<TKey, TValue>>,
        IEnumerable<KeyValuePair<TKey, TValue>>,
        IDictionary,
        ICollection,
        IEnumerable,
        ISerializable,
        INotifyCollectionChanged,
        INotifyPropertyChanged
    {
        private readonly IDictionary<TKey, TValue> backingDictionary;

        public ObservableDictionary()
            : this(new Dictionary<TKey, TValue>())
        { }

        public ObservableDictionary(IDictionary<TKey, TValue> dict)
        {
            backingDictionary = dict;
        }

        private ObservableDictionary(SerializationInfo info, StreamingContext context)
            : this()
        {
            var count = info.GetInt32("Count");
            for (var i = 0; i < count; ++i)
            {
                var key = (TKey)info.GetValue($"Key[{i}]", typeof(TKey));
                var value = (TValue)info.GetValue($"Value[{i}]", typeof(TValue));
                backingDictionary.Add(key, value);
            }
        }

        public TValue this[TKey key]
        {
            get => backingDictionary[key];
            set
            {
                if (ContainsKey(key))
                {
                    var oldValue = backingDictionary[key];
                    backingDictionary[key] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new[] { new KeyValuePair<TKey, TValue>(key, value) }, new[] { new KeyValuePair<TKey, TValue>(key, oldValue) }));
                }
                else
                    Add(key, value);
            }
        }
        public object this[object key]
        {
            get => backingDictionary[(TKey)key];
            set => this[(TKey)key] = (TValue)value;
        }

        public ICollection<TKey> Keys => backingDictionary.Keys;

        public ICollection<TValue> Values => backingDictionary.Values;

        public int Count => backingDictionary.Count;

        public bool IsReadOnly => (backingDictionary as ICollection<KeyValuePair<TKey, TValue>>).IsReadOnly;

        public bool IsFixedSize => (backingDictionary as IDictionary).IsFixedSize;

        public object SyncRoot => (backingDictionary as ICollection).SyncRoot;

        public bool IsSynchronized => (backingDictionary as ICollection).IsSynchronized;

        ICollection IDictionary.Keys => (ICollection)backingDictionary.Keys;

        ICollection IDictionary.Values => (ICollection)backingDictionary.Values;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            (backingDictionary as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            var idx = GetIndex(item.Key);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, idx.Value));
        }

        public void Add(object key, object value) => Add(new KeyValuePair<TKey, TValue>((TKey)key, (TValue)value));

        public void Clear()
        {
            var list = new List<KeyValuePair<TKey, TValue>>(backingDictionary);
            backingDictionary.Clear();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, list));
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => (backingDictionary as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);

        public bool Contains(object key) => ContainsKey((TKey)key);

        public bool ContainsKey(TKey key) => backingDictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => (backingDictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);

        public void CopyTo(Array array, int index) => (backingDictionary as ICollection).CopyTo(array, index);

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Count", Count);
            var i = 0;
            foreach (var item in backingDictionary)
            {
                info.AddValue($"Key[{i}]", item.Key, typeof(TKey));
                info.AddValue($"Value[{i}]", item.Value, typeof(TValue));
            }
        }

        public bool Remove(TKey key)
        {
            if (TryGetValue(key, out var value))
            {
                var idx = GetIndex(key);
                var result = backingDictionary.Remove(key);
                if (result)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value), idx.Value));
                }
                return result;
            }

            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var idx = GetIndex(item.Key);
            var result = (backingDictionary as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
            if (result)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, idx.Value));
            }
            return result;
        }

        public void Remove(object key) => Remove((TKey)key);

        public bool TryGetValue(TKey key, out TValue value) => backingDictionary.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => backingDictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator(backingDictionary.GetEnumerator());

        private int? GetIndex(TKey key) => (backingDictionary as ICollection<KeyValuePair<TKey, TValue>>)
                .Select((c, i) => new { key = c.Key, index = i })
                .FirstOrDefault(c => c.key.Equals(key))?
                .index;

        private class DictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> enumerator;

            public DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
            {
                this.enumerator = enumerator;
            }

            public object Key => enumerator.Current.Key;

            public object Value => enumerator.Current.Value;

            public DictionaryEntry Entry => new DictionaryEntry(Key, Value);

            public object Current => enumerator.Current;

            public bool MoveNext() => enumerator.MoveNext();

            public void Reset() => enumerator.Reset();
        }
    }
}
