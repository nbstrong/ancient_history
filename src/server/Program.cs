using WurmStyleGame.Server.Hosting;

WebApplication app = ServerApplication.Create(args);
await app.RunAsync();
