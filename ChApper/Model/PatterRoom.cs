using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using AppNetDotNet.Model;

namespace Chapper.Model
{
    class PatterRoom : IChapperCollection
    {
        public PatterRoom(Channel subscribedChannel) {
            if(subscribedChannel == null) {
                return;
            }
            displayName = "No name";
            items = new ThreadSaveObservableCollection<Item>();
            items.CollectionChanged += items_CollectionChanged;
            channel = subscribedChannel;
            if (channel.annotations != null)
            {
                foreach (Annotation annotation in channel.annotations)
                {
                    if (annotation.type == "net.patter-app.settings")
                    {
                        if (annotation.parsedObject != null)
                        {
                            AppNetDotNet.Model.Annotations.Patter settings = annotation.parsedObject as AppNetDotNet.Model.Annotations.Patter;
                            if (settings != null)
                            {
                                displayName = settings.name;
                            }
                        }
                        
                    }
                }
            }
        }

        void items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (AppController.Current.account != null)
            {
                if (AppController.Current.account.initialUpdateCompleted)
                {
                    if (e.NewItems != null)
                    {
                        foreach (Item item in e.NewItems)
                        {
                            AppController.Current.account.snarlInterface.Notify("Patter " + this.displayName, item.displayName, item.text, 10, item.avatar, "");
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return displayName;
        }

        public ThreadSaveObservableCollection<Item> items
        {
           get;set;
        }

        public string displayName
        {
            get;
            set;
        }

        public void UpdateItems()
        {
            // those are updated by the complete fetch within messages retrival
            return;
        }

        public void Kill()
        {
            items = null;
        }

        public Channel channel { get; set; }



        public string id
        {
            get
            {
                if (channel != null)
                {
                    return channel.id;
                }
                return "";
            }
            set
            {
                
            }
        }
    }
}
