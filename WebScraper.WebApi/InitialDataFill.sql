USE [ProductWatcher]
GO

BEGIN TRANSACTION 
BEGIN TRY
    INSERT INTO dbo.[SiteSettings] (BaseUrl, AutoGenerateSchedule, MinCheckInterval, CheckInterval)
    VALUES ('https://beru.ru/', 1, '00:30:00', '00:30:00')
    DECLARE @siteSettingsId INT = SCOPE_IDENTITY();

    INSERT INTO dbo.[Sites] ([Name], [SettingsId])
    VALUES ('Beru', @siteSettingsId)
    DECLARE @siteId INT = SCOPE_IDENTITY();

    INSERT INTO dbo.[Products] ([Url], [Scheduler], [SiteId])
    VALUES 
    ('https://beru.ru/product/smartfon-apple-iphone-11-pro-max-64gb-temno-zelenyi-mwhh2ru-a/100773339828?show-uid=15860914804332750068506018&offerid=YHe9t5tqbGcjULhId3P__A',
    '["0 0 0 ? * * *","0 30 1 ? * * *","0 0 3 ? * * *","0 30 4 ? * * *","0 0 6 ? * * *","0 30 7 ? * * *","0 0 9 ? * * *","0 30 10 ? * * *","0 0 12 ? * * *","0 30 13 ? * * *","0 0 15 ? * * *","0 30 16 ? * * *","0 0 18 ? * * *","0 30 19 ? * * *","0 0 21 ? * * *","0 30 22 ? * * *"]',
    @siteId)

    COMMIT
END TRY
BEGIN CATCH
    PRINT('Ошибка при добавлении данных')
    ROLLBACK
END CATCH