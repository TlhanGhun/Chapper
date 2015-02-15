using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppNetDotNet.Model;
using AppNetDotNet.ApiCalls;

namespace Chapper.Model
{
    public class Conversation : IChapperCollection
    {
        private Item startingPointPost;

        public Conversation(Item initialPost)
        {
            items = new ThreadSaveObservableCollection<Item>();
            if(initialPost == null) {
                return;
            }
            startingPointPost = initialPost;
            this.displayName = "Conversation with @" + initialPost.user.username;
            if(initialPost.apnMessage == null) {
                Tuple<List<Post>, ApiCallResponse> conversation = Posts.getRepliesById(AppController.Current.account.accessToken, initialPost.id);
                if (conversation.Item2.success)
                {
                    foreach (Post post in conversation.Item1)
                    {
                        items.Add(new Item(post));
                    }
                }
            }
            else
            {
                Tuple<List<Message>, ApiCallResponse> conversation = Messages.getMessagesInChannel(AppController.Current.account.accessToken, initialPost.apnMessage.channel_id);
                if (conversation.Item2.success)
                {
                    foreach (Message message in conversation.Item1)
                    {
                        items.Add(new Item(message));
                    }
                }
            }
          
            this.id = "conversation_" + initialPost.id;
        }

        public ThreadSaveObservableCollection<Item> items
        {
            get;
            set;
        }

        public string displayName
        {
            get;
            set;
        }

        public string id
        {
            get;
            set;
        }

        public void UpdateItems()
        {
            throw new NotImplementedException();
        }

        public void Kill()
        {
            throw new NotImplementedException();
        }
    }
}
