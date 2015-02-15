using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Chapper.Model
{
    interface IChapperCollection
    {
        ThreadSaveObservableCollection<Item> items { get; set; }
        string displayName { get; set; }
        string id { get; set; }

        void UpdateItems();
        void Kill();
    }
}
