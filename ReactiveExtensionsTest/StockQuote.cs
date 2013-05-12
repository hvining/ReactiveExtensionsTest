using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace ReactiveExtensionsTest
{
    public class StockQuote
    {
        public string Symbol { get; set; }
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public long Volume { get; set; }

        public async static Task<IEnumerable<StockQuote>> LoadQuotes()
        {
            return await LoadQuotes(@"Data");
        }

        async static Task<IEnumerable<StockQuote>> LoadQuotes(string path)
        {
            List<StockQuote> quotes = new List<StockQuote>();
            var folder = await Package.Current.InstalledLocation.GetFolderAsync(path);
            var files =  from file in await folder.GetFilesAsync()
                         where Path.GetExtension(file.Name) == ".csv"
                         select file;


            foreach (var file in files)
            {
                quotes.AddRange(await LoadQuotes(file.DisplayName, file));
            }

            return quotes;
        }

        async static Task<IEnumerable<StockQuote>> LoadQuotes(string symbol, IStorageFile path)
        {
            int i = 0;
            IList<StockQuote> quotes = new List<StockQuote>();

            foreach(var line in await FileIO.ReadLinesAsync(path))
            {
                if (i == 0 || String.IsNullOrWhiteSpace(line))
                {
                    i++;
                    continue;
                }

                var elements = line.Split(',');

                var date = DateTime.ParseExact(elements[0], "d-MMM-yy", CultureInfo.InvariantCulture);
                var open = double.Parse(elements[1]);
                var high = double.Parse(elements[2]);
                var low = double.Parse(elements[3]);
                var close = double.Parse(elements[4]);
                var volume = long.Parse(elements[5]);

                var quote = new StockQuote
                {
                    Symbol = symbol,
                    Date = date,
                    Close = close,
                    High = high,
                    Low = low,
                    Open = open,
                    Volume = volume
                };

                quotes.Add(quote);
            }

            return quotes;
        }
    }
}
