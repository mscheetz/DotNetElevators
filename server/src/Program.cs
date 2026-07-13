// See https://aka.ms/new-console-template for more information
using System.Security.Cryptography;
using DotNetElevators;

Console.WriteLine(@"
      ___                         ___                        ___                                   ___           ___                    ___           ___           ___           ___           ___     
     /  /\                       /  /\          ___         /  /\          ___       ___          /  /\         /__/\                  /__/\         /  /\         /__/\         /  /\         /  /\    
    /  /:/_                     /  /:/_        /__/\       /  /::\        /  /\     /  /\        /  /::\        \  \:\                 \  \:\       /  /::\       |  |::\       /  /:/_       /  /:/_   
   /  /:/ /\    ___     ___    /  /:/ /\       \  \:\     /  /:/\:\      /  /:/    /  /:/       /  /:/\:\        \  \:\                 \__\:\     /  /:/\:\      |  |:|:\     /  /:/ /\     /  /:/ /\  
  /  /:/ /:/_  /__/\   /  /\  /  /:/ /:/_       \  \:\   /  /:/~/::\    /  /:/    /__/::\      /  /:/  \:\   _____\__\:\            ___ /  /::\   /  /:/  \:\   __|__|:|\:\   /  /:/ /:/_   /  /:/ /::\ 
 /__/:/ /:/ /\ \  \:\ /  /:/ /__/:/ /:/ /\  ___  \__\:\ /__/:/ /:/\:\  /  /::\    \__\/\:\__  /__/:/ \__\:\ /__/::::::::\          /__/\  /:/\:\ /__/:/ \__\:\ /__/::::| \:\ /__/:/ /:/ /\ /__/:/ /:/\:\
 \  \:\/:/ /:/  \  \:\  /:/  \  \:\/:/ /:/ /__/\ |  |:| \  \:\/:/__\/ /__/:/\:\      \  \:\/\ \  \:\ /  /:/ \  \:\~~\~~\/          \  \:\/:/__\/ \  \:\ /  /:/ \  \:\~~\__\/ \  \:\/:/ /:/ \  \:\/:/~/:/
  \  \::/ /:/    \  \:\/:/    \  \::/ /:/  \  \:\|  |:|  \  \::/      \__\/  \:\      \__\::/  \  \:\  /:/   \  \:\  ~~~            \  \::/       \  \:\  /:/   \  \:\        \  \::/ /:/   \  \::/ /:/ 
   \  \:\/:/      \  \::/      \  \:\/:/    \  \:\__|:|   \  \:\           \  \:\     /__/:/    \  \:\/:/     \  \:\                 \  \:\        \  \:\/:/     \  \:\        \  \:\/:/     \__\/ /:/  
    \  \::/        \__\/        \  \::/      \__\::::/     \  \:\           \__\/     \__\/      \  \::/       \  \:\                 \  \:\        \  \::/       \  \:\        \  \::/        /__/:/   
     \__\/                       \__\/           ~~~~       \__\/                                 \__\/         \__\/                  \__\/         \__\/         \__\/         \__\/         \__\/    
");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BuildingService>();
builder.Services.AddSingleton<QueueManager>();
builder.Services.AddSingleton<PassengerService>();
builder.Services.AddSingleton<BuildingBroadcastService>();

builder.Services.AddHostedService<ElevatorManagementService>();
builder.Services.AddHostedService<PassengerTimer>();
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddCors(opts =>
{
   opts.AddPolicy("Client", pol =>
   {
      pol.WithOrigins("http://localhost:5000")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials();
   });
});

using var app = builder.Build();

app.UseCors("Client");

app.MapControllers();
app.MapHub<BuildingHub>("/hubs/building");

await app.RunAsync();