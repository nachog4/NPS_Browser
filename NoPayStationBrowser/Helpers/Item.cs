using System.Linq;

namespace NPS
{
    public class Item
    {
        public string TitleId, Region, TitleName, zRfi, pkg;

        public Item(string TitleId, string Region, string TitleName, string pkg, string zrif)
        {
            this.TitleId = TitleId;
            this.Region = Region;
            this.TitleName = TitleName;
            this.pkg = pkg;
            this.zRfi = zrif;
        }

        public bool CompareName(string name)
        {
            name = name.ToLower();

            if (this.TitleId.ToLower().Contains(name)) return true;
            if (this.TitleName.ToLower().Contains(name)) return true;
            return false;
        }
    }


}
