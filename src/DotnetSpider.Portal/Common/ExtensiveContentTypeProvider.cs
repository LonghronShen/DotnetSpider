using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.StaticFiles;

namespace DotnetSpider.Portal.Common
{
    public class ExtensiveContentTypeProvider
        : FileExtensionContentTypeProvider
    {
        public ExtensiveContentTypeProvider(IDictionary<string, string> extra_mappings)
            : base()
        {
            foreach (var item in extra_mappings)
            {
                if (!this.Mappings.TryAdd(item.Key, item.Value))
                {
                    this.Mappings[item.Key] = item.Value;
                }
            }
        }
    }
}
