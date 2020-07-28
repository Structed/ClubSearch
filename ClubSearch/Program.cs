using System;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using ConsoleTables;

namespace ClubSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            string indexName = "faceted-indexer";

            // Get the service endpoint and API key from the environment
            Uri endpoint = new Uri(Environment.GetEnvironmentVariable("SEARCH_ENDPOINT"));
            string key = Environment.GetEnvironmentVariable("SEARCH_API_KEY");

            // Create a client
            AzureKeyCredential credential = new AzureKeyCredential(key);
            SearchClient client = new SearchClient(endpoint, indexName, credential);


            string searchQuery = "and";
            string regionToScoreBy = "Europe";
            int resultSize = 10;

            SearchOptions searchOptions;
            if (string.IsNullOrWhiteSpace(regionToScoreBy))
            {
                searchOptions = new SearchOptions { Size = resultSize};
            }
            else
            {
                // need to use a new object because ScoringProfile is a read-only property
                searchOptions = new SearchOptions { Size = resultSize, ScoringProfile = "region", ScoringParameters = { $"regiontags-{regionToScoreBy}"}};
            }

            SearchResults<SearchDocument> response = client.Search<SearchDocument>(searchQuery, searchOptions);

            var table = new ConsoleTable("Name", "Region", "Accept New Members?");
            foreach (SearchResult<SearchDocument> result in response.GetResults())
            {
                string name = (string)result.Document["Name"];
                string region = (string)result.Document["Region"];
                bool acceptNewMembers = (bool)result.Document["AcceptNewMembers"];

                table.AddRow(name, region, acceptNewMembers);
            }
            
            table.Write();
            Console.WriteLine();
        }
    }
}