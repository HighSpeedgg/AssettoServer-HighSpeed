using AssettoServer.Server;
using Qmmands;
using System;
using System.IO;
using System.Threading.Tasks;
using AssettoServer.Commands.Attributes;
using AssettoServer.Server.Configuration;
using AssettoServer.Server.Weather;
using JetBrains.Annotations;

namespace AssettoServer.Commands.Modules;

[UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
public class GeneralModule : ACModuleBase
{
    private readonly WeatherManager _weatherManager;
    private readonly ACServerConfiguration _configuration;

    private readonly EntryCar.Factory _entryCarFactory;

    public GeneralModule(WeatherManager weatherManager, ACServerConfiguration configuration, EntryCar.Factory entryCarFactory)
    {
        _weatherManager = weatherManager;
        _configuration = configuration;
        _entryCarFactory = entryCarFactory;
    }

    [Command("ping"), RequireConnectedPlayer]
    public void Ping()
        => Reply($"Pong! {Client!.EntryCar.Ping}ms.");

    [Command("changecar"), RequireConnectedPlayer]
    public void ChangeCar()
    {
        // get the current player's entryCar
        var entryCar = Client!.EntryCar;

        // create a new EntryCar for the player
        EntryCar newCar = _entryCarFactory("traffic_mnba_bmw_m5_e34", "White EU", entryCar.SessionId);

        // assign the new car to the client
        Client.EntryCar = newCar;

        // optionally, notify the player
        Reply("Your car has been changed to traffic_mnba_bmw_m5_e34.");
    }

    [Command("time")]
    public void Time()
        => Reply($"It is currently {_weatherManager.CurrentDateTime:H:mm}.");

#if DEBUG
    [Command("test")]
    public ValueTask Test()
    {
        throw new Exception("Test exception");
    }
#endif

    // Do not change the reply, it is used by CSP admin detection
    [Command("admin"), RequireConnectedPlayer]
    public void AdminAsync(string password)
    {
        if (password == _configuration.Server.AdminPassword)
        {
            Client!.IsAdministrator = true;
            Reply("You are now Admin for this server");
        }
        else
            Reply("Command refused");
    }

    [Command("legal")]
    public async Task ShowLegalNotice()
    {
        using var sr = new StringReader(LegalNotice.LegalNoticeText);
        string? line;
        while ((line = await sr.ReadLineAsync()) != null)
        {
            Reply(line);
        }
    }
}
