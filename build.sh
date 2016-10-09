#!/bin/sh
# rm -rf ~/solutions/DotnetSpider/spider_nuget_packages
dotnet restore
dotnet build src/DotnetSpider.Extension/project.json
dotnet pack src/DotnetSpider.HtmlAgilityPack/project.json -o spider_nuget_packages --no-build
dotnet pack src/DotnetSpider.HtmlAgilityPack.Css/project.json -o spider_nuget_packages --no-build
dotnet pack src/DotnetSpider.Core/project.json -o spider_nuget_packages --no-build
dotnet pack src/DotnetSpider.Redial/project.json -o spider_nuget_packages --no-build
dotnet pack src/DotnetSpider.Extension/project.json -o spider_nuget_packages --no-build
