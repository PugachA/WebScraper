DECLARE @SiteSettingsId INT

INSERT INTO dbo.SiteSettings (AutoGenerateSchedule, MinCheckInterval, CheckInterval, HtmlLoader, PriceParser)
VALUES (0, N'00:01:00', N'00:01:00', N'HttpLoader', N'HtmlPriceParser');

SELECT @SiteSettingsId = SCOPE_IDENTITY();

INSERT INTO dbo.Sites ([Name], BaseUrl, SettingsId)
VALUES (N'Autospot', N'https://autospot.ru/', @SiteSettingsId);