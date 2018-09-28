using AuraWeb.Models;
using EVEStandard;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AuraWeb.Controllers
{
    public class NewsController : _BaseController
    {
        private readonly IConfiguration _Config;
        private readonly ILogger<NewsController> _Log;
        private readonly EVEStandardAPI _ESIClient;

        public NewsController(ILogger<NewsController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
        {
            _Log = logger;
            _Config = configuration;
            this._ESIClient = esiClient;
        }

        public List<NewsDataModel> GetNewsEve24()
        {
            List<NewsDataModel> result = new List<NewsDataModel>();
            using (var hc = new HttpClient())
            {
                HttpResponseMessage response = hc.GetAsync("http://evenews24.com/").Result;
                Stream stream = response.Content.ReadAsStreamAsync().Result;
                HtmlDocument _WebDocument = new HtmlDocument();
                _WebDocument.Load(stream);

                HtmlNodeCollection _webnodes = _WebDocument.DocumentNode.SelectNodes("//div[contains(@class,'hentry clearfix')]");

                foreach(HtmlNode _node in _webnodes)
                {
                    NewsDataModel _newsItem = new NewsDataModel();
                    if (_node != null && _node.ChildNodes != null && _node.ChildNodes.Count >= 10)
                    {
                        if(_node.ChildNodes[1].Name == "h2") // Implies a proper news title
                        {
                            // Get title
                            _newsItem.Title = _node.ChildNodes[1].InnerText;

                            // Get Post Author and Date
                            if (_node.ChildNodes[3].Name == "div")
                            {
                                if (_node.ChildNodes[3].ChildNodes != null && _node.ChildNodes[3].ChildNodes.Count >= 4)
                                {
                                    if (_node.ChildNodes[3].ChildNodes[1].Name == "span")
                                    {
                                        _newsItem.PostAuthor = _node.ChildNodes[3].ChildNodes[1].InnerText;
                                    }
                                    if (_node.ChildNodes[3].ChildNodes[1].ChildNodes != null && _node.ChildNodes[3].ChildNodes[1].ChildNodes.Count > 0 && _node.ChildNodes[3].ChildNodes[1].ChildNodes[0].Name == "a" && _node.ChildNodes[3].ChildNodes[1].ChildNodes[0].Attributes != null && _node.ChildNodes[3].ChildNodes[1].ChildNodes[0].Attributes.Count >= 2 && _node.ChildNodes[3].ChildNodes[1].ChildNodes[0].Attributes[1].Name == "href")
                                    {
                                        _newsItem.PostAuthorUrl = _node.ChildNodes[3].ChildNodes[1].ChildNodes[0].Attributes[1].Value;
                                    }
                                    if (_node.ChildNodes[3].ChildNodes[3].Name == "span")
                                    {
                                        string postDateString = _node.ChildNodes[3].ChildNodes[3].InnerText;
                                        DateTime postDate = DateTime.MinValue;
                                        DateTime.TryParse(postDateString, out postDate);

                                        string dateFormat1 = "MMM dd'st' yyyy h:mmtt";
                                        DateTime.TryParseExact(postDateString, dateFormat1, CultureInfo.InvariantCulture, DateTimeStyles.None, out postDate);
                                        string dateFormat2 = "MMM dd'rd' yyyy h:mmtt";
                                        if (postDate == DateTime.MinValue) DateTime.TryParseExact(postDateString, dateFormat2, CultureInfo.InvariantCulture, DateTimeStyles.None, out postDate);
                                        string dateFormat3 = "MMM dd'th' yyyy h:mmtt";
                                        if (postDate == DateTime.MinValue) DateTime.TryParseExact(postDateString, dateFormat3, CultureInfo.InvariantCulture, DateTimeStyles.None, out postDate);
                                        string dateFormat4 = "MMM dd'nd' yyyy h:mmtt";
                                        if (postDate == DateTime.MinValue) DateTime.TryParseExact(postDateString, dateFormat4, CultureInfo.InvariantCulture, DateTimeStyles.None, out postDate);

                                        if (postDate != DateTime.MinValue) _newsItem.PostDate = postDate;
                                    }
                                }
                            }

                            // Get image
                            if (_node.ChildNodes[5].Name == "div")
                            {
                                if (_node.ChildNodes[5].ChildNodes != null && _node.ChildNodes[5].ChildNodes.Count >= 4)
                                {
                                    if (_node.ChildNodes[5].ChildNodes[3].Name == "img" && _node.ChildNodes[5].ChildNodes[3].Attributes.Count > 0 && _node.ChildNodes[5].ChildNodes[3].Attributes[0].Name == "src")
                                    {
                                        _newsItem.ImgSrc = _node.ChildNodes[5].ChildNodes[3].Attributes[0].Value;
                                    }
                                }
                            }

                            // Get summary
                            if (_node.ChildNodes[7].Name == "div")
                            {
                                _newsItem.Summary = _node.ChildNodes[7].InnerText;
                            }
                            
                            // Get Url
                            if (_node.ChildNodes[9].Name == "div")
                            {
                                if (_node.ChildNodes[9].ChildNodes != null && _node.ChildNodes[9].ChildNodes.Count >= 2 && _node.ChildNodes[9].ChildNodes[1].Name == "a" && _node.ChildNodes[9].ChildNodes[1].Attributes != null && _node.ChildNodes[9].ChildNodes[1].Attributes.Count >= 1 && _node.ChildNodes[9].ChildNodes[1].Attributes[0].Name == "href")
                                {
                                    _newsItem.PostUrl = _node.ChildNodes[9].ChildNodes[1].Attributes[0].Value;
                                }
                            }

                            // Get Description
                            // TODO: Fix this later
                            /*
                            if (!String.IsNullOrWhiteSpace(_newsItem.PostUrl))
                            {
                                using (var hc2 = new HttpClient())
                                {
                                    HttpResponseMessage response2 = hc.GetAsync(_newsItem.PostUrl).Result;
                                    Stream stream2 = response2.Content.ReadAsStreamAsync().Result;
                                    HtmlDocument _WebDocument2 = new HtmlDocument();
                                    _WebDocument2.Load(stream2);

                                    HtmlNodeCollection _webnodes2 = _WebDocument2.DocumentNode.SelectNodes("//div[contains(@class,'entry-content')]");

                                    StringBuilder description = new StringBuilder();

                                    foreach (HtmlNode _postNode in _webnodes2)
                                    {
                                        if (_postNode.Name == "div" && _postNode.ChildNodes != null && _postNode.ChildNodes.Count > 0) // Implies is part of the post
                                        {
                                            List<HtmlNode> postPNodes = _postNode.ChildNodes.Where(x => x.Name == "p").ToList();
                                            if (postPNodes.Count > 0)
                                            {
                                                foreach (HtmlNode _postPNodes in postPNodes)
                                                {
                                                    description.Append(_postPNodes.InnerText + Environment.NewLine);
                                                }
                                            }
                                            else
                                            {
                                                // They nest like this because they hate me
                                                HtmlNode featuredImgNode = _postNode.ChildNodes.Where(x => x.Name == "div" && x.HasClass("featured-img")).FirstOrDefault();
                                                foreach (HtmlNode _postSubNode in featuredImgNode.ChildNodes)
                                                {
                                                    if (_postSubNode.Name == "p")
                                                    {
                                                        description.Append(_postSubNode.InnerText + Environment.NewLine);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    _newsItem.Description = description.ToString();
                                }
                            }*/

                            result.Add(_newsItem);
                        }
                    }
                }
            }
            return result;
        }

        public async Task<IActionResult> Index()
        {
            List<NewsDataModel> News_Eve24 = GetNewsEve24();

            var model = new NewsPageViewModel()
            {
                News_Eve24 = News_Eve24
            };

            return View(model);
        }
    }
}
