using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.ComponentModel;

namespace Chapper.Layouts
{
    public class Layout : INotifyPropertyChanged
    {
        public static Layout CurrentLayout
        {
            get
            {
                if (AppController.Current != null)
                {
                    return AppController.Current.CurrentLayout;
                }
                else
                {
                    return new Layout();
                }
            }
        }

        public Layout()
        {
            
        }

        public int GeneralFontSize
        {
            get
            {
                return Properties.Settings.Default.layout_GeneralFontSize;
            }
            set
            {
                Properties.Settings.Default.layout_GeneralFontSize = value;
                NotifyPropertyChanged("GeneralFontSize");
            }
        }

        
        public int ItemBoxFontSizeContent
        {
            get
            {
                return Properties.Settings.Default.layout_itemBoxFontSizeContent;
            }
            set
            {
                Properties.Settings.Default.layout_itemBoxFontSizeContent = value;
                NotifyPropertyChanged("ItemBoxFontSizeContent");
            }
        }

        public int ItemBoxFontSizeHeader
        {
            get
            {
                return Properties.Settings.Default.layout_itemBoxFontSizeHeader;
            }
            set
            {
                Properties.Settings.Default.layout_itemBoxFontSizeHeader = value;
                NotifyPropertyChanged("ItemBoxFontSizeHeader");
            }
        }

        public int ItemBoxFontSizeFooter
        {
            get
            {
                return Properties.Settings.Default.layout_itemBoxFontSizeFooter;
            }
            set
            {
                Properties.Settings.Default.layout_itemBoxFontSizeFooter = value;
                NotifyPropertyChanged("ItemBoxFontSizeFooter");
            }
        }

        
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged == null)
            {
                PropertyChanged += new PropertyChangedEventHandler(Item_PropertyChanged);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;


        public int ItemBoxBorderRadius
        {
            get;
            set;
        }
    }
}
