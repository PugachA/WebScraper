
  INSERT INTO [ProductWatcher].[dbo].[SiteSettings] (AutoGenerateSchedule, CheckInterval, MinCheckInterval, HtmlLoader)
  VALUES(0, N'00:01:00', N'00:01:00', N'HttpLoader')

  INSERT INTO [ProductWatcher].[dbo].[Sites] (BaseUrl, Name, SettingsId)
  VALUES(N'https://auto.youla.ru/', N'Youla.Auto', 10)

  INSERT INTO [ProductWatcher].[dbo].[SiteSettings] (AutoGenerateSchedule, CheckInterval, MinCheckInterval, HtmlLoader)
  VALUES(0, N'00:01:00', N'00:01:00', N'PuppeteerLoader')

  INSERT INTO [ProductWatcher].[dbo].[Sites] (BaseUrl, Name, SettingsId)
  VALUES(N'https://youla.ru/', N'Youla', 11)

  INSERT INTO [ProductWatcher].[dbo].[SiteSettings] (AutoGenerateSchedule, CheckInterval, MinCheckInterval, HtmlLoader)
  VALUES(0, N'00:01:00', N'00:01:00', N'HttpLoader')

  INSERT INTO [ProductWatcher].[dbo].[Sites] (BaseUrl, Name, SettingsId)
  VALUES(N'https://www.avito.ru/', N'Avito', 12)

  INSERT INTO [ProductWatcher].[dbo].[SiteSettings] (AutoGenerateSchedule, CheckInterval, MinCheckInterval, HtmlLoader)
  VALUES(0, N'00:01:00', N'00:01:00', N'HttpLoader')

  INSERT INTO [ProductWatcher].[dbo].[Sites] (BaseUrl, Name, SettingsId)
  VALUES(N'https://auto.ru/', N'Auto.ru', 13)
