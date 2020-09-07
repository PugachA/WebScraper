﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using WebScraper.Core.Parsers;
using WebScraper.Data.Models;

namespace WebScraper.Core.Factories
{
    public class PriceParserFactory : IFactory<IPriceParser>
    {
        private readonly ILogger<PriceParserFactory> _logger;
        private readonly IConfiguration _configuration;

        public PriceParserFactory(IConfiguration configuration, ILogger<PriceParserFactory> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IPriceParser Get(Site site)
        {
            var parserSettings = _configuration.GetSection(site.Name).Get<ParserSettings>();

            if (parserSettings == null)
                throw new ArgumentException($"Не удалось найти настройки {nameof(ParserSettings)} в конфигурации для сайта {site.Name}");

            return new PriceParser(parserSettings, _logger);
        }
    }
}