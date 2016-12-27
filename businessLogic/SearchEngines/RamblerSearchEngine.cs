﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using businessLogic.Interfaces;
using businessLogic.Models;
using HtmlAgilityPack;

namespace businessLogic.SearchEngines
{
    public class RamblerSearchEngine: BaseSearchEngine, ISearchEngine
    {
        public SearchEngineResultsList Search(string query)
        {
            var resultList = CreateSearchEngineResultsList("Rambler");
            HtmlWeb web = new HtmlWeb();

            for (var i = 1; i <= 10; i++)
            {
                int c = 1;
                HtmlDocument document = web.Load($"http://nova.rambler.ru/search?scroll=1&utm_source=nhp&utm_content=search&utm_medium=button&utm_campaign=self_promo&query={query}&page={i}");
                HtmlNode[] searchResults = document.DocumentNode.SelectNodes("//body").ToArray();
                var lWrapper = searchResults[0].SelectNodes("//div[@class='l-wrapper']//a").ToArray();
                var lContact = lWrapper[0].SelectNodes("//div[@class='l-content__results']//a").ToArray();
                var bSerpItems = lContact[0].SelectNodes("//div[@class='b-serp-item']//a").ToList();

                foreach (var bSerpItem in bSerpItems)
                {
                    var header = bSerpItem.SelectNodes("//h2[@class='b-serp-item__header']//a");
                    var snippet = bSerpItem.SelectNodes("//p[@class='b-serp-item__snippet']");

                    for (var j = 0; j < 10; j++)
                    {
                        resultList.Results.Add(new Result
                        {
                            DisplayUrl = UrlConvert(header[j].GetAttributeValue("href", null)),
                            Title = header[j].InnerText,
                            Description = snippet[j].InnerText,
                            Rank = c++
                        });
                    }
                    break;
                }
            }

            return resultList;
        }
        
        public async Task<SearchEngineResultsList> Search1(string query)
        {
            var resultList = CreateSearchEngineResultsList("Rambler");
            
            for (var i = 1; i <= 10; i++)
            {
                resultList.Results.AddRange(await SingleSearchIteration(query, i));
            }

            resultList.Results = DistinctList(resultList.Results);
            return resultList;
        }

        private async Task<List<Result>> SingleSearchIteration(string query, int i)
        {
            var resultList = new List<Result>();
            int c = i*10-10;
            var document = await SearchRequest(query, i);
            HtmlNode[] searchResults = document.DocumentNode.SelectNodes("//body").ToArray();
            var lWrapper = searchResults[0].SelectNodes("//div[@class='l-wrapper']//a").ToArray();
            var lContact = lWrapper[0].SelectNodes("//div[@class='l-content__results']//a").ToArray();
            var bSerpItems = lContact[0].SelectNodes("//div[@class='b-serp-item']//a").ToList();

            foreach (var bSerpItem in bSerpItems)
            {
                var header = bSerpItem.SelectNodes("//h2[@class='b-serp-item__header']//a");
                var snippet = bSerpItem.SelectNodes("//p[@class='b-serp-item__snippet']");

                for (var j = 0; j < 10; j++)
                {
                    resultList.Add(new Result
                    {
                        DisplayUrl = UrlConvert(header[j].GetAttributeValue("href", null)),
                        Title = header[j].InnerText,
                        Description = snippet[j].InnerText,
                        Rank = c + j + 1
                    });
                }
                break;
            }

            return resultList;
        }

        private Task<HtmlDocument> SearchRequest(string query, int page)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.Load($"http://nova.rambler.ru/search?scroll=1&utm_source=nhp&utm_content=search&utm_medium=button&utm_campaign=self_promo&query={query}&page={page}");
            return Task.FromResult(document);
        }

    }
}
