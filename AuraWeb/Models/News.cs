using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Models
{
    public class NewsDataModel
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string PostAuthor { get; set; }
        public string PostAuthorUrl { get; set; }
        public DateTime? PostDate { get; set; }
        public string PostUrl { get; set; }
        public string ImgSrc { get; set; }
    }

    public class NewsPageViewModel
    {
        public List<NewsDataModel> News_Eve24 { get; set; }
    }
}
