using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BannerView.Controls
{
    public class CycleCollectionProvider<T> : ObservableCollection<T>
    {
        private readonly ObservableCollection<T> collection;

        public CycleCollectionProvider(ObservableCollection<T> collection)
        {
            this.collection = collection;
            UpdateCollection();
            UpdateHeader();
            UpdateFooter();
            collection.CollectionChanged += ItemsCollectionChanged;
        }

        private void UpdateCollection()
        {
            this.Clear();
            foreach (var item in collection)
            {
                this.Add(item);
            }
        }

        private void UpdateHeader()
        {
            if (collection.Count > 0)
            {
                if (this.Count <= collection.Count + 1)
                {
                    this.Insert(0, collection[collection.Count - 1]);
                }
                else
                {
                    if (!EqualityComparer<T>.Default.Equals(this[0], collection[collection.Count - 1]))
                    {
                        this[0] = collection[collection.Count - 1];
                    }
                }
            }
            else
            {
                this.Clear();
            }
        }

        private void UpdateFooter()
        {
            if (collection.Count > 0)
            {
                if (this.Count <= collection.Count + 1)
                {
                    this.Add(collection[0]);
                }
                else
                {
                    if (!EqualityComparer<T>.Default.Equals(this[this.Count - 1], collection[0]))
                    {
                        this[this.Count - 1] = collection[0];
                    }
                }
            }
            else
            {
                this.Clear();
            }
        }

        private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.Insert(e.NewStartingIndex, (T)e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Move:
                    this.MoveItem(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    this.RemoveAt(e.OldStartingIndex + 1);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    this[e.OldStartingIndex + 1] = (T)e.NewItems[0];
                    break;
                case NotifyCollectionChangedAction.Reset:
                    UpdateCollection();
                    break;
            }
            UpdateHeader();
            UpdateFooter();
        }

    }
}
