﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComparisonExpressionVisualizer
{
    public class ObservableDictionary<TKey, TValue> 
        : ObservableCollection<Pair<TKey, TValue>>
    {
        public ObservableDictionary()
            : base()
        {
        }

        public ObservableDictionary(IEnumerable<Pair<TKey, TValue>> enumerable)
            : base(enumerable)
        {
        }

        public ObservableDictionary(List<Pair<TKey, TValue>> list)
            : base(list)
        {
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            foreach (var kv in dictionary)
            {
                Add(new Pair<TKey, TValue>(kv));
            }
        }

        public void Add(TKey key, TValue value)
        {
            Add(new Pair<TKey, TValue>(key, value));
        }
    }

    public class Pair<TKey, TValue> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected TKey _key;
        protected TValue _value;

        public Pair()
        {
        }

        public Pair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public Pair(KeyValuePair<TKey, TValue> kv)
        {
            Key = kv.Key;
            Value = kv.Value;
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public TKey Key
        {
            get { return _key; }
            set
            {
                if (
                    (_key == null && value != null)
                    || (_key != null && value == null)
                    || !_key.Equals(value))
                {
                    _key = value;
                    NotifyPropertyChanged("Key");
                }
            }
        }

        public TValue Value
        {
            get { return _value; }
            set
            {
                if (
                    (_value == null && value != null)
                    || (_value != null && value == null)
                    || (_value != null && !_value.Equals(value)))
                {
                    _value = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }
    }
}
