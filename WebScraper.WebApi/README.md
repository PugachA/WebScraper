## Migrations
### Для создания миграции необходимо выполнить команду:
```
dotnet ef migrations add <Name> --project .\WebScraper.Data --startup-project .\WebScraper.WebApi
```
### Для отката миграции:
```
dotnet ef database update <previous-migration-name> --project .\WebScraper.Data --startup-project .\WebScraper.WebApi
```
### Для удаления миграции:
```
dotnet ef migrations remove --project .\WebScraper.Data --startup-project .\WebScraper.WebApi
```
[Статья](https://rajbos.github.io/blog/2020/04/23/EntityFramework-Core-NET-Standard-Migrations)