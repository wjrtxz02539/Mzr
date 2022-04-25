namespace Mzr.Web.Models.Web
{
    public class SortMetaData
    {
        public string Field { get; set; } 
        public SortEnum SortState { get; set; }

        public string SortValue
        {
            get
            {
                switch(SortState)
                {
                    case SortEnum.Ascending:
                        return Field;
                    case SortEnum.Descending:
                        return $"-{Field}";
                    default:
                        return string.Empty;
                }
            }
        }

        public SortMetaData(string field, SortEnum state = SortEnum.NoSort)
        {
            Field = field;
            SortState = state;
        }

        public SortMetaData(string sort)
        {
            if (string.IsNullOrEmpty(sort))
            {
                SortState = SortEnum.NoSort;
                Field = string.Empty;
            }
            else
            {
                if (sort.StartsWith("-"))
                {
                    SortState = SortEnum.Descending;
                    Field = sort.Remove(0, 1);
                }
                else
                {
                    SortState = SortEnum.Ascending;
                    Field = sort;
                }
            }
        }

        public void Next()
        {
            switch(SortState)
            {
                case SortEnum.Ascending:
                    SortState = SortEnum.Descending;
                    return;
                case SortEnum.Descending:
                    SortState = SortEnum.Ascending;
                    return;
                default:
                    SortState = SortEnum.NoSort;
                    return;
            }
        }

        public string GetIconClass()
        {
            switch (SortState)
            {
                case SortEnum.Ascending:
                    return "fa-solid fa-sort-up";
                case SortEnum.Descending:
                    return "fa-solid fa-sort-down";
                default:
                    return "fa-thin fa-sort";
            }
        }

        
    }
}
