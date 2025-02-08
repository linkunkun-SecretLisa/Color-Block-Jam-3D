using System.Collections.Generic;
using Runtime.Entities;

namespace Runtime.Classes
{
    public class ItemsClass
    {
        private List<Item> itemsList = new List<Item>();

        public void AddItem(Item item)
        {
            itemsList.Add(item);
        }

        public List<Item> GetItems()
        {
            return itemsList;
        }

        public void RemoveItem(Item item)
        {
            if (itemsList.Contains(item))
            {
                itemsList.Remove(item);
            }
        }

        public void ClearItems()
        {
            itemsList.Clear();
        }
    }
}